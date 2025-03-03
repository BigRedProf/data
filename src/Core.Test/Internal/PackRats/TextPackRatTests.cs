using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class TextPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TextPackRat packRat = new TextPackRat(piedPiper);
			string model = string.Empty;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWork()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<string> packRat = new TextPackRat(piedPiper);
			PackRatTestHelper.TestPackModel<string>(packRat, string.Empty, "1000");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TextPackRat packRat = new TextPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForTrue()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<string> packRat = new TextPackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<string>(packRat, "1000", string.Empty);
		}
		#endregion
	}
}
