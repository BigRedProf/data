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
		#endregion
	}
}