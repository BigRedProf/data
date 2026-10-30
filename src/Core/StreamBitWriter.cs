using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Writes bits to a writable <see cref="Stream"/>, least significant bit first.
	/// </summary>
	/// <remarks>
	/// Writes forward only, from wherever the underlying stream currently is. It does not require
	/// the stream to be at its end, and it never reads back or rewrites a byte it has written.
	/// While this writer is active, callers must not reposition, read, or write that stream.
	///
	/// Complete bytes are pushed as they fill. A partial byte stays buffered until it fills, until
	/// <see cref="AlignToNextByteBoundary"/> zero-pads it, or until <see cref="BitStream.Dispose"/>
	/// zero-pads it. <see cref="Flush"/> does not pad.
	/// </remarks>
	public sealed class StreamBitWriter : BitStream
	{
		#region static fields
		private static readonly byte[] _bitMask = new byte[]
		{
			0b00000001,
			0b00000010,
			0b00000100,
			0b00001000,
			0b00010000,
			0b00100000,
			0b01000000,
			0b10000000
		};
		#endregion

		#region fields
		private readonly Stream _stream;
		private readonly bool _leaveOpen;
		private byte _currentByte;
		private int _offsetIntoCurrentByte;
		#endregion

		#region constructors
		/// <summary>
		/// Wraps <paramref name="stream"/>.
		/// </summary>
		/// <param name="stream">
		/// A writable stream. Bits are appended from its current position.
		/// </param>
		/// <param name="leaveOpen">
		/// True to leave <paramref name="stream"/> open when this writer is disposed. The default
		/// disposes it.
		/// </param>
		public StreamBitWriter(Stream stream, bool leaveOpen = false)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (!stream.CanWrite)
				throw new ArgumentException("The stream does not support writing.", nameof(stream));

			_stream = stream;
			_leaveOpen = leaveOpen;
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;
		}
		#endregion

		#region BitStream properties
		/// <inheritdoc/>
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public override bool CanWrite
		{
			get
			{
				return !IsDisposed;
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
			return new Code(0);
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
					if (_offsetIntoCurrentByte == 0)
						WriteCodeFast(code);
					else
						WriteCodeSlow(code);

					AdvancePosition(code.Length);
				}
			);
		}

		/// <inheritdoc/>
		public override void AlignToNextByteBoundary()
		{
			PrepareOperation();
			if (_offsetIntoCurrentByte == 0)
				return;

			int padding = 8 - _offsetIntoCurrentByte;
			RunIo(
				delegate()
				{
					WriteCurrentByte();
					AdvancePosition(padding);
				}
			);
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			base.Flush();
			RunIo(
				delegate()
				{
					_stream.Flush();
				}
			);
		}
		#endregion

		#region BitStream methods (dispose)
		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			Exception? primary = null;
			try
			{
				if (!IsFaulted)
				{
					try
					{
						if (_offsetIntoCurrentByte != 0)
							WriteCurrentByte();

						_stream.Flush();
					}
					catch (Exception exception)
					{
						Fault(exception);
						primary = exception;
					}
				}
			}
			finally
			{
				if (!_leaveOpen)
				{
					try
					{
						_stream.Dispose();
					}
					catch (Exception cleanup)
					{
						if (primary != null)
							primary.Data[DisposeExceptionDataKey] = cleanup;
						else
							primary = cleanup;
					}
				}
			}

			if (primary != null)
				ExceptionDispatchInfo.Capture(primary).Throw();
		}
		#endregion

		#region private methods
		private void WriteCurrentByte()
		{
			_stream.WriteByte(_currentByte);
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;
		}

		private void WriteBit(Bit bit)
		{
			if (bit == 1)
			{
				byte bitMask = _bitMask[_offsetIntoCurrentByte];
				_currentByte |= bitMask;
			}

			++_offsetIntoCurrentByte;
			if (_offsetIntoCurrentByte == 8)
				WriteCurrentByte();
		}

		private void WriteCodeFast(Code code)
		{
			Debug.Assert(code != null);
			Debug.Assert(_offsetIntoCurrentByte == 0);

			int fullByteLength = code.Length / 8;
			int lastByteBitLength = code.Length % 8;
			if (fullByteLength > 0)
				_stream.Write(code.ByteArray, 0, fullByteLength);

			if (lastByteBitLength != 0)
				WriteCodeSlow(code[fullByteLength * 8, lastByteBitLength]);
		}

		private void WriteCodeSlow(Code code)
		{
			Debug.Assert(code != null);

			foreach (Bit bit in code)
				WriteBit(bit);
		}
		#endregion
	}
}
