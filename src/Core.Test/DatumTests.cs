using BigRedProf.Data.Core;
using System;
using Xunit;

namespace BigRedProf.Data.Core.Test
{
	public class DatumTests
	{
		#region constants
		// A schema this pied piper will never have a pack rat for. Nothing may depend on that
		// changing, which is the entire point of the tests below.
		private const string UnknownSchemaId = "7b1d54a4-2f0e-4f5a-9c3f-1a5a1d0b8e21";
		#endregion

		#region constructor tests
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowWhenSchemaIdIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Datum(null, "1010"));
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowWhenCodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new Datum(CoreSchema.TextUtf8, null));
		}
		#endregion

		#region method tests
		[Fact]
		[Trait("Region", "methods")]
		public void Unpack_ShouldYieldTheModel()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Datum datum = piedPiper.PackDatum<string>("Memorial Stadium", CoreSchema.TextUtf8);

			Assert.Equal("Memorial Stadium", datum.Unpack<string>(piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Unpack_ShouldThrowWhenSchemaIsNotRegistered()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Datum datum = new Datum(UnknownSchemaId, "1010");

			// The failure belongs here, where a caller asked to interpret something, and not
			// at unpack time, where it would take the whole stream down with it.
			Assert.ThrowsAny<Exception>(() => datum.Unpack<string>(piedPiper));
		}
		#endregion

		#region equality tests
		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldBeStructural()
		{
			Datum a = new Datum(CoreSchema.TextUtf8, "10110");
			Datum b = new Datum(CoreSchema.TextUtf8, "10110");

			Assert.Equal(a, b);
			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldDistinguishSchema()
		{
			Datum a = new Datum(CoreSchema.TextUtf8, "10110");
			Datum b = new Datum(CoreSchema.TextAscii, "10110");

			Assert.NotEqual(a, b);
			Assert.True(a != b);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldDistinguishCode()
		{
			Datum a = new Datum(CoreSchema.TextUtf8, "10110");
			Datum b = new Datum(CoreSchema.TextUtf8, "10111");

			Assert.NotEqual(a, b);
		}
		#endregion
	}
}
