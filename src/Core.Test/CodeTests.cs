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
		public void ZeroLengthCodesShouldThrow()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					new Code();
				}
			);

			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code(" ");
				}
			);
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
		public void IndexerShouldSetCorrectValues()
		{
			Code code = new Code(0, 0, 0, 0, 0, 0, 0, 0, 0);

			code[0] = 0;
			Assert.Equal<Bit>(0, code[0]);

			code[1] = 1;
			Assert.Equal<Bit>(1, code[1]);

			code[2] = 0;
			Assert.Equal<Bit>(0, code[2]);

			code[3] = 1;
			Assert.Equal<Bit>(1, code[3]);

			code[4] = 0;
			Assert.Equal<Bit>(0, code[4]);

			code[5] = 1;
			Assert.Equal<Bit>(1, code[5]);

			code[6] = 0;
			Assert.Equal<Bit>(0, code[6]);

			code[7] = 0;
			Assert.Equal<Bit>(0, code[7]);

			code[8] = 1;
			Assert.Equal<Bit>(1, code[8]);
		}

		[Fact]
		public void IndexerShouldChangeValues()
		{
			Code code = new Code(1, 0, 0, 1, 0, 1);

			code[0] = 0;
			Assert.Equal<Bit>(0, code[0]);

			code[1] = 1;
			Assert.Equal<Bit>(1, code[1]);

			code[2] = 0;
			Assert.Equal<Bit>(0, code[2]);

			code[3] = 1;
			Assert.Equal<Bit>(1, code[3]);

			code[4] = 0;
			Assert.Equal<Bit>(0, code[4]);

			code[5] = 1;
			Assert.Equal<Bit>(1, code[5]);
		}

		[Fact]
		public void CodeIndexerShouldGetCorrectValues()
		{
			Code code = new Code("1010101010");

			Assert.Equal("1", code[0, 1]);
		}

		[Fact]
		public void CodeIndexerShouldSetCorrectValues()
		{
			Code code = "0000 0000 0000";

			code[0, 4] = "1010";
			code[4, 4] = "1111";
			code[8, 4] = "1100";

			Code expectedCode = "1010 1111 1100";
			Assert.Equal(expectedCode, code);
		}

		[Fact]
		public void CodeIndexerShouldChangeValues()
		{
			Code code = new Code(0, 0, 0, 0, 1, 1, 1, 1);

			code[0, 3] = new Code("101");
			code[5, 3] = new Code("010");

			Code expectedCode = new Code(1, 0, 1, 0, 1, 0, 1, 0);
			Assert.Equal<Code>(expectedCode, code);
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
