using BigRedProf.Data;

namespace prt.A.B.C
{
	[RegisterPackRat("d3499bac-fb5d-4407-8d28-e32ed37898c6")]
	public class Point
	{
		[PackField(2, SchemaId.Int32)]
		public int Y;

		[PackField(1, SchemaId.Int32)]
		public int X;
	}
}
