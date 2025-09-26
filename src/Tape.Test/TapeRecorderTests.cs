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
