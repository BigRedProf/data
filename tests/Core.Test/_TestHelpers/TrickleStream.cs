using System;
using System.IO;

namespace BigRedProf.Data.Test._TestHelpers
{
	/// <summary>
	/// A read-only stream that returns at most a few bytes from each Read, the way a network
	/// stream such as an HTTP request body does once the data spans more than one packet.
	/// </summary>
	/// <remarks>
	/// Stream.Read is allowed to return fewer bytes than were asked for, and a MemoryStream never
	/// does, so a reader tested only against MemoryStream never finds out it ignored the count.
	/// </remarks>
	internal class TrickleStream : Stream
	{
		#region fields
		private readonly byte[] _bytes;
		private readonly int _maximumBytesPerRead;
		private int _position;
		#endregion

		#region constructors
		public TrickleStream(byte[] bytes, int maximumBytesPerRead)
		{
			_bytes = bytes;
			_maximumBytesPerRead = maximumBytesPerRead;
			_position = 0;
		}
		#endregion

		#region Stream properties
		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}
		#endregion

		#region Stream methods
		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesToCopy = Math.Min(count, Math.Min(_maximumBytesPerRead, _bytes.Length - _position));
			Array.Copy(_bytes, _position, buffer, offset, bytesToCopy);
			_position += bytesToCopy;

			return bytesToCopy;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		#endregion
	}
}
