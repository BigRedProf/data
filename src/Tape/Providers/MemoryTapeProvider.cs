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
		protected override Code ReadInternal(int length, int offset)
		{
			// Validate boundaries (already done in TapeProvider.ValidateRange)
			var byteStart = offset / 8;
			var bitOffset = offset % 8;
			var byteEnd = (offset + length - 1) / 8;

			// Allocate only the required portion of bytes
			var resultBytes = new byte[byteEnd - byteStart + 1];
			Array.Copy(_data, byteStart, resultBytes, 0, resultBytes.Length);

			// Create a Code object from the relevant portion
			return new Code(resultBytes, length, resultBytes[^1]); // Pass last byte to handle partial bits
		}

		protected override void WriteInternal(Code content, int offset)
		{
			// Validate boundaries (already done in TapeProvider.ValidateRange)
			var byteStart = offset / 8;
			var bitOffset = offset % 8;
			var dataBytes = content.ToByteArray();

			for (int i = 0; i < dataBytes.Length; i++)
			{
				// Merge content into the target byte array
				_data[byteStart + i] |= (byte)(dataBytes[i] >> bitOffset);

				// Handle any carry-over bits
				if (bitOffset > 0 && byteStart + i + 1 < _data.Length)
				{
					_data[byteStart + i + 1] |= (byte)(dataBytes[i] << (8 - bitOffset));
				}
			}
		}
		#endregion
	}
}
