using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BigRedProf.Data
{
	public class CodeReader : IDisposable
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
		private Stream _stream;
		private byte _currentByte;
		private int _offsetIntoCurrentByte;
		private bool _isDisposed;
		#endregion

		#region constructors
		public CodeReader(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			_stream = stream;
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;
			_isDisposed = false;
		}
		#endregion

		#region methods
		public void AlignToNextByteBoundary()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CodeWriter));

			// This implementation seems counterintuitive, but:
			// 1. If we're already byte-aligned, this is a no-op.
			// 2. If not, the next call to ReadBit() will read the next byte.
			_offsetIntoCurrentByte = 0;
		}

		public Code Read(int bitCount)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CodeWriter));

			if(bitCount < 1)
				throw new ArgumentOutOfRangeException(nameof(bitCount), "The parameter must be at least 1.");

			Code code;
			if (_offsetIntoCurrentByte == 0)
				code = ReadCodeFast(bitCount);
			else
				code = ReadCodeSlow(bitCount);

			return code;
		}
		#endregion

		#region IDisposable methods
		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes this object.
		/// </summary>
		/// <param name="disposing">True if user-initated, false if finalizer-initiated.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_stream.Dispose();
			}

			_isDisposed = true;
		}

		~CodeReader()
		{
			Dispose(false);
		}
		#endregion

		#region private methods
		private void ReadNextByte()
		{
			int result = _stream.ReadByte();
			if (result == -1)
				throw new InvalidOperationException("Attempted to read past end of stream.");

			_currentByte = (byte)result;
			_offsetIntoCurrentByte = 0;
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
				_stream.Read(bytes, 0, fullByteLength);
				code = new Code(bytes, bitCount);
			}
			else
			{
				code = new Code(bitCount);
			}

			if (lastByteBitLength != 0)
			{
				for (int i = 0; i < lastByteBitLength; ++i)
					code[(fullByteLength * 8) + i] = ReadBit();
			}

			return code;
		}

		private Code ReadCodeSlow(int bitCount)
		{
			Debug.Assert(bitCount >= 1);

			Code code = new Code(bitCount);
			for(int i = 0; i < bitCount; ++i)
				code[i] = ReadBit();

			return code;
		}
		#endregion
	}
}
