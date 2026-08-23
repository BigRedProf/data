using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	public enum ColorHexagon
	{
		Red = 12,
		Yellow = 2,
		Green = 4,
		Cyan = 6,
		Blue = 8,
		Magenta = 10
	}

	[GeneratePackRat("2993c2cf-3d21-4d94-94af-091cfc5594ad")]
	public class ModelWithEnum
	{
		[PackField(1, CoreSchema.Int32)]
		public ColorHexagon Color1 { get; set; }

		[PackField(2, CoreSchema.EfficientWholeNumber31)]
		public ColorHexagon Color2 { get; set; }
	}
}
