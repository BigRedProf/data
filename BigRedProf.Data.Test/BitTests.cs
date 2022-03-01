using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class BitTests
	{
		#region equality tests
		[Fact]
		public void BitZeroShouldEqualBitZero()
		{
			Assert.Equal(Bit.Zero, Bit.Zero);
		}

		[Fact]
		public void BitOneShouldEqualBitOne()
		{
			Assert.Equal(Bit.One, Bit.One);
		}


		[Fact]
		public void BitZeroShouldNotEqualBitOne()
		{
			Assert.NotEqual(Bit.Zero, Bit.One);
		}

		[Fact]
		public void BitOneShouldNotEqualBitZero()
		{
			Assert.NotEqual(Bit.One, Bit.Zero);
		}
		#endregion

		#region object tests
		[Fact]
		public void BitZeroAndBitOneHashCodesShouldBeDifferent()
		{
			Assert.NotEqual(Bit.Zero.GetHashCode(), Bit.One.GetHashCode());
		}

		[Fact]
		public void ZeroBitToStringShouldEqual0()
		{
			string actual = Bit.Zero.ToString();
			Assert.Equal("0", actual);
		}

		[Fact]
		public void OneBitToStringShouldEqual1()
		{
			string actual = Bit.One.ToString();
			Assert.Equal("1", actual);
		}
		#endregion

		#region cast tests
		[Fact]
		public void ImplictBitCastsShouldWork()
		{
			int zeroAsInt32 = 0;
			int oneAsInt32 = 1;

			Assert.Equal<Bit>(Bit.Zero, zeroAsInt32);
			Assert.Equal<Bit>(Bit.One, oneAsInt32);

			Assert.Equal<int>(zeroAsInt32, Bit.Zero);
			Assert.Equal<int>(oneAsInt32, Bit.One);
		}

		[Fact]
		public void OutOfRangeImplicitBitCastsShouldThrow()
		{
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					Bit bit = -1;
				}
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					Bit bit = 2;
				}
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					Bit bit = 9990234;
				}
			);
		}
		#endregion
	}
}
