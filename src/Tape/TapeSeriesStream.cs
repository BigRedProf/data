using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// A non-seekable stream over a series of tapes.
	/// - Read mode: sequentially reads content across tapes (based on each tape's Position in bits).
	/// - Append mode: appends content at series tail; auto-rolls to a new tape when full.
	/// Designed to be used with CodeReader/CodeWriter.
	/// </summary>
	public sealed class TapeSeriesStream : BitAwareStream
	{
		#region nested types
		public enum OpenMode
		{
			Read,
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

		public sealed class RolloverEventArgs : EventArgs
		{
			public Tape FinishedTape { get; }
			public Tape NewTape { get; }

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
		private TapeCursor _current;
		private long _totalSeriesBits;

		// Pending partial-byte flush buffering (to disambiguate Align vs Dispose semantics)
		private bool _hasPendingPartial;
		private byte _pendingByte;
		private int _pendingBits; // 1..7
		private bool _hasRolloverPending;

		private bool _isDisposed;
		#endregion

		#region events
		/// <summary>
		/// Raised after the stream rolls from the finished tape to a new current tape.
		/// </summary>
		public event EventHandler<RolloverEventArgs> CurrentTapeChanged;
		#endregion

		#region construction
		public TapeSeriesStream(Librarian librarian, Guid seriesId, OpenMode mode)
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
					InitializeForRead();
					break;

				case OpenMode.Append:
					InitializeForAppend();
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(mode));
			}
		}
		#endregion

		#region initialization
		private void InitializeForRead()
		{
			_currentIndex = 0;
			_tapeContentLengths.Clear();
			_current = MakeReadCursor(_orderedTapes[_currentIndex]);
			_totalSeriesBits = 0;

			for (int i = 0; i < _orderedTapes.Count; i++)
			{
				int tapeBits;
				if (i == _currentIndex)
				{
					tapeBits = _current.ContentBitLength;
				}
				else
				{
					tapeBits = GetTapeContentBitLength(_orderedTapes[i]);
				}

				_tapeContentLengths.Add(tapeBits);
				_totalSeriesBits += tapeBits;
			}

			CurrentByte = 0;
			OffsetIntoCurrentByte = 0;

			_hasPendingPartial = false;
			_pendingByte = 0;
			_pendingBits = 0;
		}

		private void InitializeForAppend()
		{
			_currentIndex = _orderedTapes.Count - 1;

			Tape last = _orderedTapes[_currentIndex];
			int posBits = last.Position;

			_current = new TapeCursor(last, posBits, posBits);

			CurrentByte = 0;
			OffsetIntoCurrentByte = 0;

			_hasPendingPartial = false;
			_pendingByte = 0;
			_pendingBits = 0;
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
		#endregion

		#region Stream capability properties
		public override bool CanRead => _mode == OpenMode.Read;
		public override bool CanSeek => true;
		public override bool CanWrite => _mode == OpenMode.Append;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}
		#endregion

		#region Read
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(TapeSeriesStream));
			if (!CanRead)
				throw new NotSupportedException("Stream not opened for reading.");
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException("Invalid buffer range.");

			int totalRead = 0;

			while (count > 0)
			{
				if (!EnsureReadableTapeAvailable())
					break;

				int bitsRemaining = _current.ContentBitLength - _current.BitPosition;
				if (bitsRemaining <= 0)
				{
					AdvanceReadTape();
					continue;
				}

				int maxBytesFromBits = bitsRemaining / 8;
				bool hasPartialTail = (bitsRemaining % 8) != 0;

				int bytesToRead = Math.Min(count, maxBytesFromBits);
				if (bytesToRead > 0)
				{
					Code code = TapeHelper.ReadContent(_current.Tape, _current.BitPosition, bytesToRead * 8);
					CopyCodeToBuffer(code, buffer, offset, bytesToRead);

					offset += bytesToRead;
					count -= bytesToRead;
					totalRead += bytesToRead;
					_current.BitPosition += bytesToRead * 8;

					continue;
				}

				if (hasPartialTail)
				{
					if (count == 0)
						break;

					int bitsToRead = bitsRemaining;
					Code code = TapeHelper.ReadContent(_current.Tape, _current.BitPosition, bitsToRead);
					buffer[offset] = PackPartialByte(code, 0, bitsToRead);

					offset += 1;
					count -= 1;
					totalRead += 1;
					_current.BitPosition += bitsToRead;

					AdvanceReadTape();
					continue;
				}

				AdvanceReadTape();
			}

			return totalRead;
		}
		#endregion

		#region Write
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(TapeSeriesStream));
			if (!CanWrite)
				throw new NotSupportedException("Stream not opened for writing.");
			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));
			if (offset < 0 || count < 0 || offset + count > buffer.Length)
				throw new ArgumentOutOfRangeException("Invalid buffer range.");

			while (count > 0)
			{
				EnsureWritableTape();

				// If we have a pending partial byte and new data arrives, treat the pending as ALIGN:
				// commit as a FULL BYTE (pad zeros) before proceeding.
				if (_hasPendingPartial)
				{
					CommitPendingAsFullByte();
				}

				int capacityBits = Tape.MaxContentLength - _current.BitPosition;
				if (capacityBits <= 0)
				{
					RolloverAppendTape();
					continue;
				}

				int capacityBytes = capacityBits / 8;
				int bytesToWrite = Math.Min(count, capacityBytes);

				// Detect CodeWriter single-byte *partial* flush (1..7 bits).
				bool isSingleByte = bytesToWrite == 1;
				bool writerFlushingPartial = OffsetIntoCurrentByte > 0 && OffsetIntoCurrentByte < 8;

				if (isSingleByte && writerFlushingPartial)
				{
					// Buffer; don't advance tape yet.
					// Next Write() => treat as Align and commit as FULL BYTE.
					// Dispose() with no further writes => commit only meaningful bits.
					_pendingByte = buffer[offset];
					_pendingBits = OffsetIntoCurrentByte;
					_hasPendingPartial = true;

					offset += 1;
					count -= 1;
					continue;
				}

				if (bytesToWrite > 0)
				{
					byte[] slice = new byte[bytesToWrite];
					Array.Copy(buffer, offset, slice, 0, bytesToWrite);

					Code code = new Code(slice);
					TapeHelper.WriteContent(_current.Tape, code, _current.BitPosition, 0, bytesToWrite * 8);

					offset += bytesToWrite;
					count -= bytesToWrite;

					_current.BitPosition += bytesToWrite * 8;
					PersistTapePosition();

					continue;
				}

				// No whole bytes fit, but we still have bits to write.
				if (capacityBits > 0 && count > 0)
				{
					byte current = buffer[offset];
					int bitsToWrite = capacityBits;

					Code partial = UnpackPartialByteToCode(current, bitsToWrite);
					TapeHelper.WriteContent(_current.Tape, partial, _current.BitPosition, 0, bitsToWrite);

					// Reflect current state for any readers (optional).
					CurrentByte = current;
					OffsetIntoCurrentByte = bitsToWrite;

					_current.BitPosition += bitsToWrite;
					PersistTapePosition();

					offset += 1;
					count -= 1;

					int remainingBits = 8 - bitsToWrite;
					if (remainingBits > 0)
					{
						byte remainder = (byte)(current >> bitsToWrite);

						Tape finished = _current.Tape;
						RolloverAppendTape(); // raises event after we switch

						Code remainderCode = UnpackPartialByteToCode(remainder, remainingBits);
						TapeHelper.WriteContent(_current.Tape, remainderCode, _current.BitPosition, 0, remainingBits);

						CurrentByte = remainder;
						OffsetIntoCurrentByte = remainingBits;

						_current.BitPosition += remainingBits;
						PersistTapePosition();
					}

					continue;
				}

				RolloverAppendTape();
			}
		}

		public override void Flush()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(TapeSeriesStream));
			// No internal buffering to flush here; label updates happen via PersistTapePosition().
			// Note: We intentionally do NOT auto-commit pending partial on Flush(); semantics are:
			// - Next Write => commit pending as full byte (align).
			// - Dispose    => commit pending as meaningful bits.
		}
		#endregion

		#region Seek/SetLength not supported
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanSeek)
				throw new NotSupportedException("Seeking is only supported in Read mode.");
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(TapeSeriesStream));

			long target;
			switch (origin)
			{
				case SeekOrigin.Begin:
					target = offset;
					break;
				case SeekOrigin.Current:
				{
					// current absolute bit position = sum(bits of earlier tapes) _current.BitPosition
					long prefix = 0;
					for (int i = 0; i < _currentIndex; i++)
						prefix += _tapeContentLengths[i];
					target = prefix + _current.BitPosition + offset;
				}
				break;
				case SeekOrigin.End:
					target = (_totalSeriesBits) + offset; // offset may be negative
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(origin));
			}

			if (target < 0 || target > _totalSeriesBits)
				throw new ArgumentOutOfRangeException(nameof(offset), "Bit offset is out of range for the series.");

			// Find the tape that contains 'target' and set per-tape cursor
			long running = 0;
			int idx = 0;
			while (idx < _orderedTapes.Count)
			{
				int tapeBits = _tapeContentLengths[idx];
				if (target <= running + tapeBits) break;
				running += tapeBits;
				idx++;
			}

			if (idx >= _orderedTapes.Count)
			{
				// target == _totalSeriesBits: position to logical EOF (at last tape's end)
				idx = _orderedTapes.Count - 1;
				running -= _tapeContentLengths[idx]; // back to start of last tape
				target = running + _tapeContentLengths[idx];
			}

			_currentIndex = idx;
			_current = MakeReadCursor(_orderedTapes[idx]);
			_current.BitPosition = (int)(target - running); // 0..tapeBits

			// clear partial-byte state; reading resumes cleanly at this bit
			CurrentByte = 0;
			OffsetIntoCurrentByte = 0;

			return target; // return absolute bit position
		}

		public override void SetLength(long value) => throw new NotSupportedException();
		#endregion

		#region disposal
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				// If we have a pending partial and no more writes are coming,
				// this was a Dispose-driven flush: commit only the *meaningful* bits.
				if (CanWrite && _hasPendingPartial)
					CommitPendingAsMeaningfulBits();

				// Ensure position is persisted one last time on dispose in append mode.
				if (CanWrite && _current != null)
					PersistTapePosition();
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
		#endregion

		#region helpers (read)
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
		#endregion

		#region helpers (write)
		private void EnsureWritableTape()
		{
			if (_current != null && _currentIndex >= 0 && _currentIndex < _orderedTapes.Count)
				return;

			throw new InvalidOperationException("No writable tape is available.");
		}

		private void RolloverAppendTape()
		{
			Tape finished = _current?.Tape;

			Guid newId = Guid.NewGuid();
			Tape newTape = Tape.CreateNew(_librarian.TapeProvider, newId);

			TapeLabel prior = _orderedTapes[_orderedTapes.Count - 1].ReadLabel();
			TapeLabel label = newTape.ReadLabel();

			label = label.WithSeriesInfo(prior.SeriesId, prior.SeriesName, prior.SeriesNumber + 1);
			label = label.WithName(prior.Name);

			string desc;
			if (prior.TryGetSeriesDescription(out desc))
				label = label.WithSeriesDescription(desc);

			newTape.WriteLabel(label);

			_orderedTapes.Add(newTape);
			_currentIndex = _orderedTapes.Count - 1;
			_current = new TapeCursor(newTape, 0, 0);

			CurrentByte = 0;
			OffsetIntoCurrentByte = 0;

			_hasRolloverPending = true;

			// Notify listeners that we moved to a new tape.
			CurrentTapeChanged?.Invoke(this, new RolloverEventArgs(finished, newTape));
		}

		private void PersistTapePosition()
		{
			_current.Tape.Position = _current.BitPosition;
			_current.ContentBitLength = _current.BitPosition;
		}
		#endregion

		#region internal methods
		[Obsolete("Use CurrentTapeChanged event instead.")]
		internal bool TryConsumeRolloverFlag()
		{
			if (!_hasRolloverPending)
				return false;

			_hasRolloverPending = false;
			return true;
		}
		#endregion

		#region helpers (pending commit)
		private void CommitPendingAsFullByte()
		{
			if (!_hasPendingPartial)
				return;

			Code full = new Code(new byte[] { _pendingByte });
			TapeHelper.WriteContent(_current.Tape, full, _current.BitPosition, 0, 8);

			_current.BitPosition += 8;
			PersistTapePosition();

			_hasPendingPartial = false;
			_pendingByte = 0;
			_pendingBits = 0;
		}

		private void CommitPendingAsMeaningfulBits()
		{
			if (!_hasPendingPartial)
				return;

			Code partial = UnpackPartialByteToCode(_pendingByte, _pendingBits);
			TapeHelper.WriteContent(_current.Tape, partial, _current.BitPosition, 0, _pendingBits);

			_current.BitPosition += _pendingBits;
			PersistTapePosition();

			_hasPendingPartial = false;
			_pendingByte = 0;
			_pendingBits = 0;
		}
		#endregion

		#region helpers (byte/bit pack/unpack)
		private static void CopyCodeToBuffer(Code code, byte[] buffer, int offset, int byteCount)
		{
			byte[] bytes = code.ToByteArray();
			Array.Copy(bytes, 0, buffer, offset, byteCount);
		}

		private static byte PackPartialByte(Code bits, int bitOffset, int bitCount)
		{
			int value = 0;
			for (int i = 0; i < bitCount; ++i)
			{
				int bit = bits[bitOffset + i];
				value |= (bit & 0x01) << i;
			}
			return (byte)value;
		}

		private static Code UnpackPartialByteToCode(byte b, int bitCount)
		{
			if (bitCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(bitCount));

			Code code = new Code(bitCount);
			for (int i = 0; i < bitCount; ++i)
			{
				int bit = (b >> i) & 0x01;
				code[i] = bit;
			}
			return code;
		}
		#endregion
	}
}
