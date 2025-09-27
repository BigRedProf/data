using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape;
using System;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
	public class TapePlayerTests : IDisposable
	{
		#region fields
		private readonly TapeProvider _memoryTapeProvider;
		private readonly TapeProvider _diskTapeProvider;
		private bool _disposed;
		#endregion

		#region constructors
		public TapePlayerTests()
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

		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldReturnWrittenData(TapeProvider tapeProvider)
		{
			// Arrange (seed bytes directly so position stays at 0)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			Code expectedCode = new Code("10101010");
			byte[] bytes = new byte[] { 0b01010101 };
			tapeProvider.WriteTapeInternal(tapeId, bytes, 0, bytes.Length);

			// Act
			TapePlayer tapePlayer = new TapePlayer();
			tapePlayer.InsertTape(tape);
			Code actualCode = tapePlayer.Play(expectedCode.Length);

			// Assert
			Assert.Equal(expectedCode, actualCode);
		}

		// Aligned, multi-byte (32 bits)
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ByteAlignedWholeBytes_ReturnsExactBits(TapeProvider tapeProvider)
		{
			// Arrange: "11110000 00110011 01010101 10101010"
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[]
			{
				0b00001111,
				0b11001100,
				0b10101010,
				0b01010101
			};
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			Code read = player.Play(32);

			// Assert
			Assert.Equal(new Code("11110000 00110011 01010101 10101010"), read);
		}

		// Aligned, tail bits (12 bits total)
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ByteAlignedWithTailBits_ReturnsMaskedTail(TapeProvider tapeProvider)
		{
			// Arrange: "11111111 1111" (12 bits)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[] { 0b11111111, 0b00001111 };
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			Code read = player.Play(12);

			// Assert
			Assert.Equal(new Code("11111111 1111"), read);
		}

		// Misaligned start @ bit 3, read 8 bits (crosses byte boundary)
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_MisalignedStartBit3_Read8Ones(TapeProvider tapeProvider)
		{
			// Arrange: bytes after writing "000" + "11111111" are [11111000, 00000111]
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[] { 0b11111000, 0b00000111 };
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act: skip 3 bits, then read 8
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(3);
			Code read = player.Play(8);

			// Assert
			Assert.Equal(new Code("11111111"), read);
		}

		// Misaligned start @ bit 5, read 10 bits
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_MisalignedStartBit5_Read10(TapeProvider tapeProvider)
		{
			// Arrange: bytes after "00000" + 10 ones are [11100000, 01111111]
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[] { 0b11100000, 0b01111111 };
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act: skip 5, then read 10
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(5);
			Code read = player.Play(10);

			// Assert
			Assert.Equal(new Code("1111111111"), read);
		}

		// Misaligned start @ bit 7, read 16 bits spanning 3 bytes
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_MisalignedStartBit7_Read16AcrossThreeBytes(TapeProvider tapeProvider)
		{
			// Arrange: "0000000" + "11111111 11111111" -> bytes [10000000, 11111111, 01111111]
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[] { 0b10000000, 0b11111111, 0b01111111 };
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act: skip 7, then read 16
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(7);
			Code read = player.Play(16);

			// Assert
			Assert.Equal(new Code("11111111 11111111"), read);
		}

		// Long, awkward offset 11, length 65 (crosses many bytes, partial tail)
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_Long_Carry_Offset11_Len65(TapeProvider tapeProvider)
		{
			// Arrange: seed exact bytes representing zeros(11) + payload(65 bits)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			byte[] payload = new byte[] { 0x13, 0x57, 0x9B, 0xDF, 0xF0, 0x0D, 0xAA, 0x55, 0x01 };
			int payloadBits = 65;
			int offsetBits = 11;

			int totalBytes = (offsetBits + payloadBits + 7) >> 3;
			byte[] seeded = new byte[totalBytes];
			byte[] seededComputed = OverlayBits(seeded, offsetBits, (byte[])payload.Clone(), payloadBits);
			tapeProvider.WriteTapeInternal(tapeId, seededComputed, 0, seededComputed.Length);

			// Act: skip 11, then read 65
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(offsetBits);
			string codeBits = ToLsbFirstBitString(payload, payloadBits);
			Code read = player.Play(payloadBits);

			// Assert
			Assert.Equal(new Code(codeBits), read);
		}

		// Sequential reads should advance head correctly
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_SequentialReads_AdvanceHead(TapeProvider tapeProvider)
		{
			// Arrange: "10101010 1111" -> bytes [01010101, 00001111]
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			byte[] seeded = new byte[] { 0b01010101, 0b00001111 };
			tapeProvider.WriteTapeInternal(tapeId, seeded, 0, seeded.Length);

			// Act
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			Code first = player.Play(8);
			Code second = player.Play(4);

			// Assert
			Assert.Equal(new Code("10101010"), first);
			Assert.Equal(new Code("1111"), second);
		}

		// Exceptions

		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldThrow_WhenTapeNotInserted(TapeProvider tapeProvider)
		{
			// Arrange
			TapePlayer tapePlayer = new TapePlayer();

			// Act & Assert
			Assert.ThrowsAny<InvalidOperationException>(() => tapePlayer.Play(1));
		}

		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldThrow_WhenLengthIsNegative(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			TapePlayer tapePlayer = new TapePlayer();
			tapePlayer.InsertTape(tape);

			// Act & Assert
			Assert.ThrowsAny<ArgumentException>(() => tapePlayer.Play(-1));
		}

		#endregion

		#region helpers

		// Convert raw bytes to Code-string bit format (LSB-first per byte).
		private static string ToLsbFirstBitString(byte[] bytes, int bitLength)
		{
			StringBuilder sb = new StringBuilder(bitLength + (bitLength / 8));
			for (int i = 0; i < bitLength; i++)
			{
				int b = bytes[i >> 3];
				int bit = (b >> (i & 7)) & 1;
				sb.Append(bit == 1 ? '1' : '0');
			}
			return sb.ToString();
		}

		// Overlay src bits onto dst (LSB-first), starting at bit offset 'startBit'
		private static byte[] OverlayBits(byte[] background, int startBit, byte[] src, int bitLen)
		{
			byte[] dst = new byte[background.Length];
			Buffer.BlockCopy(background, 0, dst, 0, background.Length);

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

		// --- Round-trip tests (Recorder -> Player) ---

		[Trait("Region", "Tape round-trip")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void RoundTrip_AlignedWholeBytes(TapeProvider tapeProvider)
		{
			// Arrange (write aligned 32 bits)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			TapeRecorder rec = new TapeRecorder();
			rec.InsertTape(tape);
			Code written = new Code("11110000 00110011 01010101 10101010");
			rec.Record(written);

			// Rewind via recorder (tape is shared by reference)
			rec.RewindOrFastForwardTo(0);

			// Act
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			Code read = player.Play(written.Length);

			// Assert
			Assert.Equal(written, read);
		}

		[Trait("Region", "Tape round-trip")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void RoundTrip_AlignedWithTailBits(TapeProvider tapeProvider)
		{
			// Arrange (12 bits: byte-aligned start with a tail)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			TapeRecorder rec = new TapeRecorder();
			rec.InsertTape(tape);
			Code written = new Code("11111111 1111"); // 12 bits, LSB-first per byte
			rec.Record(written);

			// Rewind via recorder
			rec.RewindOrFastForwardTo(0);

			// Act
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			Code read = player.Play(written.Length);

			// Assert
			Assert.Equal(written, read);
		}

		[Trait("Region", "Tape round-trip")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void RoundTrip_MisalignedStartBit3_Read8Ones(TapeProvider tapeProvider)
		{
			// Arrange: write "000" then "11111111" (forces misaligned start for payload)
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			TapeRecorder rec = new TapeRecorder();
			rec.InsertTape(tape);
			rec.Record(new Code("000"));
			rec.Record(new Code("11111111"));

			// Rewind via recorder
			rec.RewindOrFastForwardTo(0);

			// Act: skip 3, then read 8
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(3);
			Code read = player.Play(8);

			// Assert
			Assert.Equal(new Code("11111111"), read);
		}

		[Trait("Region", "Tape round-trip")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void RoundTrip_MisalignedStartBit5_Tail10Bits(TapeProvider tapeProvider)
		{
			// Arrange: write "00000" then 10 ones
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			TapeRecorder rec = new TapeRecorder();
			rec.InsertTape(tape);
			rec.Record(new Code("00000"));
			rec.Record(new Code("1111111111"));

			// Rewind via recorder
			rec.RewindOrFastForwardTo(0);

			// Act: skip 5, then read 10
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(5);
			Code read = player.Play(10);

			// Assert
			Assert.Equal(new Code("1111111111"), read);
		}

		[Trait("Region", "Tape round-trip")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void RoundTrip_Long_Carry_Offset11_Len65(TapeProvider tapeProvider)
		{
			// Arrange: combine as one write to avoid mid-stream extend quirks
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);

			TapeRecorder rec = new TapeRecorder();
			rec.InsertTape(tape);

			byte[] payload = new byte[] { 0x13, 0x57, 0x9B, 0xDF, 0xF0, 0x0D, 0xAA, 0x55, 0x01 }; // 72 bits available
			int payloadBits = 65;
			string codeBits = ToLsbFirstBitString(payload, payloadBits);
			string combined = new string('0', 11) + codeBits;

			rec.Record(new Code(combined));

			// Rewind via recorder
			rec.RewindOrFastForwardTo(0);

			// Act: skip 11, read 65
			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			_ = player.Play(11);
			Code read = player.Play(payloadBits);

			// Assert
			Assert.Equal(new Code(codeBits), read);
		}
		#endregion
	}
}
