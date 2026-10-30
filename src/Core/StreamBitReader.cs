using System;
using System.Diagnostics;
using System.IO;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Reads bits from a readable <see cref="Stream"/>, least significant bit first.
	/// </summary>
	/// <remarks>
	/// Keeps a fast path for a byte-aligned read and a bit-by-bit path otherwise. A short read is
	/// not treated as the end of the stream: the reader keeps calling <see cref="Stream.Read"/>
	/// until the requested bytes arrive or the stream returns zero.
	/// </remarks>
	public sealed class StreamBitReader : BitStream
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
		/// <param name="stream">A readable stream. The reader starts at the stream's current position.</param>
		/// <param name="leaveOpen">
		/// True to leave <paramref name="stream"/> open when this reader is disposed. The default
		/// disposes it.
		/// </param>
		public StreamBitReader(Stream stream, bool leaveOpen = false)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (!stream.CanRead)
				throw new ArgumentException("The stream does not support reading.", nameof(stream));

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
				return !IsDisposed;
			}
		}

		/// <inheritdoc/>
		public override bool CanWrite
		{
			get
			{
				return false;
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
					if (_offsetIntoCurrentByte == 0)
						code = ReadCodeFast(bitCount);
					else
						code = ReadCodeSlow(bitCount);

					AdvancePosition(bitCount);
				}
			);

			return code;
		}

		/// <inheritdoc/>
		public override void WriteBits(Code code)
		{
			PrepareWrite(code);
		}

		/// <inheritdoc/>
		public override void AlignToNextByteBoundary()
		{
			PrepareOperation();
			if (_offsetIntoCurrentByte == 0)
				return;

			int skipped = 8 - _offsetIntoCurrentByte;
			_offsetIntoCurrentByte = 0;
			AdvancePosition(skipped);
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			// No I/O. The base method enforces the disposed and faulted checks.
			base.Flush();
		}
		#endregion

		#region BitStream methods (dispose)
		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (!disposing || _leaveOpen)
				return;

			_stream.Dispose();
		}
		#endregion

		#region private methods
		private void ReadNextByte()
		{
			int result = _stream.ReadByte();
			if (result == -1)
				throw new EndOfStreamException();

			_currentByte = (byte)result;
			_offsetIntoCurrentByte = 0;
		}

		/// <summary>
		/// Fills the first <paramref name="count"/> bytes of <paramref name="buffer"/>.
		/// </summary>
		/// <remarks>
		/// <see cref="Stream.Read"/> may return fewer bytes than asked for without being at the
		/// end. Taking one read as the whole answer would zero-fill the rest.
		/// </remarks>
		private void ReadExactly(byte[] buffer, int count)
		{
			int offset = 0;
			while (offset < count)
			{
				int bytesRead = _stream.Read(buffer, offset, count - offset);
				if (bytesRead == 0)
					throw new EndOfStreamException();

				offset += bytesRead;
			}
		}

		private Bit ReadBit()
		{
			if (_offsetIntoCurrentByte == 0)
				ReadNextByte();

			byte bitMask = _bitMask[_offsetIntoCurrentByte];
			Bit result = (_currentByte & bitMask) == bitMask ? 1 : 0;

			++_offsetIntoCurrentByte;
			if (_offsetIntoCurrentByte == 8)
				_offsetIntoCurrentByte = 0;

			return result;
		}

		private Code ReadCodeFast(int bitCount)
		{
			Debug.Assert(bitCount >= 1);
			Debug.Assert(_offsetIntoCurrentByte == 0);

			Code code;

			int fullByteLength = bitCount / 8;
			int lastByteBitLength = bitCount % 8;
			int bytesLength = lastByteBitLength == 0 ? fullByteLength : fullByteLength + 1;
			byte[] bytes = new byte[bytesLength];
			if (fullByteLength > 0)
			{
				ReadExactly(bytes, fullByteLength);
				code = new Code(bytes, bitCount);
			}
			else
			{
				code = new Code(bitCount);
			}

			if (lastByteBitLength != 0)
			{
				CodeBuilder builder = new CodeBuilder(code);
				for (int i = 0; i < lastByteBitLength; ++i)
					builder[(fullByteLength * 8) + i] = ReadBit();

				code = builder.Build();
			}

			return code;
		}

		private Code ReadCodeSlow(int bitCount)
		{
			Debug.Assert(bitCount >= 1);

			CodeBuilder builder = new CodeBuilder(bitCount);
			for (int i = 0; i < bitCount; ++i)
				builder[i] = ReadBit();

			return builder.Build();
		}
		#endregion
	}
}
