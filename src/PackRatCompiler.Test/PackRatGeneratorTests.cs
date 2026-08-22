using BigRedProf.Data.Test._TestHelpers;

namespace BigRedProf.Data.PackRatCompiler.Test
{
	public class PackRatGeneratorTests : IClassFixture<CompilationContextFixture>
	{
		#region fields
		private readonly CompilationContextFixture _compilationContextFixture;
		#endregion

		#region constructors
		public PackRatGeneratorTests(CompilationContextFixture compilationContextFixture)
		{
			_compilationContextFixture = compilationContextFixture;
		}
		#endregion

		#region methods
		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForPointModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				_compilationContextFixture.CompilationContext,
				"_Resources/Models/Point.cs",
				"_Resources/ExpectedPackRats/PointPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldRejectAGapInFieldPositions()
		{
			// A gap in the declaration is not a gap on the wire. Positions 1 and 3 would generate
			// two sequential parts, so the part declared 3 simply becomes the second thing
			// written -- and every code already packed under this schema is then misread. To
			// remove a part, mint a new schema identifier.
			IReadOnlyList<(int Code, string Message)> errors =
				PackRatCompilerTestHelper.TestGeneratePackRatErrors(
					_compilationContextFixture.CompilationContext,
					"_Resources/Models/GappedFieldPositionTestModel.cs"
				);
		
			Assert.Single(errors);
			Assert.Equal(102, errors[0].Code);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForNullableTestModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				_compilationContextFixture.CompilationContext,
				"_Resources/Models/NullableTestModel.cs",
				"_Resources/ExpectedPackRats/NullableTestModelPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForListTestModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				_compilationContextFixture.CompilationContext,
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
				_compilationContextFixture.CompilationContext,
				"_Resources/Models/MixedListTestModel.cs",
				"_Resources/ExpectedPackRats/MixedListTestModelPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForModelWithEnum()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				_compilationContextFixture.CompilationContext,
				"_Resources/Models/ModelWithEnum.cs",
				"_Resources/ExpectedPackRats/ModelWithEnumPackRat.cs"
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GeneratePackRat_ShouldWorkForTokenTestModelModel()
		{
			PackRatCompilerTestHelper.TestGeneratePackRat(
				_compilationContextFixture.CompilationContext,
				"_Resources/Models/TokenTestModel.cs",
				"_Resources/ExpectedPackRats/TokenTestModelPackRat.cs"
			);
		}
		#endregion
	}
}
