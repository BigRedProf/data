using BigRedProf.Data;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[GeneratePackRat("d3499bac-fb5d-4407-8d28-e32ed37898c6")]
	public class Point
	{
		[PackField(2, "43")]
		public int Y{ get; set; }

		[PackField(1, CoreSchema.Int32)]
		public int X { get; set; }
	}
}
