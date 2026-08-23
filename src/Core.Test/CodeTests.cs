using BigRedProf.Data.Core;
using System;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class CodeTests
	{
		#region constructor tests
		[Fact]
		public void ByteArrayConstructorShouldWork()
		{
			Code code = new Code(new byte[] { 127, 0b01011011 });

			Assert.Equal(16, code.Length);

			Assert.Equal<Bit>(1, code[0]);
			Assert.Equal<Bit>(1, code[1]);
			Assert.Equal<Bit>(1, code[2]);
			Assert.Equal<Bit>(1, code[3]);
			Assert.Equal<Bit>(1, code[4]);
			Assert.Equal<Bit>(1, code[5]);
			Assert.Equal<Bit>(1, code[6]);
			Assert.Equal<Bit>(0, code[7]);

			Assert.Equal<Bit>(1, code[8]);
			Assert.Equal<Bit>(1, code[9]);
			Assert.Equal<Bit>(0, code[10]);
			Assert.Equal<Bit>(1, code[11]);
			Assert.Equal<Bit>(1, code[12]);
			Assert.Equal<Bit>(0, code[13]);
			Assert.Equal<Bit>(1, code[14]);
			Assert.Equal<Bit>(0, code[15]);
		}
		[Fact]
		public void ByteArrayConstructorShouldZeroUnusedTrailingBits()
		{
			// only the first 12 bits are significant; the source arrays differ solely
			// in the unused high 4 bits of the last byte
			Code codeWithGarbageBits = new Code(new byte[] { 0b10110101, 0b11111010 }, 12);
			Code codeWithZeroedBits = new Code(new byte[] { 0b10110101, 0b00001010 }, 12);

			Assert.Equal(codeWithZeroedBits, codeWithGarbageBits);
			Assert.True(codeWithZeroedBits == codeWithGarbageBits);
			Assert.Equal(codeWithZeroedBits.GetHashCode(), codeWithGarbageBits.GetHashCode());
			Assert.Equal((byte)0b00001010, codeWithGarbageBits.ToByteArray()[1]);
		}

		[Fact]
		public void LastByteConstructorShouldZeroUnusedTrailingBits()
		{
			// only the first 12 bits are significant; the last bytes differ solely
			// in the unused high 4 bits
			Code codeWithGarbageBits = new Code(new byte[] { 0b10110101, 0 }, 12, 0b11111010);
			Code codeWithZeroedBits = new Code(new byte[] { 0b10110101, 0 }, 12, 0b00001010);

			Assert.Equal(codeWithZeroedBits, codeWithGarbageBits);
			Assert.True(codeWithZeroedBits == codeWithGarbageBits);
			Assert.Equal(codeWithZeroedBits.GetHashCode(), codeWithGarbageBits.GetHashCode());
			Assert.Equal((byte)0b00001010, codeWithGarbageBits.ToByteArray()[1]);
		}

		[Fact]
		public void NegativeLengthCodesShouldThrow()
		{
			// Zero is allowed -- see AnEmptyCodeShouldBeAllowed. Negative is not a length.
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					new Code(-1);
				}
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					new CodeBuilder(-1);
				}
			);
		}

		[Fact]
		public void WhitespaceOnlyStringShouldBeTheEmptyCode()
		{
			// Whitespace is skipped when reading a code out of a string, so a string of it
			// carries no bits at all.
			Assert.Equal(new Code(0), new Code(" "));
		}

		[Fact]
		public void IllegalCharactersInConstructorShouldThrow()
		{
			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code("1,000");
				}
			);
			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code("123");
				}
			);
			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code("1010IO");
				}
			);
			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code("😀");
				}
			);
		}
		#endregion

		#region Length tests
		[Fact]
		public void LengthShouldReturnLength()
		{
			// zeroed-out, length-based constructor
			Assert.Equal(1, new Code(1).Length);
			Assert.Equal(2, new Code(2).Length);
			Assert.Equal(3, new Code(3).Length);
			Assert.Equal(16, new Code(16).Length);
			Assert.Equal(127, new Code(127).Length);
			Assert.Equal(128, new Code(128).Length);
			Assert.Equal(4000, new Code(4000).Length);
			Assert.Equal(65536, new Code(65536).Length);

			// Bit[] constructor			
			Assert.Equal(1, new Code(new Bit[] { 1 }).Length);
			Assert.Equal(2, new Code(1, 0).Length);
			Assert.Equal(3, new Code(1, 0, 1).Length);
			Assert.Equal(4, new Code(1, 0, 1, 1).Length);
			Assert.Equal(9, new Code(1, 0, 1, 1, 0, 1, 1, 1, 0).Length);
			Assert.Equal(24, new Code(1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1, 0, 1).Length);
			Assert.Equal(500, new Code(Enumerable.Repeat(0, 500).Select(x => (Bit) x).ToArray()).Length);
			Assert.Equal(8192, new Code(Enumerable.Repeat(1, 8192).Select(x => (Bit)x).ToArray()).Length);

			// string constructor
			Assert.Equal(1, new Code("1").Length);
			Assert.Equal(2, new Code("10").Length);
			Assert.Equal(3, new Code("101").Length);
			Assert.Equal(4, new Code("1011").Length);
			Assert.Equal(9, new Code("1011 0111 0").Length);
			Assert.Equal(24, new Code("10110111 10010001 00101101").Length);
			Assert.Equal(500, new Code(new string('0', 500)).Length);
			Assert.Equal(8192, new Code(new string('1', 8192)).Length);

			// byte[] constructor
			Assert.Equal(1 * 8, new Code(new byte[] { 243 }).Length);
			Assert.Equal(2 * 8, new Code(new byte[] { 19, 221 }).Length);
			Assert.Equal(6 * 8, new Code(new byte[] { 1, 1, 1, 1, 1, 1 }).Length);
			Assert.Equal(343 * 8, new Code(Enumerable.Repeat<byte>(43, 343).ToArray()).Length);
		}
		#endregion

		#region Indexer tests
		[Fact]
		public void IndexerShouldGetCorrectValues()
		{
			Code code = new Code(1, 0, 0, 1, 0, 1, 0, 0, 1);
			Assert.Equal<Bit>(1, code[0]);
			Assert.Equal<Bit>(0, code[1]);
			Assert.Equal<Bit>(0, code[2]);
			Assert.Equal<Bit>(1, code[3]);
			Assert.Equal<Bit>(0, code[4]);
			Assert.Equal<Bit>(1, code[5]);
			Assert.Equal<Bit>(0, code[6]);
			Assert.Equal<Bit>(0, code[7]);
			Assert.Equal<Bit>(1, code[8]);
		}

		[Fact]
		public void CodeIndexerShouldGetCorrectValues()
		{
			Code code = new Code("1010101010");

			Assert.Equal("1", code[0, 1]);
		}

		#endregion

		#region empty code tests
		[Fact]
		[Trait("Region", "constructors")]
		public void AnEmptyCodeShouldBeAllowed()
		{
			// The code that says nothing. A schema with no parts produces one, and an event
			// meaning "this happened" is answered completely by naming which event it was.
			Assert.Equal(0, new Code(0).Length);
			Assert.Equal(0, new Code(new Bit[0]).Length);
			Assert.Equal(0, ((Code) "").Length);
			Assert.Equal(0, new CodeBuilder(0).Build().Length);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void EmptyCodesShouldBeEqualAndPrintAsNothing()
		{
			Assert.Equal(new Code(0), new Code(new Bit[0]));
			Assert.Equal(new Code(0).GetHashCode(), new Code(new Bit[0]).GetHashCode());
			Assert.Equal(string.Empty, new Code(0).ToString());
			Assert.NotEqual(new Code(0), new Code("0"));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AnEmptyCodeShouldHaveNoBytesAndNoBits()
		{
			Code empty = new Code(0);
		
			Assert.Equal(0, empty.ByteLength);
			Assert.Empty(empty.ToByteArray());
			Assert.Equal(0, empty.AsSpan().Length);
			Assert.Throws<ArgumentOutOfRangeException>(() => empty[0]);
		}

		[Fact]
		[Trait("Region", "indexers")]
		public void AZeroLengthSliceShouldBeTheEmptyCode()
		{
			Code code = new Code("1011");
		
			Assert.Equal(new Code(0), code[0, 0]);
			Assert.Equal(new Code(0), code[4, 0]);
			Assert.Throws<ArgumentOutOfRangeException>(() => code[5, 0]);
			Assert.Throws<ArgumentOutOfRangeException>(() => code[0, -1]);
		}
		#endregion

		#region reading tests
		[Fact]
		[Trait("Region", "methods")]
		public void GetByteShouldReturnTheByte()
		{
			Code code = new Code("10101010 11110000");
		
			Assert.Equal(2, code.ByteLength);
			Assert.Equal(code.ToByteArray()[0], code.GetByte(0));
			Assert.Equal(code.ToByteArray()[1], code.GetByte(1));
			Assert.Throws<ArgumentOutOfRangeException>(() => code.GetByte(2));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void CopyToShouldCopyTheRequestedBytes()
		{
			Code code = new Code("10101010 11110000 00001111");
			byte[] buffer = new byte[4];
		
			code.CopyTo(buffer, 1, 1, 2);
		
			Assert.Equal(0, buffer[0]);
			Assert.Equal(code.GetByte(1), buffer[1]);
			Assert.Equal(code.GetByte(2), buffer[2]);
			Assert.Equal(0, buffer[3]);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void CopyToShouldRejectRangesOutsideTheCodeOrTheBuffer()
		{
			Code code = new Code("10101010 11110000");
		
			Assert.Throws<ArgumentNullException>(() => code.CopyTo(null, 0, 0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => code.CopyTo(new byte[2], 0, 1, 2));
			Assert.Throws<ArgumentOutOfRangeException>(() => code.CopyTo(new byte[1], 0, 0, 2));
		}
		#endregion

		#region view tests
		[Fact]
		[Trait("Region", "methods")]
		public void AsSpanShouldSeeTheCodesBytes()
		{
			Code code = new Code("10101010 11110000");
		
			ReadOnlySpan<byte> span = code.AsSpan();
		
			Assert.Equal(code.ByteLength, span.Length);
			Assert.Equal(code.GetByte(0), span[0]);
			Assert.Equal(code.GetByte(1), span[1]);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AsMemoryShouldSeeTheCodesBytes()
		{
			Code code = new Code("10101010 11110000");
		
			ReadOnlyMemory<byte> memory = code.AsMemory();
		
			Assert.Equal(code.ByteLength, memory.Length);
			Assert.Equal(code.GetByte(0), memory.Span[0]);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AsSpanShouldZeroTheUnusedBitsOfTheFinalByte()
		{
			// Two codes of equal length must have identical views, so the bits past the end of
			// the code cannot be allowed to differ.
			Code fromString = new Code("101");
			Code fromBits = new Code(1, 0, 1);
		
			Assert.Equal(fromString.AsSpan()[0], fromBits.AsSpan()[0]);
			Assert.Equal(0, fromString.AsSpan()[0] & 0b1111_1000);
		}
		#endregion

		#region ToByteArray tests
		[Fact]
		[Trait("Region", "methods")]
		public void ToByteArrayShouldReturnACopy()
		{
			// A code is immutable, so it must not hand out its backing array.
			Code code = new Code("1010 1010");
		
			byte[] bytes = code.ToByteArray();
			bytes[0] = 0xFF;
		
			Assert.Equal<Code>("1010 1010", code);
			Assert.NotEqual((byte) 0xFF, code.ToByteArray()[0]);
		}
		#endregion

		#region object tests
		[Fact]
		public void ToStringShouldFormatCorrectly()
		{
			Code code = new Code(1, 1, 0, 0, 1, 0, 1, 0);
			Assert.Equal("11001010", code.ToString());
		}
		#endregion
	}
}
