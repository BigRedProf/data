using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape.Internal
{
	internal static class TapeHelper
	{
		#region public methods
		public static FlexModel ReadLabel(TapeProvider tapeProvider, Guid tapeId)
		{
			if (tapeProvider == null)
				throw new ArgumentNullException(nameof(tapeProvider), "Tape provider cannot be null.");

			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

			throw new NotImplementedException();
		}

		public static void WriteLabel(TapeProvider tapeProvider, Guid tapeId, FlexModel label)
		{
			if(tapeProvider == null)
				throw new ArgumentNullException(nameof(tapeProvider), "Tape provider cannot be null.");

			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");

			throw new NotImplementedException();
		}

		public static Code ReadContent(TapeProvider tapeProvider, Guid tapeId, int offset, int length)
		{
			if (offset < 0 || offset > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					$"Offset must be in the range [0, {Tape.MaxContentLength}]"
				);
			}

			if (length < 1 || offset + length > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(length),
					$"Length must be at least 1 and when added to the 'offset' parameter " +
					$"cannot exceed {Tape.MaxContentLength}."
				);
			}

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + length - 1) / 8) - byteStart + 1;

			// TODO: Offset by label size!!
			byte[] contentBytes = tapeProvider.ReadInternal(tapeId, byteStart, byteLength);

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
		/// <param name="tapeId">Unique identifier of the tape to write to.</param>
		/// <param name="content">The content to write.</param>
		/// <param name="offset">The starting position in bits.</param>
		public static void WriteContent(TapeProvider tapeProvider, Guid tapeId, Code content, int offset)
		{
			if (offset < 0 || offset > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					$"Offset must be in the range [0, {Tape.MaxContentLength}]"
				);
			}

			if (offset + content.Length > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(content),
					$"Content length when added to the 'offset' parameter " +
					$"cannot exceed {Tape.MaxContentLength}."
				);
			}

			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + content.Length - 1) / 8) - byteStart + 1;

			byte[] contentBytes = content.ToByteArray();

			// TODO: Offset by label size!!

			// Fast path: Byte-aligned write
			if (bitOffset == 0 && content.Length % 8 == 0)
			{
				tapeProvider.WriteInternal(tapeId, contentBytes, byteStart, byteLength);
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
				byte firstByte = tapeProvider.ReadInternal(tapeId, byteStart, 1)[0];
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

			tapeProvider.WriteInternal(tapeId, bytesToWrite, byteStart, byteLength);
		}
		#endregion
	}
}
