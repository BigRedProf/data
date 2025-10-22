using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape.Internal
{
	internal static class TapeHelper
	{
		#region public methods
		public static TapeLabel ReadLabel(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			var piedPiper = tape.TapeProvider.PiedPiper;

			byte[] bytes = tape.TapeProvider.ReadLabelInternal(tape.Id);
			Code code = new Code(bytes);

			FlexModel flexModel = piedPiper.DecodeModel<FlexModel>(code, CoreSchema.FlexModel);
			TapeLabel tapeLabel = TapeLabel.FromFlexModel(flexModel);
			return tapeLabel;
		}

		public static void WriteLabel(Tape tape, FlexModel label)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");

			var piedPiper = tape.TapeProvider.PiedPiper;

			var code = piedPiper.EncodeModel(label, CoreSchema.FlexModel);
			byte[] bytes = code.ToByteArray();

			tape.TapeProvider.WriteLabelInternal(tape.Id, bytes);
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

		private static void WriteRawCode(TapeProvider tapeProvider, Guid tapeId, Code content, int offsetBits)
		{
			// Number of bits we are writing
			int bitLen = content.Length;
			if (bitLen == 0)
				return;

			int startByte = offsetBits >> 3;
			int startBit = offsetBits & 7;

			// Bytes of content (may include unused bits in the last byte)
			byte[] src = content.ToByteArray();

			// Mask out any unused high bits in the final source byte so we never leak garbage
			int tailBits = bitLen & 7;
			if (tailBits != 0)
			{
				byte mask = (byte)((1 << tailBits) - 1); // keep low 'tailBits' bits
				src[(bitLen - 1) >> 3] &= mask;
			}

			// Fast path: fully byte-aligned and whole-byte length
			if (startBit == 0 && (bitLen & 7) == 0)
			{
				int fullBytes = bitLen >> 3;
				if (fullBytes > 0)
					tapeProvider.WriteTapeInternal(tapeId, src, startByte, fullBytes);
				return;
			}

			// How many destination bytes are affected on tape?
			int affectedBytes = (startBit + bitLen + 7) >> 3; // ceil((startBit + bitLen)/8)

			// Read existing region so we can preserve untouched bits
			byte[] dst = tapeProvider.ReadTapeInternal(tapeId, startByte, affectedBytes);

			// Overlay bit-by-bit (clear & set) for clarity and correctness
			for (int i = 0; i < bitLen; i++)
			{
				int sByte = i >> 3;
				int sBit = i & 7;
				int bit = (src[sByte] >> sBit) & 1;

				int tIndex = startBit + i;
				int dByte = tIndex >> 3;
				int dBit = tIndex & 7;

				// clear then set
				dst[dByte] = (byte)(dst[dByte] & ~(1 << dBit));
				dst[dByte] = (byte)(dst[dByte] | ((bit & 1) << dBit));
			}

			// Write the merged bytes back to tape
			tapeProvider.WriteTapeInternal(tapeId, dst, startByte, affectedBytes);
		}
		#endregion
	}
}
