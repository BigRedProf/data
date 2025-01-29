using System;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeProvider
	{
		#region constants
		/// <summary>
		/// The maximum length, in bits, for a tape's content.
		/// </summary>
		public const int MaxContentLength = 1_000_000_000; // 1 billion bits
		#endregion 

		#region public methods
		/// <summary>
		/// Reads a portion of the tape starting at the specified offset.
		/// </summary>
		/// <param name="length">The number of bits to read.</param>
		/// <param name="offset">The starting position in bits.</param>
		/// <returns>The <see cref="Code"/> read from the tape.</returns>
		public Code Read(int length, int offset)
		{
			ValidateRange(offset, length);

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + length - 1) / 8) - byteStart + 1;

			byte[] rawBytes = ReadInternal(byteStart, byteLength);

			// Fast path: Byte-aligned read
			if (bitOffset == 0 && length % 8 == 0)
			{
				return new Code(rawBytes, length);
			}

			// Slow path: Unaligned read, use bitwise shifting
			return ExtractBits(rawBytes, bitOffset, length);
		}

		/// <summary>
		/// Writes content to the tape starting at the specified offset.
		/// </summary>
		/// <param name="content">The content to write.</param>
		/// <param name="offset">The starting position in bits.</param>
		public void Write(Code content, int offset)
		{
			ValidateRange(offset, content.Length);

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + content.Length - 1) / 8) - byteStart + 1;

			byte[] rawBytes = new byte[byteLength];

			// Read existing data in case we need to merge partial bytes
			if (bitOffset != 0 || content.Length % 8 != 0)
			{
				rawBytes = ReadInternal(byteStart, byteLength);
			}

			// Fast path: Byte-aligned write
			if (bitOffset == 0 && content.Length % 8 == 0)
			{
				Array.Copy(content.ToByteArray(), 0, rawBytes, 0, rawBytes.Length);
			}
			else
			{
				// Slow path: Use bitwise merging to handle unaligned writes
				MergeBits(rawBytes, content.ToByteArray(), bitOffset, content.Length);
			}

			WriteInternal(rawBytes, byteStart, byteLength);
		}
		#endregion

		#region protected methods
		/// <summary>
		/// Validates the requested offset and length.
		/// </summary>
		/// <param name="offset">The starting position in bits.</param>
		/// <param name="length">The number of bits to process.</param>
		protected void ValidateRange(int offset, int length)
		{
			if (offset < 0 || offset >= MaxContentLength)
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset is out of bounds.");

			if (length <= 0 || offset + length > MaxContentLength)
				throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds tape bounds.");
		}
		#endregion

		#region abstract methods
		/// <summary>
		/// Reads a portion of the tape. Must be implemented by subclasses.
		/// </summary>
		protected abstract byte[] ReadInternal(int byteOffset, int byteLength);

		/// <summary>
		/// Writes content to the tape. Must be implemented by subclasses.
		/// </summary>
		protected abstract void WriteInternal(byte[] data, int byteOffset, int byteLength);
		#endregion

		#region private methods
		/// <summary>
		/// Extracts bits from a byte array when the offset is not byte-aligned.
		/// </summary>
		private Code ExtractBits(byte[] rawBytes, int bitOffset, int length)
		{
			byte[] alignedBytes = new byte[(length + 7) / 8];

			for (int i = 0; i < alignedBytes.Length; i++)
			{
				alignedBytes[i] = (byte)(rawBytes[i] << bitOffset);
				if (i + 1 < rawBytes.Length)
				{
					alignedBytes[i] |= (byte)(rawBytes[i + 1] >> (8 - bitOffset));
				}
			}

			return new Code(alignedBytes, length);
		}

		/// <summary>
		/// Merges bitwise unaligned writes into an existing byte array.
		/// </summary>
		private void MergeBits(byte[] target, byte[] source, int bitOffset, int length)
		{
			for (int i = 0; i < source.Length; i++)
			{
				target[i] |= (byte)(source[i] >> bitOffset);
				if (bitOffset > 0 && i + 1 < target.Length)
				{
					target[i + 1] |= (byte)(source[i] << (8 - bitOffset));
				}
			}
		}
		#endregion
	}
}


