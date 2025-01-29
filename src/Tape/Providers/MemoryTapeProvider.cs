using System;

namespace BigRedProf.Data.Tape.Providers
{
	public class MemoryTapeProvider : TapeProvider
	{
		#region fields
		private readonly byte[] _data;
		#endregion

		#region constructors
		public MemoryTapeProvider()
		{
			// Allocate space for 1 billion bits (125 MB)
			_data = new byte[MaxContentLength / 8];
		}
		#endregion

		#region TapeProvider methods
		protected override byte[] ReadInternal(int byteOffset, int byteLength)
		{
			var resultBytes = new byte[byteLength];
			Array.Copy(_data, byteOffset, resultBytes, 0, byteLength);
			return resultBytes;
		}

		protected override void WriteInternal(byte[] data, int byteOffset, int byteLength)
		{
			Array.Copy(data, 0, _data, byteOffset, byteLength);
		}
		#endregion
	}
}


