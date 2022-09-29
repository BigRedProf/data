using BigRedProf.Data;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[RegisterPackRat("b8a0492a-c5e6-4955-819b-f47797005105")]
	public class NullableTestModel
	{
		[PackField(1, SchemaId.StringUtf8)]
		public string? NullableField;

		[PackField(2, SchemaId.StringUtf8)]
		public string NonNullableField;
	}
}
