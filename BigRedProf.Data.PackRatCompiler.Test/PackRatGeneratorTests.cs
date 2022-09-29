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
		#endregion
	}
}