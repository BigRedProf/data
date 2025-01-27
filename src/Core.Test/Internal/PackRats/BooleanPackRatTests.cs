using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class BooleanPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			BooleanPackRat packRat = new BooleanPackRat(piedPiper);
			bool model = false;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForTrue()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<bool> packRat = new BooleanPackRat(piedPiper);
			PackRatTestHelper.TestPackModel<bool>(packRat, true, "1");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForFalse()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<bool> packRat = new BooleanPackRat(piedPiper);
			PackRatTestHelper.TestPackModel<bool>(packRat, false, "0");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			BooleanPackRat packRat = new BooleanPackRat(piedPiper);

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
			PiedPiper piedPiper = new PiedPiper();
			PackRat<bool> packRat = new BooleanPackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<bool>(packRat, "1", true);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForFalse()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<bool> packRat = new BooleanPackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<bool>(packRat, "0", false);
		}
		#endregion
	}
}
