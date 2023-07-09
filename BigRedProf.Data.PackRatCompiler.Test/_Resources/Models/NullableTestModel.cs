using BigRedProf.Data;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[RegisterPackRat("b8a0492a-c5e6-4955-819b-f47797005105")]
	public class NullableTestModel
	{
		[PackField(1, SchemaId.TextUtf8, IsNullable=true)]
		public string? ExplicitlyNullableField { get; set; }

		[PackField(2, SchemaId.TextUtf8, IsNullable = false)]
		public string? ExplicitlyNonNullableField { get; set; }

		[PackField(3, SchemaId.TextUtf8)]
		public string? ImplicitlyNullableField { get; set; }

		[PackField(4, SchemaId.TextUtf8)]
		public string ImplicitlyNonNullableField { get; set; }
	}
}
