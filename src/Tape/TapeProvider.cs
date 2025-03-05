using BigRedProf.Data.Core;
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
		/// <param name="offset">The starting position in bits.</param>
		/// <param name="length">The number of bits to read.</param>
		/// <returns>The <see cref="Code"/> read from the tape.</returns>
		public Code Read(int offset, int length)
		{
			if (offset < 0 || offset > MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset), 
					$"Offset must be in the range [0, {MaxContentLength}]"
				);
			}

			if (length < 1 || offset + length > MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(length),
					$"Length must be at least 1 and when added to the 'offset' parameter " +
					$"cannot exceed {MaxContentLength}."
				);
			}

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + length - 1) / 8) - byteStart + 1;

			byte[] contentBytes = ReadInternal(byteStart, byteLength);

			// Fast path: Byte-aligned read
			if (bitOffset == 0 && length % 8 == 0)
			{
				Code code = new Code(contentBytes);
				return code;
			}

			// Slow path: Unaligned read, use bitwise shifting
			byte[] bytesToEncode = new byte[(length + 7) / 8];

			for (int i = 0; i < bytesToEncode.Length; i++)
			{
				bytesToEncode[i] = (byte)(contentBytes[i] >> bitOffset);
				if (i + 1 < contentBytes.Length)
				{
					bytesToEncode[i] |= (byte)(contentBytes[i + 1] << (8 - bitOffset));
				}
			}

			return new Code(bytesToEncode, length);
		}

		/// <summary>
		/// Writes content to the tape starting at the specified offset.
		/// </summary>
		/// <param name="content">The content to write.</param>
		/// <param name="offset">The starting position in bits.</param>
		public void Write(Code content, int offset)
		{
			if (offset < 0 || offset > MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					$"Offset must be in the range [0, {MaxContentLength}]"
				);
			}

			if (offset + content.Length > MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(content),
					$"Content length when added to the 'offset' parameter " +
					$"cannot exceed {MaxContentLength}."
				);
			}

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + content.Length - 1) / 8) - byteStart + 1;

			byte[] contentBytes = content.ToByteArray();

			// Fast path: Byte-aligned write
			if (bitOffset == 0 && content.Length % 8 == 0)
			{
				WriteInternal(contentBytes, byteStart, byteLength);
				return;
			}

			// Slow path: Create the byte array shifting each byte as needed
			byte[] bytesToWrite = new byte[byteLength];
			if (bitOffset == 0)
			{
				// we start aligned, so just shift each byte
				for (int i = 0; i < byteLength - 1; ++i)
					bytesToWrite[i] = (byte)((contentBytes[i] >> bitOffset) | (contentBytes[i + 1] << bitOffset));
			}
			else
			{
				// we need to read one existing byte on tape to help calculate are first byte
				byte firstByte = ReadInternal(byteStart, 1)[0];
				firstByte |= (byte)(contentBytes[0] << bitOffset);
				bytesToWrite[0] = firstByte;

				// subsequent bytes are created by shifting bytes from our content bytes
				int bytesToWriteIndex;
				if (byteLength > 1)
				{
					for (bytesToWriteIndex = 1; bytesToWriteIndex < byteLength - 1; ++bytesToWriteIndex)
						bytesToWrite[bytesToWriteIndex] = (byte)((contentBytes[bytesToWriteIndex - 1] >> 8 - bitOffset) | (contentBytes[bytesToWriteIndex] << bitOffset));

					// except for the last byte which doesn't have an upper part to merge in
					bytesToWrite[bytesToWriteIndex] = (byte)(contentBytes[bytesToWriteIndex - 1] >> 8 - bitOffset);
				}
			}

			WriteInternal(bytesToWrite, byteStart, byteLength);
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
	}
}
