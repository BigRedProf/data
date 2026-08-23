using BigRedProf.Data.Core;
using System.Collections;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	/// <summary>
	/// Covers list members declared as properties AND as fields in the same model.
	/// <see cref="ListTestModel"/> is entirely properties, so before this model existed
	/// the generator had no coverage for a list declared as a plain field.
	/// </summary>
	[GeneratePackRat("d4e8f1a2-3b5c-4d6e-8f90-1a2b3c4d5e6f")]
	public class MixedListTestModel
	{
		[PackListField(2, CoreSchema.Int32, ByteAligned.No)]
		public IList<int> ListField;

		[PackListField(4, CoreSchema.Int32, ByteAligned.Yes)]
		public IList<int>? NullableListField;

		[PackListField(1, CoreSchema.TextUtf8, ByteAligned.Yes)]
		public IList<string> ListProperty { get; set; }

		[PackListField(3, CoreSchema.TextUtf8, ByteAligned.No)]
		public IList<string?>? NullableListPropertyOfNullableElements { get; set; }
	}
}
