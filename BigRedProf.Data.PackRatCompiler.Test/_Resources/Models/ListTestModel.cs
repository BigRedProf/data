using BigRedProf.Data;
using System.Collections;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[RegisterPackRat("c3b5a06c-da78-4f48-863f-455dfbd339f6")]
	public class ListTestModel
	{
		[PackListField(1, SchemaId.StringUtf8, ByteAligned.Yes)]
		public IList<string> List;

		[PackListField(2, SchemaId.StringUtf8, ByteAligned.No)]
		public IList<string>? NullableList;

		[PackListField(3, SchemaId.StringUtf8, ByteAligned.No)]
		public IList<string?> ListOfNullableElements;

		[PackListField(4, SchemaId.StringUtf8, ByteAligned.Yes)]
		public IList<string?>? NullableListOfNullableElements;
	}
}
