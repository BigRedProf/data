using BigRedProf.Data.Core;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Core.Test.Internal.PackRats
{
	public class FlexDatumPackRatTests
	{
		#region constants
		private const string Trait1 = "00000000-0000-0000-0000-000000000001";
		private const string Trait2 = "00000000-0000-0000-0000-000000000002";
		private const string TraitText = "00000000-0000-0000-0000-000000000009";

		// The reader below never learns what this asks or how to read its answer. That is the
		// entire point: it must still get everything else, and hand this back untouched.
		private const string FutureTrait = "ffffffff-0000-0000-0000-00000000000f";
		#endregion

		#region test helpers
		private static IPiedPiper CreateWriterPiedPiper()
		{
			IPiedPiper piedPiper = CreateReaderPiedPiper();
			piedPiper.DefineTrait(new Trait(FutureTrait, CoreSchema.TextUtf8));
			return piedPiper;
		}

		private static IPiedPiper CreateReaderPiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineTrait(new Trait(Trait1, CoreSchema.Int32));
			piedPiper.DefineTrait(new Trait(Trait2, CoreSchema.Int32));
			piedPiper.DefineTrait(new Trait(TraitText, CoreSchema.TextUtf8));
			return piedPiper;
		}
		#endregion

		#region PackRat tests
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = CreateReaderPiedPiper();
			PackRat<FlexDatum> packRat = piedPiper.GetPackRat<FlexDatum>(CoreSchema.FlexDatum);

			Assert.Throws<ArgumentNullException>(
				() => packRat.PackModel(null, new FlexDatumBuilder(piedPiper).Build())
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldWorkForAnEmptyFlexDatum()
		{
			IPiedPiper piedPiper = CreateReaderPiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper).Build();

			Code code = piedPiper.PackModel<FlexDatum>(flexDatum, CoreSchema.FlexDatum);

			Assert.Equal(flexDatum, piedPiper.UnpackModel<FlexDatum>(code, CoreSchema.FlexDatum));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldWorkForAFlexDatumWithTraits()
		{
			IPiedPiper piedPiper = CreateReaderPiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 43)
				.AddTrait(Trait2, 95)
				.AddTrait(TraitText, "Lincoln, Nebraska")
				.Build();

			Code code = piedPiper.PackModel<FlexDatum>(flexDatum, CoreSchema.FlexDatum);
			FlexDatum roundTripped = piedPiper.UnpackModel<FlexDatum>(code, CoreSchema.FlexDatum);

			Assert.Equal(flexDatum, roundTripped);
			Assert.Equal(43, roundTripped.GetTrait<int>(Trait1, piedPiper));
			Assert.Equal("Lincoln, Nebraska", roundTripped.GetTrait<string>(TraitText, piedPiper));
		}
		#endregion

		#region partial understanding tests
		[Fact]
		[Trait("Region", "partial understanding")]
		public void UnpackModel_ShouldSucceedWhenATraitIsUnrecognised()
		{
			IPiedPiper writer = CreateWriterPiedPiper();
			IPiedPiper reader = CreateReaderPiedPiper();

			Code code = writer.PackModel<FlexDatum>(
				new FlexDatumBuilder(writer)
					.AddTrait(Trait1, 43)
					.AddTrait(FutureTrait, "written by a later vocabulary")
					.Build(),
				CoreSchema.FlexDatum
			);

			FlexDatum asRead = reader.UnpackModel<FlexDatum>(code, CoreSchema.FlexDatum);

			// It reads, and what the reader understands it can use.
			Assert.Equal(2, asRead.Count);
			Assert.Equal(43, asRead.GetTrait<int>(Trait1, reader));
		}

		[Fact]
		[Trait("Region", "partial understanding")]
		public void UnpackModel_ShouldReportAnUnrecognisedTraitOnlyWhenItIsInterpreted()
		{
			IPiedPiper writer = CreateWriterPiedPiper();
			IPiedPiper reader = CreateReaderPiedPiper();

			Code code = writer.PackModel<FlexDatum>(
				new FlexDatumBuilder(writer)
					.AddTrait(Trait1, 43)
					.AddTrait(FutureTrait, "written by a later vocabulary")
					.Build(),
				CoreSchema.FlexDatum
			);

			FlexDatum asRead = reader.UnpackModel<FlexDatum>(code, CoreSchema.FlexDatum);

			Assert.True(asRead.HasTrait(FutureTrait));
			Assert.ThrowsAny<Exception>(() => asRead.GetTrait<string>(FutureTrait, reader));
		}

		[Fact]
		[Trait("Region", "partial understanding")]
		public void UnpackModifyRepack_ShouldPreserveAnUnrecognisedTrait()
		{
			IPiedPiper writer = CreateWriterPiedPiper();
			IPiedPiper reader = CreateReaderPiedPiper();

			FlexDatum original = new FlexDatumBuilder(writer)
				.AddTrait(Trait1, 43)
				.AddTrait(FutureTrait, "written by a later vocabulary")
				.Build();
			Code code = writer.PackModel<FlexDatum>(original, CoreSchema.FlexDatum);

			// The older reader revises the one answer it understands and writes the record back.
			FlexDatum asRead = reader.UnpackModel<FlexDatum>(code, CoreSchema.FlexDatum);
			FlexDatum revised = asRead.ToBuilder(reader).AddTrait(Trait1, 44).Build();
			Code revisedCode = reader.PackModel<FlexDatum>(revised, CoreSchema.FlexDatum);

			// The later vocabulary gets its own trait back, bit for bit.
			FlexDatum asReadByWriter = writer.UnpackModel<FlexDatum>(revisedCode, CoreSchema.FlexDatum);
			Assert.Equal(44, asReadByWriter.GetTrait<int>(Trait1, writer));
			Assert.Equal(
				"written by a later vocabulary",
				asReadByWriter.GetTrait<string>(FutureTrait, writer)
			);

			original.TryGetTraitCode(FutureTrait, out Code before);
			asReadByWriter.TryGetTraitCode(FutureTrait, out Code after);
			Assert.Equal(before, after);
		}

		[Fact]
		[Trait("Region", "partial understanding")]
		public void UnpackModel_ShouldLeaveTheReaderPositionedAfterAFlexDatumWithUnrecognisedTraits()
		{
			IPiedPiper writer = CreateWriterPiedPiper();
			IPiedPiper reader = CreateReaderPiedPiper();

			Code stream =
				writer.PackModel<FlexDatum>(
					new FlexDatumBuilder(writer).AddTrait(FutureTrait, "opaque").Build(),
					CoreSchema.FlexDatum
				)
				+ writer.PackModel<FlexDatum>(
					new FlexDatumBuilder(writer).AddTrait(Trait1, 7).Build(),
					CoreSchema.FlexDatum
				);

			using (CodeReader codeReader = new CodeReader(new MemoryStream(stream.ToByteArray())))
			{
				FlexDatum first = reader.UnpackModel<FlexDatum>(codeReader, CoreSchema.FlexDatum);
				FlexDatum second = reader.UnpackModel<FlexDatum>(codeReader, CoreSchema.FlexDatum);

				Assert.True(first.HasTrait(FutureTrait));
				Assert.Equal(7, second.GetTrait<int>(Trait1, reader));
			}
		}
		#endregion
	}
}
