using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// A <see cref="BitStream"/> over a series of tapes.
	/// </summary>
	/// <remarks>
	/// Append mode writes forward from the series tail and rolls onto a new tape when the current
	/// one is full. The partial byte lives here, so a dispose of an unaligned write records only
	/// the meaningful bits, and alignment is a full zero-padded byte committed on the next write.
	/// That is the same tape layout <c>CodeWriter</c> produced when it pushed its partial byte
	/// into <c>TapeSeriesStream</c>.
	///
	/// Read mode walks the series from a starting bit offset. <see cref="BitStream.Position"/>
	/// still starts at 0: the offset is where the underlying series is positioned, and this wrapper
	/// counts bits read from there. It cannot seek.
	/// </remarks>
	public sealed class TapeSeriesBitStream : BitStream
	{
		#region nested types
		/// <summary>
		/// Which direction a series stream is opened for. One instance is never both.
		/// </summary>
		public enum OpenMode
		{
			/// <summary>Read the series from a starting bit offset.</summary>
			Read,

			/// <summary>Append to the series tail.</summary>
			Append
		}

		private sealed class TapeCursor
		{
			public Tape Tape { get; }
			public int ContentBitLength { get; set; }
			public int BitPosition { get; set; }

			public TapeCursor(Tape tape, int contentBitLength, int bitPosition)
			{
				Tape = tape ?? throw new ArgumentNullException(nameof(tape));
				ContentBitLength = contentBitLength;
				BitPosition = bitPosition;
			}
		}

		/// <summary>
		/// Raised after the stream rolls from one tape to the next.
		/// </summary>
		public sealed class RolloverEventArgs : EventArgs
		{
			/// <summary>The tape that filled.</summary>
			public Tape FinishedTape { get; }

			/// <summary>The tape now being written.</summary>
			public Tape NewTape { get; }

			/// <summary>
			/// Creates the event.
			/// </summary>
			public RolloverEventArgs(Tape finishedTape, Tape newTape)
			{
				FinishedTape = finishedTape ?? throw new ArgumentNullException(nameof(finishedTape));
				NewTape = newTape ?? throw new ArgumentNullException(nameof(newTape));
			}
		}
		#endregion

		#region fields
		private readonly Librarian _librarian;
		private readonly Guid _seriesId;
		private readonly OpenMode _mode;

		private readonly List<Tape> _orderedTapes;
		private readonly List<int> _tapeContentLengths;
		private int _currentIndex;
		private TapeCursor? _current;
		private long _totalSeriesBits;

		private byte _buffer;
		private int _bufferBits;
		private bool _hasPending;
		private byte _pendingByte;
		private int _pendingBits;

		private bool _hasRolloverPending;
		#endregion

		#region events
		/// <summary>
		/// Raised after the stream rolls from the finished tape to a new current tape.
		/// </summary>
		public event EventHandler<RolloverEventArgs>? CurrentTapeChanged;

		/// <summary>
		/// Raised when this wrapper is disposed, including when disposal throws.
		/// </summary>
		internal event EventHandler? Closed;
		#endregion

		#region constructors
		/// <summary>
		/// Opens the series at the start (read) or the tail (append).
		/// </summary>
		public TapeSeriesBitStream(Librarian librarian, Guid seriesId, OpenMode mode)
			: this(librarian, seriesId, mode, 0)
		{
		}

		/// <summary>
		/// Opens the series. <paramref name="startBitOffset"/> is the read cursor, and must be 0
		/// when appending. <see cref="BitStream.Position"/> still starts at 0.
		/// </summary>
		public TapeSeriesBitStream(Librarian librarian, Guid seriesId, OpenMode mode, long startBitOffset)
		{
			if (librarian == null)
				throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));

			_librarian = librarian;
			_seriesId = seriesId;
			_mode = mode;

			_orderedTapes = new List<Tape>(_librarian.FetchTapesInSeries(seriesId) ?? Array.Empty<Tape>());
			_tapeContentLengths = new List<int>(_orderedTapes.Count);
			if (_orderedTapes.Count == 0)
				throw new InvalidOperationException("No tapes found in the requested series.");

			switch (mode)
			{
				case OpenMode.Read:
					InitializeForRead(startBitOffset);
					break;

				case OpenMode.Append:
					if (startBitOffset != 0)
						throw new ArgumentException("An append stream starts at the series tail.", nameof(startBitOffset));

					InitializeForAppend();
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(mode));
			}
		}
		#endregion

		#region BitStream properties
		/// <inheritdoc/>
		public override bool CanRead
		{
			get
			{
				return !IsDisposed && _mode == OpenMode.Read;
			}
		}

		/// <inheritdoc/>
		public override bool CanWrite
		{
			get
			{
				return !IsDisposed && _mode == OpenMode.Append;
			}
		}

		/// <inheritdoc/>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region BitStream methods
		/// <inheritdoc/>
		public override Code ReadBits(int bitCount)
		{
			PrepareRead(bitCount);
			if (bitCount == 0)
				return new Code(0);

			Code code = null!;
			RunIo(
				delegate()
				{
					code = ReadFromTapes(bitCount);
					AdvancePosition(bitCount);
				}
			);

			return code;
		}

		/// <inheritdoc/>
		public override void WriteBits(Code code)
		{
			PrepareWrite(code);
			if (code.Length == 0)
				return;

			RunIo(
				delegate()
				{
					if (_hasPending)
						CommitPendingAsFullByte();

					if (_bufferBits == 0)
					{
						int fullBytes = code.Length / 8;
						int remainder = code.Length % 8;
						if (fullBytes > 0)
							WriteAlignedBytes(code, 0, fullBytes);

						if (remainder != 0)
							BufferBits(code, fullBytes * 8, remainder);
					}
					else
					{
						BufferBits(code, 0, code.Length);
					}

					AdvancePosition(code.Length);
				}
			);
		}

		/// <inheritdoc/>
		public override void AlignToNextByteBoundary()
		{
			PrepareOperation();
			if (_mode == OpenMode.Read)
			{
				// Crossing onto the next tape reads that tape's label. A failure there faults
				// the reader, the same way a failed ReadBits does, so a later call cannot resume
				// from a cursor that already moved.
				RunIo(
					delegate()
					{
						AlignRead();
					}
				);
			}
			else
			{
				AlignWrite();
			}
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			// A partial byte stays buffered. Complete bytes are already on the tape.
			base.Flush();
		}
		#endregion

		#region BitStream methods (dispose)
		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			try
			{
				if (!IsFaulted && _mode == OpenMode.Append)
				{
					try
					{
						if (_hasPending)
							CommitBits(_pendingByte, _pendingBits);
						else if (_bufferBits != 0)
							CommitBits(_buffer, _bufferBits);

						_hasPending = false;
						_buffer = 0;
						_bufferBits = 0;

						if (_current != null)
							PersistTapePosition();
					}
					catch (Exception exception)
					{
						Fault(exception);
						throw;
					}
				}
			}
			finally
			{
				Closed?.Invoke(this, EventArgs.Empty);
			}
		}
		#endregion

		#region private methods
		private void InitializeForRead(long startBitOffset)
		{
			_currentIndex = 0;
			_tapeContentLengths.Clear();
			_current = MakeReadCursor(_orderedTapes[_currentIndex]);
			_totalSeriesBits = 0;

			for (int i = 0; i < _orderedTapes.Count; i++)
			{
				int tapeBits;
				if (i == _currentIndex)
					tapeBits = _current.ContentBitLength;
				else
					tapeBits = GetTapeContentBitLength(_orderedTapes[i]);

				_tapeContentLengths.Add(tapeBits);
				_totalSeriesBits += tapeBits;
			}

			if (startBitOffset < 0 || startBitOffset > _totalSeriesBits)
				throw new ArgumentOutOfRangeException(nameof(startBitOffset), "Bit offset is out of range for the series.");

			PlaceReadCursor(startBitOffset);
		}

		private void InitializeForAppend()
		{
			_currentIndex = _orderedTapes.Count - 1;
			Tape last = _orderedTapes[_currentIndex];
			int posBits = last.Position;
			_current = new TapeCursor(last, posBits, posBits);
			_hasPending = false;
			_pendingByte = 0;
			_pendingBits = 0;
			_buffer = 0;
			_bufferBits = 0;
		}

		private static TapeCursor MakeReadCursor(Tape tape)
		{
			int contentBits = GetTapeContentBitLength(tape);
			return new TapeCursor(tape, contentBits, 0);
		}

		private static int GetTapeContentBitLength(Tape tape)
		{
			Debug.Assert(tape != null, "Tape cannot be null when calculating content length.");

			TapeLabel label = tape.ReadLabel();
			int contentLength;
			if (!label.TryGetContentLength(out contentLength))
				contentLength = label.TapePosition;

			return contentLength;
		}

		private void PlaceReadCursor(long bitOffset)
		{
			long running = 0;
			int index = 0;
			while (index < _orderedTapes.Count)
			{
				int tapeBits = _tapeContentLengths[index];
				if (bitOffset <= running + tapeBits)
					break;

				running += tapeBits;
				index++;
			}

			if (index >= _orderedTapes.Count)
			{
				index = _orderedTapes.Count - 1;
				running = _totalSeriesBits - _tapeContentLengths[index];
				bitOffset = _totalSeriesBits;
			}

			_currentIndex = index;
			_current = MakeReadCursor(_orderedTapes[index]);
			_current.BitPosition = (int)(bitOffset - running);
		}

		private Code ReadFromTapes(int bitCount)
		{
			CodeBuilder builder = new CodeBuilder(bitCount);
			int filled = 0;
			while (filled < bitCount)
			{
				if (!EnsureReadableTapeAvailable())
					throw new EndOfStreamException();

				Debug.Assert(_current != null);
				int available = _current.ContentBitLength - _current.BitPosition;
				int count = bitCount - filled;
				if (count > available)
					count = available;

				Code chunk = TapeHelper.ReadContent(_current.Tape, _current.BitPosition, count);
				for (int i = 0; i < count; ++i)
					builder[filled + i] = chunk[i];

				_current.BitPosition += count;
				filled += count;
			}

			return builder.Build();
		}

		private void AlignRead()
		{
			int offset = (int)(Position % 8);
			if (offset == 0)
				return;

			int skip = 8 - offset;
			int left = skip;
			while (left > 0)
			{
				if (_current == null)
					break;

				int available = _current.ContentBitLength - _current.BitPosition;
				if (available <= 0)
				{
					if (!AdvanceReadTape())
						break;

					continue;
				}

				int count = left < available ? left : available;
				_current.BitPosition += count;
				left -= count;
			}

			AdvancePosition(skip);
		}

		private void AlignWrite()
		{
			if (_bufferBits == 0)
				return;

			_pendingByte = _buffer;
			_pendingBits = _bufferBits;
			_hasPending = true;
			int padding = 8 - _bufferBits;
			_buffer = 0;
			_bufferBits = 0;
			AdvancePosition(padding);
		}

		private void BufferBits(Code code, int offset, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				if (code[offset + i] == 1)
					_buffer |= (byte)(1 << _bufferBits);

				++_bufferBits;
				if (_bufferBits == 8)
				{
					CommitBits(_buffer, 8);
					_buffer = 0;
					_bufferBits = 0;
				}
			}
		}

		private void WriteAlignedBytes(Code code, int byteOffset, int byteCount)
		{
			int index = 0;
			while (index < byteCount)
			{
				EnsureWritableTape();
				Debug.Assert(_current != null);
				int capacityBits = Tape.MaxContentLength - _current.BitPosition;
				if (capacityBits <= 0)
				{
					RolloverAppendTape();
					continue;
				}

				if (capacityBits < 8)
				{
					CommitBits(code.GetByte(byteOffset + index), 8);
					++index;
					continue;
				}

				int capacityBytes = capacityBits / 8;
				int count = byteCount - index;
				if (count > capacityBytes)
					count = capacityBytes;

				byte[] slice = new byte[count];
				for (int i = 0; i < count; ++i)
					slice[i] = code.GetByte(byteOffset + index + i);

				Code chunk = new Code(slice);
				TapeHelper.WriteContent(_current.Tape, chunk, _current.BitPosition, 0, count * 8);
				_current.BitPosition += count * 8;
				PersistTapePosition();
				index += count;
			}
		}

		private void CommitPendingAsFullByte()
		{
			CommitBits(_pendingByte, 8);
			_hasPending = false;
			_pendingByte = 0;
			_pendingBits = 0;
		}

		private void CommitBits(byte value, int bitCount)
		{
			Debug.Assert(bitCount > 0);
			Debug.Assert(bitCount <= 8);

			int remaining = bitCount;
			byte bits = value;
			while (remaining > 0)
			{
				EnsureWritableTape();
				Debug.Assert(_current != null);
				int capacity = Tape.MaxContentLength - _current.BitPosition;
				if (capacity <= 0)
				{
					RolloverAppendTape();
					continue;
				}

				int count = remaining < capacity ? remaining : capacity;
				Code partial = UnpackPartialByteToCode(bits, count);
				TapeHelper.WriteContent(_current.Tape, partial, _current.BitPosition, 0, count);
				_current.BitPosition += count;
				PersistTapePosition();
				bits = (byte)(bits >> count);
				remaining -= count;
				if (remaining > 0)
					RolloverAppendTape();
			}
		}

		private bool EnsureReadableTapeAvailable()
		{
			if (_current != null && _current.BitPosition < _current.ContentBitLength)
				return true;

			return AdvanceReadTape();
		}

		private bool AdvanceReadTape()
		{
			if (_currentIndex + 1 >= _orderedTapes.Count)
				return false;

			_currentIndex++;
			_current = MakeReadCursor(_orderedTapes[_currentIndex]);
			return _current.ContentBitLength > 0;
		}

		private void EnsureWritableTape()
		{
			if (_current != null && _currentIndex >= 0 && _currentIndex < _orderedTapes.Count)
				return;

			throw new InvalidOperationException("No writable tape is available.");
		}

		private void RolloverAppendTape()
		{
			if (_current == null)
				throw new InvalidOperationException("_current is null. This should NEVER happen.");

			Tape finished = _current.Tape;

			Guid newId = Guid.NewGuid();
			Tape newTape = Tape.CreateNew(_librarian.TapeProvider, newId);

			TapeLabel prior = _orderedTapes[_orderedTapes.Count - 1].ReadLabel();
			TapeLabel label = newTape.ReadLabel();

			label = label.WithSeriesInfo(prior.SeriesId, prior.SeriesName, prior.SeriesNumber + 1);
			label = label.WithName(prior.Name);

			string? description;
			if (prior.TryGetSeriesDescription(out description))
				label = label.WithSeriesDescription(description);

			newTape.WriteLabel(label);

			_orderedTapes.Add(newTape);
			_currentIndex = _orderedTapes.Count - 1;
			_current = new TapeCursor(newTape, 0, 0);
			_hasRolloverPending = true;

			CurrentTapeChanged?.Invoke(this, new RolloverEventArgs(finished, newTape));
		}

		private void PersistTapePosition()
		{
			if (_current == null)
				throw new InvalidOperationException("_current is null. This should NEVER happen.");

			_current.Tape.Position = _current.BitPosition;
			_current.ContentBitLength = _current.BitPosition;
		}

		private static Code UnpackPartialByteToCode(byte value, int bitCount)
		{
			if (bitCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(bitCount));

			CodeBuilder builder = new CodeBuilder(bitCount);
			for (int i = 0; i < bitCount; ++i)
			{
				int bit = (value >> i) & 0x01;
				builder[i] = bit;
			}

			return builder.Build();
		}
		#endregion

		#region internal methods
		/// <summary>
		/// Reports whether a rollover has happened since the last call, and clears the flag.
		/// </summary>
		[Obsolete("Use CurrentTapeChanged event instead.")]
		internal bool TryConsumeRolloverFlag()
		{
			if (!_hasRolloverPending)
				return false;

			_hasRolloverPending = false;
			return true;
		}
		#endregion
	}
}
