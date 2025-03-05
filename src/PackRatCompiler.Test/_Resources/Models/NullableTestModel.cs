using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[GeneratePackRat("b8a0492a-c5e6-4955-819b-f47797005105")]
	public class NullableTestModel
	{
		[PackField(1, CoreSchema.TextUtf8, IsNullable=true)]
		public string? ExplicitlyNullableField { get; set; }

		[PackField(2, CoreSchema.TextUtf8, IsNullable = false)]
		public string? ExplicitlyNonNullableField { get; set; }

		[PackField(3, CoreSchema.TextUtf8)]
		public string? ImplicitlyNullableField { get; set; }

		[PackField(4, CoreSchema.TextUtf8)]
		public string ImplicitlyNonNullableField { get; set; }
	}
}
