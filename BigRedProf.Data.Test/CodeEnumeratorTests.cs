using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class CodeEnumeratorTests
	{
		#region constructors
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowWhenCodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new CodeEnumerator(null);
				}
			);
		}
		#endregion

		#region IEnumerator properties
		[Fact]
		[Trait("Region", "IEnumerator properties")]
		public void Current_ShouldThrowBeforeFirstMoveNext()
		{
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					Code code = "0001";
					CodeEnumerator enumerator = new CodeEnumerator(code);

					Bit bit = enumerator.Current;
				}
			);
		}

		[Fact]
		[Trait("Region", "IEnumerator properties")]
		public void Current_ShouldThrowAfterMoveNextReturnsFalse()
		{
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					Code code = "0001";
					CodeEnumerator enumerator = new CodeEnumerator(code);

					while(enumerator.MoveNext())
					{
					}

					Bit bit = enumerator.Current;
				}
			);
		}
		[Fact]
		[Trait("Region", "IEnumerator properties")]
		public void Current_ShouldReturnExpectedValues()
		{
			Code code = "0001";
			CodeEnumerator enumerator = new CodeEnumerator(code);

			enumerator.MoveNext();
			Assert.Equal<Bit>(0, enumerator.Current);

			enumerator.MoveNext();
			Assert.Equal<Bit>(0, enumerator.Current);

			enumerator.MoveNext();
			Assert.Equal<Bit>(0, enumerator.Current);

			enumerator.MoveNext();
			Assert.Equal<Bit>(1, enumerator.Current);
		}
		#endregion

		#region IEnumerator methods
		[Fact]
		[Trait("Region", "IEnumerator methods")]
		public void MoveNext_ShouldThrowAfterLastTrueMoveNext()
		{
			Code code = "0001";
			CodeEnumerator enumerator = new CodeEnumerator(code);

			while(enumerator.MoveNext())
			{
			}

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					enumerator.MoveNext();
				}
			);
		}

		[Fact]
		[Trait("Region", "IEnumerator methods")]
		public void MoveNext_ShouldReturnCorrectResults()
		{
			Code code = "0001";
			CodeEnumerator enumerator = new CodeEnumerator(code);

			bool result = enumerator.MoveNext();
			Assert.True(result);

			result = enumerator.MoveNext();
			Assert.True(result);

			result = enumerator.MoveNext();
			Assert.True(result);

			result = enumerator.MoveNext();
			Assert.True(result);

			result = enumerator.MoveNext();
			Assert.False(result);
		}

		[Fact]
		[Trait("Region", "IEnumerator methods")]
		public void Reset_ShouldWork()
		{
			Code code = "0001";
			CodeEnumerator enumerator = new CodeEnumerator(code);
			while (enumerator.MoveNext()) ;

			enumerator.Reset();

			Assert.True(enumerator.MoveNext());
			Assert.Equal<Bit>(0, enumerator.Current);

			Assert.True(enumerator.MoveNext());
			Assert.Equal<Bit>(0, enumerator.Current);

			Assert.True(enumerator.MoveNext());
			Assert.Equal<Bit>(0, enumerator.Current);

			Assert.True(enumerator.MoveNext());
			Assert.Equal<Bit>(1, enumerator.Current);

			Assert.False(enumerator.MoveNext());
		}
		#endregion
	}
}
