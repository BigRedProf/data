using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class ModelWithSchemaAndLengthPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			ModelWithSchemaAndLengthPackRat packRat = new ModelWithSchemaAndLengthPackRat(piedPiper);
			ModelWithSchemaAndLength model = null;

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
			PackRat<ModelWithSchemaAndLength> packRat = new ModelWithSchemaAndLengthPackRat(piedPiper);
			ModelWithSchemaAndLength model = new ModelWithSchemaAndLength()
			{
				SchemaId = CoreSchema.TextUtf8,
				Length = 24,	// 4 (length) + 4 (align to byte) + 16 ('4' '3')
				Model = "43"
			};

			Code expectedCode = 
				piedPiper.EncodeModel<Guid>(new Guid(CoreSchema.TextUtf8), CoreSchema.Guid)	// TextUtf8 identifier
				+ piedPiper.EncodeModel<int>(24, CoreSchema.EfficientWholeNumber31)	// Length (of the encoded model)
				+ "1001" // 2 (length of "43" in EfficientWholeNumber31)
				+ "0000" // align to byte
				+ "00101100 11001100";  // '4' '3' (UTF8)

			PackRatTestHelper.TestPackModel<ModelWithSchemaAndLength>(
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
			ModelWithSchemaAndLengthPackRat packRat = new ModelWithSchemaAndLengthPackRat(piedPiper);

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
			PackRat<ModelWithSchemaAndLength> packRat = new ModelWithSchemaAndLengthPackRat(piedPiper);
			ModelWithSchemaAndLength model = new ModelWithSchemaAndLength()
			{
				SchemaId = CoreSchema.TextUtf8,
				Length = 24,    // 4 (length) + 4 (align to byte) + 16 ('4' '3')
				Model = "43"
			};

			Code expectedCode =
				piedPiper.EncodeModel<Guid>(new Guid(CoreSchema.TextUtf8), CoreSchema.Guid) // TextUtf8 identifier
				+ piedPiper.EncodeModel<int>(24, CoreSchema.EfficientWholeNumber31)   // Length (of the encoded model)
				+ "1001" // 2 (length of "43" in EfficientWholeNumber31)
				+ "0000" // align to byte
				+ "00101100 11001100";  // '4' '3' (UTF8)

			PackRatTestHelper.TestUnpackModel<ModelWithSchemaAndLength>(packRat, expectedCode, model);
		}
		#endregion
	}
}
