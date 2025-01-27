using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class BitTests
	{
		#region object methods
		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_0ShouldEqual0()
		{
			Assert.Equal(Bit.Zero, Bit.Zero);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_1ShouldEqual1()
		{
			Assert.Equal(Bit.One, Bit.One);
		}


		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_0ShouldNotEqual1()
		{
			Assert.NotEqual(Bit.Zero, Bit.One);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_1ShouldNotEqual0()
		{
			Assert.NotEqual(Bit.One, Bit.Zero);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void GetHashCode_0And1ShouldHaveDifferentHashCodes()
		{
			Assert.NotEqual(Bit.Zero.GetHashCode(), Bit.One.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void GetHashCode_0And0ShouldHaveTheSameHashCode()
		{
			Assert.Equal(Bit.Zero.GetHashCode(), Bit.Zero.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void GetHashCode_1And1ShouldHaveTheSameHashCode()
		{
			Assert.Equal(Bit.One.GetHashCode(), Bit.One.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void ToString_0ShouldWork()
		{
			string actual = Bit.Zero.ToString();
			Assert.Equal("0", actual);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void ToString_1ShouldWork()
		{
			string actual = Bit.One.ToString();
			Assert.Equal("1", actual);
		}
		#endregion

		#region operator overloads
		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorEqualEqual_0ShouldEqual0()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.True(Bit.Zero == Bit.Zero);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorEqualEqual_1ShouldEqual1()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.True(Bit.One == Bit.One);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorEqualEqual_0ShouldNotEqual1()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.False(Bit.Zero == Bit.One);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorEqualEqual_1ShouldNotEqual0()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.False(Bit.One == Bit.Zero);
#pragma warning restore CS1718 // Comparison made to same variable
		}
		
		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorNotEqual_0ShouldNotEqual1()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.True(Bit.Zero != Bit.One);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorNotEqual_1ShouldNotEqual0()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.True(Bit.One != Bit.Zero);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorNotEqual_0ShouldNotNotEqual0()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.False(Bit.Zero != Bit.Zero);
#pragma warning restore CS1718 // Comparison made to same variable
		}

		[Fact]
		[Trait("Region", "operator overloads")]
		public void OperatorNotEqual_1ShouldNotNotEqual1()
		{
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.False(Bit.One != Bit.One);
#pragma warning restore CS1718 // Comparison made to same variable
		}
		#endregion

		#region casts
		[Fact]
		[Trait("Region", "casts")]
		public void ImplictInt32ToBitCast_ShouldWork()
		{
			int zeroAsInt32 = 0;
			int oneAsInt32 = 1;

			Assert.Equal<Bit>(Bit.Zero, zeroAsInt32);
			Assert.Equal<Bit>(Bit.One, oneAsInt32);
		}

		[Fact]
		[Trait("Region", "casts")]
		public void ImplictBitToInt32Cast_ShouldWork()
		{
			int zeroAsInt32 = 0;
			int oneAsInt32 = 1;

			Assert.Equal<int>(zeroAsInt32, Bit.Zero);
			Assert.Equal<int>(oneAsInt32, Bit.One);
		}

		[Fact]
		[Trait("Region", "casts")]
		public void ImplicitInt32ToBitCast_ShouldThrowWhenArgumentIsOutOfRange()
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
