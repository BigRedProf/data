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