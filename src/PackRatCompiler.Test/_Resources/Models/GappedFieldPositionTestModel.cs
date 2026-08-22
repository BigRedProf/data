using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	// Position 2 is missing. This must not compile: a gap in the declaration is not a gap on the
	// wire, so Weight would simply become the second part and every code already written under
	// this schema would be misread.
	[GeneratePackRat("2f5c8f1a-6b2d-4a77-9d0e-84c3a51b7e60")]
	public class GappedFieldPositionTestModel
	{
		[PackField(1, CoreSchema.Int32)]
		public int Height { get; set; }

		[PackField(3, CoreSchema.Int32)]
		public int Weight { get; set; }
	}
}
