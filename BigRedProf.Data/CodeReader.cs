using System;
using System.Collections.Generic;
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

			throw new NotImplementedException();
		}

		public Code Read(int bitCount)
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CodeWriter));

			throw new NotImplementedException();
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
	}
}
