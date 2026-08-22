using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	// Position 2 was retired. It stays retired: renumbering Weight down to 2 would silently
	// change what every code already written under this schema means.
	[GeneratePackRat("2f5c8f1a-6b2d-4a77-9d0e-84c3a51b7e60")]
	public class RetiredFieldTestModel
	{
		[PackField(1, CoreSchema.Int32)]
		public int Height { get; set; }

		[PackField(3, CoreSchema.Int32)]
		public int Weight { get; set; }
	}
}
