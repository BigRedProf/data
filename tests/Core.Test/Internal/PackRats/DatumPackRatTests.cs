using BigRedProf.Data.Core;
using System;
using Xunit;

namespace BigRedProf.Data.Core.Test.Internal.PackRats
{
	public class DatumPackRatTests
	{
		#region constants
		// Deliberately never registered. A datum under this schema must still survive a full
		// round trip -- that is what "an unreadable datum is opaque, not corrupt" means.
		private const string UnknownSchemaId = "3d5b0d2c-9f14-4a6e-8f52-0c9a2f7d61b4";
		#endregion

		#region PackRat tests
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			PackRat<Datum> packRat = piedPiper.GetPackRat<Datum>(CoreSchema.Datum);
			Assert.Throws<ArgumentNullException>(
				() => packRat.PackModel(null, new Datum(CoreSchema.TextUtf8, "1"))
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldWork()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Datum datum = piedPiper.PackDatum<int>(1_234, CoreSchema.EfficientWholeNumber31);
			Code code = piedPiper.PackModel<Datum>(datum, CoreSchema.Datum);
			Datum roundTripped = piedPiper.UnpackModel<Datum>(code, CoreSchema.Datum);

			Assert.Equal(datum, roundTripped);
			Assert.Equal(1_234, roundTripped.Unpack<int>(piedPiper));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldRoundTripAnUnknownSchema()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Datum datum = new Datum(UnknownSchemaId, "1101001");
			Code code = piedPiper.PackModel<Datum>(datum, CoreSchema.Datum);
			Datum roundTripped = piedPiper.UnpackModel<Datum>(code, CoreSchema.Datum);

			Assert.Equal(datum, roundTripped);
			Assert.Equal<Code>("1101001", roundTripped.Code);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldLeaveTheReaderPositionedAfterAnUnknownDatum()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			// An unreadable datum must not poison what follows it. Write one, then something
			// ordinary, and read straight past the first to get the second.
			Datum opaque = new Datum(UnknownSchemaId, "10011");
			Datum readable = piedPiper.PackDatum<string>("Lincoln", CoreSchema.TextUtf8);

			Code stream =
				piedPiper.PackModel<Datum>(opaque, CoreSchema.Datum)
				+ piedPiper.PackModel<Datum>(readable, CoreSchema.Datum);

			using (CodeReader reader = new CodeReader(new System.IO.MemoryStream(stream.ToByteArray())))
			{
				Datum first = piedPiper.UnpackModel<Datum>(reader, CoreSchema.Datum);
				Datum second = piedPiper.UnpackModel<Datum>(reader, CoreSchema.Datum);

				Assert.Equal(opaque, first);
				Assert.Equal(readable, second);
				Assert.Equal("Lincoln", second.Unpack<string>(piedPiper));
			}
		}
		#endregion
	}
}
