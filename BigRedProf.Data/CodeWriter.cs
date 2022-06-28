using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BigRedProf.Data
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
			_stream.WriteByte(_currentByte);
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
			IReadOnlyCollection<byte> bytes = code.ByteArray;
			for (int i = 0; i < fullByteLength; ++i)
				_stream.WriteByte(code.ByteArray[i]);

			if (lastByteBitLength != 0)
				WriteCodeSlow(code[fullByteLength, lastByteBitLength]);
		}

		private void WriteCodeSlow(Code code)
		{
			Debug.Assert(code != null);
			Debug.Assert(_offsetIntoCurrentByte != 0);

			foreach (Bit bit in code)
				WriteBit(bit);
		}
		#endregion
	}
}
