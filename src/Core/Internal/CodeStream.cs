using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BigRedProf.Data.Core.Internal
{
	internal class CodeStream : Stream, IBitAwareStream, IDisposable
	{
		#region fields
		private MemoryStream _memoryStream;
		private Code _code;
		#endregion

		#region constructors
		public CodeStream()
		{
			_memoryStream = new MemoryStream();
			OffsetIntoCurrentByte = 0;
		}
		#endregion

		#region Stream properties
		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => _memoryStream.Length;

		public override long Position
		{
			get
			{
				return _memoryStream.Position;
			}
			set
			{
				_memoryStream.Position = value;
			}
		}
		#endregion

		#region IBitAwareStream properties
		public byte CurrentByte { get; set; }
		public int OffsetIntoCurrentByte { get; set; }
		#endregion

		#region methods
		public Code ToCode()
		{
			Flush();
			return _code;
		}
		#endregion

		#region Stream methods
		public override void Flush()
		{
			_memoryStream.Flush();

			if (OffsetIntoCurrentByte == 0)
				_code = new Code(_memoryStream.ToArray(), (int) Length * 8);
			else
				_code = new Code(_memoryStream.ToArray(), (int) Length * 8 + OffsetIntoCurrentByte, CurrentByte);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _memoryStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			_memoryStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_memoryStream.Write(buffer, offset, count);
		}
		#endregion

		#region IDisposable methods
		public new bool Dispose()
		{
			Flush();
			base.Dispose();
			return true;
		}
		#endregion
	}
}
