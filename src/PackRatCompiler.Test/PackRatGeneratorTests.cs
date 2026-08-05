using BigRedProf.Data.Test._TestHelpers;

namespace BigRedProf.Data.PackRatCompiler.Test
{
	public class PackRatGeneratorTests
	{
		#region methods
		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForPointModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/Point.cs",
				"_Resources/ExpectedPackRats/PointPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForNullableTestModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/NullableTestModel.cs",
				"_Resources/ExpectedPackRats/NullableTestModelPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForListTestModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/ListTestModel.cs",
				"_Resources/ExpectedPackRats/ListTestModelPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForMixedListTestModel()
		{
			// ListTestModel declares every list member as a property, so this model is the
			// only coverage for a list declared as a plain field -- and for the two forms
			// interleaved, which also exercises ordering by Position rather than by
			// declaration order.
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/MixedListTestModel.cs",
				"_Resources/ExpectedPackRats/MixedListTestModelPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForModelWithEnum()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/ModelWithEnum.cs",
				"_Resources/ExpectedPackRats/ModelWithEnumPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForTokenTestModelModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				"_Resources/Models/TokenTestModel.cs",
				"_Resources/ExpectedPackRats/TokenTestModelPackRat.cs"
			);
		}
		#endregion
	}
}