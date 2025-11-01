// File: TapeSeriesStream.cs
using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Collections.Generic;
using System.IO;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// A non-seekable stream over a series of tapes.
	/// - Read mode: sequentially reads content across tapes (based on each tape's Position in bits).
	/// - Append mode: appends content at series tail; auto-rolls to a new tape when full.
	/// Designed to be used with CodeReader/CodeWriter.
	/// </summary>
	public sealed class TapeSeriesStream : Stream, IBitAwareStream
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
		#endregion

		#region fields
		private readonly Librarian _librarian;
		private readonly Guid _seriesId;
		private readonly OpenMode _mode;

		private readonly List<Tape> _orderedTapes;
		private int _currentIndex;
		private TapeCursor _current = null!;

		// Pending partial-byte flush buffering (to disambiguate Align vs Dispose semantics)
		private bool _hasPendingPartial;
		private byte _pendingByte;
		private int _pendingBits; // 1..7
		private bool _hasRolloverPending;

		private bool _isDisposed;
		#endregion

		#region IBitAwareStream (maintained by CodeWriter during partial-byte writes)
		public byte CurrentByte
		{
			get;
			set;
		}

		public int OffsetIntoCurrentByte
		{
			get;
			set;
		}
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
			if (_orderedTapes.Count == 0)
				throw new InvalidOperationException("No tapes found in the requested series.");

			_orderedTapes.Sort((a, b) =>
			{
				TapeLabel la = a.ReadLabel();
				TapeLabel lb = b.ReadLabel();
				int na = la.SeriesNumber;
				int nb = lb.SeriesNumber;
				if (na < nb)
					return -1;
				if (na > nb)
					return 1;
				return 0;
			});

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
			_current = MakeReadCursor(_orderedTapes[_currentIndex]);

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
			int contentBits = tape.Position;
			return new TapeCursor(tape, contentBits, 0);
		}
		#endregion

		#region Stream capability properties
		public override bool CanRead
		{
			get
			{
				return _mode == OpenMode.Read;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _mode == OpenMode.Append;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
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

				int bytesToRead = count < maxBytesFromBits ? count : maxBytesFromBits;
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

				// If we have a pending partial byte (from a prior Align/Dispose-triggered flush),
				// and new data is now being written, treat the pending as an ALIGNMENT:
				// finalize it as a FULL BYTE (pad zeros) before handling the new bytes.
				if (_hasPendingPartial)
				{
					CommitPendingAsFullByte();
				}

				int capacityBits = Tape.MaxContentLength - _current.BitPosition;
				bool writerFlushingPartial = OffsetIntoCurrentByte > 0 && OffsetIntoCurrentByte < 8;
				if (capacityBits <= 0)
				{
					if (writerFlushingPartial && count > 0)
					{
						_pendingByte = buffer[offset];
						_pendingBits = OffsetIntoCurrentByte;
						_hasPendingPartial = true;

						offset += 1;
						count -= 1;
					}

					RolloverAppendTape();
					continue;
				}

				int capacityBytes = capacityBits / 8;
				int bytesToWrite = count < capacityBytes ? count : capacityBytes;

				// Detect CodeWriter single-byte *partial* flush (1..7 bits).
				bool isSingleByteBuffer = count == 1;

				if (isSingleByteBuffer && writerFlushingPartial)
				{
					// Buffer; don't advance tape yet. We'll decide later:
					//  - Next Write() => treat as Align and commit as FULL BYTE.
					//  - Dispose() with no further writes => commit only the meaningful bits.
					_pendingByte = buffer[offset];
					_pendingBits = OffsetIntoCurrentByte;
					_hasPendingPartial = true;

					offset += 1;
					count -= 1;

					// Do not PersistTapePosition() here (nothing committed yet).
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

				// No whole bytes fit, but there are bits left and we still have buffer.
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

						RolloverAppendTape();

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
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
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
		}

		private void PersistTapePosition()
		{
			_current.Tape.Position = _current.BitPosition;
			_current.ContentBitLength = _current.BitPosition;
		}
		#endregion


		#region internal methods
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

			// Commit the byte as a full 8-bit (pad zeros for the high bits).
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
		private static CopyCodeToBufferDelegate _copyCodeToBufferDelegate = CopyCodeToBuffer;
		private delegate void CopyCodeToBufferDelegate(Code code, byte[] buffer, int offset, int byteCount);

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
