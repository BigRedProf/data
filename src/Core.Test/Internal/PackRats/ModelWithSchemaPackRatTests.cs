using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class ModelWithSchemaPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			ModelWithSchemaPackRat packRat = new ModelWithSchemaPackRat(piedPiper);
			ModelWithSchema model = null;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWork()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<ModelWithSchema> packRat = new ModelWithSchemaPackRat(piedPiper);
			ModelWithSchema model = new ModelWithSchema()
			{
				SchemaId = CoreSchema.TextUtf8,
				Model = "43"
			};

			Code expectedCode = 
				piedPiper.EncodeModel<Guid>(new Guid(CoreSchema.TextUtf8), CoreSchema.Guid)	// TextUtf8 identifier
				+ "1001" // 2 (length of "43" in EfficientWholeNumber31)
				+ "0000" // align to byte
				+ "00101100 11001100";  // '4' '3' (UTF8)

			PackRatTestHelper.TestPackModel<ModelWithSchema>(
				packRat, 
				model,
				expectedCode
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			ModelWithSchemaPackRat packRat = new ModelWithSchemaPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForTrue()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<ModelWithSchema> packRat = new ModelWithSchemaPackRat(piedPiper);
			ModelWithSchema model = new ModelWithSchema()
			{
				SchemaId = CoreSchema.TextUtf8,
				Model = "43"
			};

			Code expectedCode =
				piedPiper.EncodeModel<Guid>(new Guid(CoreSchema.TextUtf8), CoreSchema.Guid) // TextUtf8 identifier
				+ "1001" // 2 (length of "43" in EfficientWholeNumber31)
				+ "0000" // align to byte
				+ "00101100 11001100";  // '4' '3' (UTF8)

			PackRatTestHelper.TestUnpackModel<ModelWithSchema>(packRat, expectedCode, model);
		}
		#endregion
	}
}
