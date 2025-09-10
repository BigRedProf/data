using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape.Internal
{
	internal static class TapeHelper
	{
		#region public methods
		public static FlexModel ReadLabel(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			var piedPiper = tape.TapeProvider.PiedPiper;
			// Read a reasonable max label size (4096 bytes = 32768 bits)
			const int maxLabelBits = 4096 * 8;
			Code code = ReadRawCode(tape.TapeProvider, tape.Id, 0, maxLabelBits);
			return piedPiper.DecodeModel<FlexModel>(code, CoreSchema.FlexModel);
		}

		public static void WriteLabel(Tape tape, FlexModel label)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");

			var piedPiper = tape.TapeProvider.PiedPiper;
			var code = piedPiper.EncodeModel(label, CoreSchema.FlexModel);
			WriteRawCode(tape.TapeProvider, tape.Id, code, 0);
		}

		public static Code ReadContent(Tape tape, int offset, int length)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			if (offset < 0 || offset > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					$"Offset must be between 0 and {Tape.MaxContentLength}.");
			}

			if (length <= 0 || length > Tape.MaxContentLength - offset)
			{
				throw new ArgumentOutOfRangeException(
					nameof(length),
					$"Length must be greater than 0 and offset+length must be less than {Tape.MaxContentLength}.");
			}

			return ReadRawCode(tape.TapeProvider, tape.Id, offset, length);
		}

		public static void WriteContent(Tape tape, Code content, int offset)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			if (content == null)
				throw new ArgumentNullException(nameof(content), "Content cannot be null.");

			if (offset < 0 || offset > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(offset),
					$"Offset must be between 0 and {Tape.MaxContentLength}.");
			}

			WriteRawCode(tape.TapeProvider, tape.Id, content, offset);
		}
		#endregion

		#region private methods
		private static Code ReadRawCode(TapeProvider tapeProvider, Guid tapeId, int offset, int length)
		{
			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + length - 1) / 8) - byteStart + 1;

			// TODO: Offset by label size!!
			byte[] contentBytes = tapeProvider.ReadTapeInternal(tapeId, byteStart, byteLength);

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

		private static void WriteRawCode(TapeProvider tapeProvider, Guid tapeId, Code content, int offset)
		{
			int byteStart = offset / 8;
			int bitOffset = offset % 8;
			int byteLength = ((offset + content.Length - 1) / 8) - byteStart + 1;

			byte[] contentBytes = content.ToByteArray();

			// TODO: Offset by label size!!

			// Fast path: Byte-aligned write
			if (bitOffset == 0 && content.Length % 8 == 0)
			{
				tapeProvider.WriteTapeInternal(tapeId, contentBytes, byteStart, byteLength);
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
				byte firstByte = tapeProvider.ReadTapeInternal(tapeId, byteStart, 1)[0];
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

			tapeProvider.WriteTapeInternal(tapeId, bytesToWrite, byteStart, byteLength);
		}
		#endregion
	}
}
