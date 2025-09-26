using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape._TestHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Xunit;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BigRedProf.Data.Tape.Test
{
    public class TapeRecorderTests : IDisposable
    {
        #region fields
        private readonly TapeProvider _memoryTapeProvider;
        private readonly TapeProvider _diskTapeProvider;
        private bool _disposed;
        #endregion

        #region constructors
        public TapeRecorderTests()
        {
            _memoryTapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
            _diskTapeProvider = TapeProviderHelper.CreateDiskTapeProvider();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (!_disposed)
            {
                TapeProviderHelper.DestroyDiskTapeProvider();
                _disposed = true;
            }
        }
        #endregion

        #region unit tests
		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_ShouldWriteDataToTape(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			var tapeRecorder = new TapeRecorder();
			tapeRecorder.InsertTape(tape);

			// Act
			Code code = new Code("10101010 00000000 11111111 00001111 010");
			byte[] expectedBytes = new byte[] { 0b01010101, 0b00000000, 0b11111111, 0b11110000, 0b010 };
			tapeRecorder.Record(code);

			// Assert
			byte[] actualBytes = tapeProvider.ReadTapeInternal(tapeId, 0, 5);
			Assert.Equal(expectedBytes, actualBytes);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_ByteAlignedWholeBytes_WritesExactly(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Act (aligned, whole bytes)
			Code code = new Code("11110000 00110011 01010101 10101010");
			rec.Record(code);

			// Assert
			// Note: Code packs LSB-first per byte: "11110000" => 0x0F, "00110011" => 0xCC, "01010101" => 0xAA, "10101010" => 0x55
			byte[] expected = { 0x0F, 0xCC, 0xAA, 0x55 };
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, expected.Length);
			Assert.Equal(expected, actual);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_ByteAlignedWithTailBits_MasksLastByte(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Act (aligned, but not a multiple of 8 bits: 8 + 4 = 12 bits)
			Code code = new Code("11111111 1111"); // 12 bits total
			rec.Record(code);

			// Assert
			// LSB-first: first byte all ones => 0xFF; second byte low 4 bits set => 0x0F
			byte[] expected = { 0xFF, 0x0F };
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, expected.Length);
			Assert.Equal(expected, actual);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_MisalignedStartBit3_WholeBytePayload_ShiftsAcrossBytes(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Advance head by 3 bits (misaligned start)
			rec.Record(new Code("000"));

			// Act: write 8 ones starting at bit offset 3
			rec.Record(new Code("11111111"));

			// Assert
			// Bits 3..7 of byte0 become 1 -> 11111000 (0xF8)
			// Bits 0..2 of byte1 become 1 -> 00000111 (0x07)
			byte[] expected = { 0x00 /* first 3 zeros live here, but combined result is below */, 0x00 }; // we'll read two bytes then assert
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, 2);
			Assert.Equal(0xF8, actual[0]);
			Assert.Equal(0x07, actual[1]);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_MisalignedStartBit5_TailBits_ShiftsAndMasks(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Start at bit offset 5
			rec.Record(new Code("00000"));

			// Act: write 10 ones (will span two bytes, not end on a byte boundary)
			rec.Record(new Code("1111111111"));

			// Assert
			// Byte0: bits 5..7 => 11100000 (0xE0)
			// Byte1: bits 0..6 => 01111111 (0x7F)
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, 2);
			Assert.Equal(0xE0, actual[0]);
			Assert.Equal(0x7F, actual[1]);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_MisalignedStartBit7_SpansThreeBytes(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Start at bit offset 7
			rec.Record(new Code("0000000"));

			// Act: write 16 ones across three bytes
			rec.Record(new Code("11111111 11111111"));

			// Assert
			// Byte0: only bit7 set => 10000000 (0x80)
			// Byte1: all ones      => 11111111 (0xFF)
			// Byte2: bits 0..6     => 01111111 (0x7F)
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, 3);
			Assert.Equal(0x80, actual[0]);
			Assert.Equal(0xFF, actual[1]);
			Assert.Equal(0x7F, actual[2]);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_ShortTail_MasksHighBitsOfLastByte(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Act: 4 bits, LSB-first "1010" => 0b00000101 (0x05)
			rec.Record(new Code("1010"));

			// Assert
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, 1);
			Assert.Equal(0x05, actual[0]); // LSB-first
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_ShortTail_MSBLookingPattern_FlippedGives0x0A(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			// Act: "0101" (LSB-first) => 0b00001010 (0x0A)
			rec.Record(new Code("0101"));

			// Assert
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, 1);
			Assert.Equal(0x0A, actual[0]);
		}

		[Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void Record_ShouldThrow_WhenContentIsNull(TapeProvider tapeProvider)
        {
            // Arrange
            Guid tapeId = Guid.NewGuid();
            Tape tape = Tape.CreateNew(tapeProvider, tapeId);
            var tapeRecorder = new TapeRecorder();
            tapeRecorder.InsertTape(tape);

            // Act & Assert
            Assert.ThrowsAny<ArgumentNullException>(() => tapeRecorder.Record(null!));
        }

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void Record_ShouldThrow_WhenTapeNotInserted(TapeProvider tapeProvider)
        {
            // Arrange
            var tapeRecorder = new TapeRecorder();
            var codeToWrite = new Code("10101010");

            // Act & Assert
            Assert.ThrowsAny<InvalidOperationException>(() => tapeRecorder.Record(codeToWrite));
        }

		// ---------- Helpers (pure, deterministic) ----------

		// Convert raw bytes to your Code-string bit format (LSB-first per byte).
		private static string ToLsbFirstBitString(byte[] bytes, int bitLength)
		{
			var sb = new System.Text.StringBuilder(bitLength + bitLength / 8);
			for (int i = 0; i < bitLength; i++)
			{
				int b = bytes[i >> 3];
				int bit = (b >> (i & 7)) & 1;
				sb.Append(bit == 1 ? '1' : '0');
				// (Optional) spaces every 8 bits:
				// if (((i + 1) % 8) == 0 && i + 1 < bitLength) sb.Append(' ');
			}
			return sb.ToString();
		}

		// Read-modify-write overlay: returns the resulting bytes after placing 'src' at 'startBit' over 'background'
		private static byte[] OverlayBits(byte[] background, int startBit, byte[] src, int bitLen)
		{
			var dst = new byte[background.Length];
			System.Buffer.BlockCopy(background, 0, dst, 0, background.Length);

			// mask tail of src so no garbage leaks
			if ((bitLen & 7) != 0)
			{
				int last = (bitLen - 1) >> 3;
				int keep = bitLen & 7;
				byte mask = (byte)((1 << keep) - 1);
				src[last] &= mask;
			}

			for (int i = 0; i < bitLen; i++)
			{
				int sByte = i >> 3;
				int sBit = i & 7;
				int bit = (src[sByte] >> sBit) & 1;

				int t = startBit + i;
				int dByte = t >> 3;
				int dBit = t & 7;

				dst[dByte] = (byte)(dst[dByte] & ~(1 << dBit));
				dst[dByte] = (byte)(dst[dByte] | ((bit & 1) << dBit));
			}

			return dst;
		}

		// Convenience to size the buffer we need to read/expect.
		private static int AffectedByteCount(int offsetBits, int bitLen)
		{
			int startBit = offsetBits & 7;
			return (startBit + bitLen + 7) >> 3; // ceil((startBit + bitLen)/8)
		}

		// ---------- Tests ----------

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_Long_Carry_Offset3_Len37(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			int offsetBits = 3;
			byte[] payload = { 0xDE, 0xAD, 0xBE, 0xEF, 0x01 }; // 5 bytes => 40 bits, will use only 37
			int payloadBits = 37;

			// Advance head by offset
			rec.Record(new Code(new string('0', offsetBits)));

			// Act
			string codeBits = ToLsbFirstBitString(payload, payloadBits);
			rec.Record(new Code(codeBits));

			// Assert
			int bytesToRead = AffectedByteCount(offsetBits, payloadBits);
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, bytesToRead);

			// background is zeros
			byte[] expected = OverlayBits(new byte[bytesToRead], offsetBits, (byte[])payload.Clone(), payloadBits);
			Assert.Equal(expected, actual);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_Long_Carry_Offset11_Len65(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			int offsetBits = 11;
			byte[] payload = { 0x13, 0x57, 0x9B, 0xDF, 0xF0, 0x0D, 0xAA, 0x55, 0x01 }; // 72 bits available
			int payloadBits = 65;

			// Build one contiguous write: 11 zeros + payload
			string codeBits = ToLsbFirstBitString(payload, payloadBits);
			string combined = new string('0', offsetBits) + codeBits;

			// Act
			rec.Record(new Code(combined));

			// Assert
			int totalBytes = (offsetBits + payloadBits + 7) >> 3; // ceil((offset+len)/8)
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, totalBytes);

			// Expected: overlay payload at bit 11 over zero background
			byte[] expected = OverlayBits(new byte[totalBytes], offsetBits, (byte[])payload.Clone(), payloadBits);
			Assert.Equal(expected, actual);
		}

		[Trait("Region", "TapeRecorder methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Record_VeryLong_Carry_Offset7_Len257(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			var tape = Tape.CreateNew(tapeProvider, tapeId);
			var rec = new TapeRecorder();
			rec.InsertTape(tape);

			int offsetBits = 7;
			// 33 bytes (264 bits); we'll write 257 bits (crosses 32+ bytes, awkward tail)
			byte[] payload = new byte[]
			{
			0xA5, 0x5A, 0xC3, 0x3C, 0x0F, 0xF0, 0x96, 0x69,
			0x12, 0x48, 0x24, 0x81, 0x7E, 0xE7, 0x3B, 0xB3,
			0xD2, 0x2D, 0xCC, 0x4C, 0x55, 0xAA, 0xFE, 0x01,
			0x39, 0x93, 0xC0, 0x0C, 0xED, 0xDE, 0xBE, 0xEF,
			0x42
			};
			int payloadBits = 257;

			rec.Record(new Code(new string('0', offsetBits)));

			// Act
			string codeBits = ToLsbFirstBitString(payload, payloadBits);
			rec.Record(new Code(codeBits));

			// Assert
			int bytesToRead = AffectedByteCount(offsetBits, payloadBits);
			byte[] actual = tapeProvider.ReadTapeInternal(tapeId, 0, bytesToRead);

			byte[] expected = OverlayBits(new byte[bytesToRead], offsetBits, (byte[])payload.Clone(), payloadBits);
			Assert.Equal(expected, actual);
		}

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void InsertTape_ShouldThrow_WhenTapeIsNull(TapeProvider tapeProvider)
        {
            // Arrange
            var tapeRecorder = new TapeRecorder();

            // Act & Assert
            Assert.ThrowsAny<ArgumentNullException>(() => tapeRecorder.InsertTape(null!));
        }

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void InsertTape_ShouldThrow_WhenTapeAlreadyInserted(TapeProvider tapeProvider)
        {
            // Arrange
            Guid tapeId = Guid.NewGuid();
            Tape tape = Tape.CreateNew(tapeProvider, tapeId);
            var tapeRecorder = new TapeRecorder();
            tapeRecorder.InsertTape(tape);

            // Act & Assert
            Assert.ThrowsAny<InvalidOperationException>(() => tapeRecorder.InsertTape(tape));
        }
        #endregion
    }
}
