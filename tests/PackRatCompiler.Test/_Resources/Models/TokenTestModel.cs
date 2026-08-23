using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[GeneratePackRat("54f0c71b-af41-4a25-ac7f-4e0e9741a818")]
	public class TokenTestModel
	{
		[PackField(1, CoreSchema.TextUtf16)]
		public string StringAsValue { get; set; }

		[PackField(2, "e9ab0764-2e47-42dc-860d-0ba1573b728f")]
		public string StringAsToken { get; set; }
	}
}
