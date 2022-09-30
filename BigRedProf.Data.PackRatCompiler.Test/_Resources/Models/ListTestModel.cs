using BigRedProf.Data;
using System.Collections;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[RegisterPackRat("c3b5a06c-da78-4f48-863f-455dfbd339f6")]
	public class ListTestModel
	{
		[PackField(1, SchemaId.StringUtf8)]
		public IList<string> List;

		[PackField(2, SchemaId.StringUtf8)]
		public IList<string>? NullableList;

		[PackField(3, SchemaId.StringUtf8)]
		public IList<string?> ListOfNullableElements;

		[PackField(4, SchemaId.StringUtf8)]
		public IList<string?>? NullableListOfNullableElements;
	}
}
