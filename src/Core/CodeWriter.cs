using BigRedProf.Data.Core.Internal;
using System;
using System.Diagnostics;
using System.IO;

namespace BigRedProf.Data.Core
{
	public class CodeWriter : IDisposable
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
		public CodeWriter(Stream stream)
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

			if(_offsetIntoCurrentByte == 0)
				return;

			WriteCurrentByte();
		}

		public void WriteCode(Code code)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CodeWriter));

			if(code == null)
				throw new ArgumentNullException(nameof(code));

			if (_offsetIntoCurrentByte == 0)
				WriteCodeFast(code);
			else
				WriteCodeSlow(code);
		}

		/// <summary>
		/// Useful in debugging, this method attempts to return data that's been written to the stream's
		/// buffer.
		/// </summary>
		/// <param name="startOffset">The bit offset into the stream. Defaults to 0.</param>
		/// <param name="length">The number of bits to read. Defaults to everything that's been written.</param>
		/// <returns>The requested data as a <see cref="Code"/>.</returns>
		/// <remarks>
		/// This method can be very useful for debugging. Beware though that it will only work for seekable
		/// streams.
		/// </remarks>
		public Code ToDebugCode(int startOffset = 0, int length = 0)
		{
			if (!_stream.CanSeek)
			{
				throw new InvalidOperationException(
					"This method can only be called if the underlying stream is seekable."
				);
			}

			long streamLength = (_stream.Length * 8) + _offsetIntoCurrentByte;
			if (streamLength > int.MaxValue)
				throw new InvalidOperationException("Length of stream cannot exceed Int32.MaxValue.");

			if (startOffset < 0)
				throw new ArgumentOutOfRangeException(nameof(startOffset), "Start offset cannot be negative.");

			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), "Length cannot be negative.");
			if (length == 0)
				length = (int)streamLength - startOffset;

			if (startOffset + length > streamLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(length),
					"Start offset plus length exceeds stream length."
				);
			}

			byte[] newBuffer = new byte[streamLength / 8 + 1];
			long currentStreamPosition = _stream.Position;
			_stream.Seek(0, SeekOrigin.Begin);
			_stream.Read(newBuffer, 0, newBuffer.Length - 1);
			_stream.Seek(currentStreamPosition, SeekOrigin.Begin);
			newBuffer[newBuffer.Length - 1] = _currentByte;

			CodeReader codeReader = new CodeReader(new MemoryStream(newBuffer));
			Code code = codeReader.Read((int) streamLength);

			// HACKHACK: Not the most elegant way to do this, but probably fine for a debug method.
			if(startOffset != 0 || length != streamLength)
			{
				string codeAsString = code;
				codeAsString = codeAsString.Replace(" ", string.Empty);
				codeAsString = codeAsString.Substring(startOffset, length);
				code = codeAsString;
			}

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

			if(disposing)
			{
				if (_offsetIntoCurrentByte != 0)
					WriteCurrentByte();

				_stream.Dispose();
			}

			_isDisposed = true;
		}

		~CodeWriter()
		{
			Dispose(false);
		}
		#endregion

		#region private methods
		private void WriteCurrentByte()
		{
			IBitAwareStream bitAwareStream = _stream as IBitAwareStream;
			if (bitAwareStream != null)
			{
				bitAwareStream.CurrentByte = _currentByte;
				int meaningfulBits = _offsetIntoCurrentByte == 0 ? 8 : _offsetIntoCurrentByte;
				bitAwareStream.OffsetIntoCurrentByte = meaningfulBits;
			}

			_stream.WriteByte(_currentByte);
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;

			UpdateBitAwareStream();
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

			UpdateBitAwareStream();
		}

		private void WriteCodeFast(Code code)
		{
			Debug.Assert(code != null);
			Debug.Assert(_offsetIntoCurrentByte == 0);

			int fullByteLength = code.Length / 8;
			int lastByteBitLength = code.Length % 8;
			if(fullByteLength > 0)
				_stream.Write(code.ByteArray, 0, fullByteLength);

			if (lastByteBitLength != 0)
				WriteCodeSlow(code[fullByteLength * 8, lastByteBitLength]);

			UpdateBitAwareStream();
		}

		private void WriteCodeSlow(Code code)
		{
			Debug.Assert(code != null);

			foreach (Bit bit in code)
				WriteBit(bit);

			UpdateBitAwareStream();
		}

		private void UpdateBitAwareStream()
		{
			IBitAwareStream bitAwareStream = _stream as IBitAwareStream;
			if (bitAwareStream != null)
			{
				bitAwareStream.CurrentByte = _currentByte;
				bitAwareStream.OffsetIntoCurrentByte = _offsetIntoCurrentByte;
			}
		}
		#endregion
	}
}
