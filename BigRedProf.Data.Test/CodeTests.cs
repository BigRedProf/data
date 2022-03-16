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
			Assert.Equal<Bit>(1, code[7]);

			Assert.Equal<Bit>(1, code[8]);
			Assert.Equal<Bit>(1, code[9]);
			Assert.Equal<Bit>(0, code[10]);
			Assert.Equal<Bit>(1, code[11]);
			Assert.Equal<Bit>(1, code[12]);
			Assert.Equal<Bit>(0, code[13]);
			Assert.Equal<Bit>(1, code[14]);
			Assert.Equal<Bit>(0, code[15]);
		}
		#endregion

		#region main tests
		[Fact]
		public void ZeroLengthCodesShouldThrow()
		{
			Assert.Throws<ArgumentException>(
				() =>
				{
					new Code();
				}
			);
		}

		[Fact]
		public void LengthShouldReturnLength()
		{
			Assert.Equal(1, new Code(1).Length);
			Assert.Equal(2, new Code(1, 0).Length);
			Assert.Equal(3, new Code(1, 0, 1).Length);
			Assert.Equal(4, new Code(1, 0, 1, 1).Length);
			Assert.Equal(500, new Code(Enumerable.Repeat(0, 500).Select(x => (Bit) x).ToArray()).Length);
			Assert.Equal(8192, new Code(Enumerable.Repeat(1, 8192).Select(x => (Bit)x).ToArray()).Length);
		}

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
