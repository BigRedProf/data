using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class FlexModelPackRatTests
	{
		private IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			DefineTraits(piedPiper);
			return piedPiper;
		}

		private void DefineTraits(IPiedPiper piedPiper)
		{
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000002", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000003", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000005", CoreSchema.Int32));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModelPackRat packRat = new FlexModelPackRat(piedPiper);
			FlexModel model = new FlexModel();

			Assert.Throws<ArgumentNullException>(() => packRat.PackModel(null, model));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModelPackRat packRat = new FlexModelPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() => packRat.UnpackModel(null));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldWorkForEmptyModel()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModelPackRat packRat = new FlexModelPackRat(piedPiper);
			FlexModel model = new FlexModel();

			// Placeholder value for expected code. Replace with the correct value after testing.
			Code expectedCode =
				"1000" // trait length (0)
			;
			PackRatTestHelper.TestPackModel(packRat, model, expectedCode);
			PackRatTestHelper.TestUnpackModelCodeOnlyNoEquals(packRat, expectedCode);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModelAndUnpackModel_ShouldWorkForModelWithTraits()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModelPackRat packRat = new FlexModelPackRat(piedPiper);
			FlexModel model = new FlexModel();
			string traitId1 = "00000000-0000-0000-0000-000000000002";
			string traitId2 = "00000000-0000-0000-0000-000000000003";
			string traitId3 = "00000000-0000-0000-0000-000000000005";

			Trait<int> trait1 = new Trait<int>(traitId1, 10);
			Trait<int> trait2 = new Trait<int>(traitId2, 20);
			Trait<int> trait3 = new Trait<int>(traitId3, 30);

			model.AddTrait(trait1);
			model.AddTrait(trait2);
			model.AddTrait(trait3);

			// Placeholder value for expected code. Replace with the correct value after testing.
			Code expectedCode = 
				"1011" // trait length (3)
				+ "0000"	// byte-align
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000"
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 01000000" // trait 1 identifier (2)
				+ "00000100 00000000 00000000 00000000"	// trait 1 Int32 length (32)
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000"
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 11000000" // trait 2 identifier (3)
				+ "00000100 00000000 00000000 00000000"	// trait 2 Int32 length (32)
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000"
				+ "00000000 00000000 00000000 00000000 00000000 00000000 00000000 10100000" // trait 3 identifier (5)
				+ "00000100 00000000 00000000 00000000"	// trait 3 Int32 length (32)
				+ "01010000 00000000 00000000 00000000"	// 1st Int32 model (10)
				+ "00101000 00000000 00000000 00000000"	// 2nd Int32 model (20)
				+ "01111000 00000000 00000000 00000000"	// 3rd Int32 model (30)
			;
			PackRatTestHelper.TestPackModel(packRat, model, expectedCode);
			PackRatTestHelper.TestUnpackModelCodeOnlyNoEquals(packRat, expectedCode);
		}
	}
}
