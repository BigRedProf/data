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

		#region test helpers
		private enum TestSeat
		{
			Window = 1,
			Aisle = 2
		}
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
		public void PackDatum_ShouldWorkWhenTheModelIsHeldAsObject()
		{
			// The ordinary case when packing whatever arrived: a service takes an object and a
			// schema identifier and has no type to name.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			object model = "Memorial Stadium";
		
			Datum datum = piedPiper.PackDatum(model, CoreSchema.TextUtf8);
		
			Assert.Equal("Memorial Stadium", datum.Unpack<string>(piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Unpack_ShouldWorkWithoutKnowingTheModelType()
		{
			// The ordinary case for a datum that arrived from somewhere: the reader knows how to
			// read it, because the datum says, but not what type that is until it looks.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			Datum datum = piedPiper.PackDatum<string>("Memorial Stadium", CoreSchema.TextUtf8);
		
			object model = datum.Unpack<object>(piedPiper);
		
			Assert.Equal("Memorial Stadium", model);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Unpack_ShouldWorkForAnEnumOverAnIntegralSchema()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			Datum datum = piedPiper.PackDatum<int>((int) TestSeat.Aisle, CoreSchema.Int32);
		
			Assert.Equal(TestSeat.Aisle, datum.Unpack<TestSeat>(piedPiper));
			Assert.Equal((int) TestSeat.Aisle, datum.Unpack<int>(piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Unpack_ShouldThrowWhenTheModelIsNotTheRequestedType()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			Datum datum = piedPiper.PackDatum<string>("Lincoln", CoreSchema.TextUtf8);
		
			Assert.Throws<InvalidOperationException>(() => datum.Unpack<int>(piedPiper));
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

		#region empty code tests
		[Fact]
		[Trait("Region", "methods")]
		public void ADatumMayHaveAnEmptyCode()
		{
			// A schema with no parts: the event means "this happened" and the schema identifier
			// carries the whole message. Digihouse signals GestureInitiated exactly this way.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			Datum datum = new Datum(CoreSchema.TextUtf8, new Code(0));
			Code code = piedPiper.PackModel<Datum>(datum, CoreSchema.Datum);
			Datum roundTripped = piedPiper.UnpackModel<Datum>(code, CoreSchema.Datum);
		
			Assert.Equal(datum, roundTripped);
			Assert.Equal(0, roundTripped.Code.Length);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AnEmptyDatumShouldNotSwallowWhatFollowsIt()
		{
			// The framing has to survive a payload of nothing, or one such event would eat the
			// next thing in the stream.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
		
			Datum nothingToSay = new Datum(CoreSchema.TextUtf8, new Code(0));
			Datum somethingToSay = piedPiper.PackDatum<string>("Lincoln", CoreSchema.TextUtf8);
		
			Code stream =
				piedPiper.PackModel<Datum>(nothingToSay, CoreSchema.Datum)
				+ piedPiper.PackModel<Datum>(somethingToSay, CoreSchema.Datum);
		
			using (CodeReader reader = new CodeReader(new System.IO.MemoryStream(stream.ToByteArray())))
			{
				Assert.Equal(nothingToSay, piedPiper.UnpackModel<Datum>(reader, CoreSchema.Datum));
				Assert.Equal("Lincoln", piedPiper.UnpackModel<Datum>(reader, CoreSchema.Datum).Unpack<string>(piedPiper));
			}
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
