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
			BooleanPackRat packRat = new BooleanPackRat();
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
			PackRat<bool> packRat = new BooleanPackRat();
			PackRatTestHelper.TestPackModel<bool>(packRat, true, "1");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForFalse()
		{
			PackRat<bool> packRat = new BooleanPackRat();
			PackRatTestHelper.TestPackModel<bool>(packRat, false, "0");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			BooleanPackRat packRat = new BooleanPackRat();

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
			PackRat<bool> packRat = new BooleanPackRat();
			PackRatTestHelper.TestUnpackModel<bool>(packRat, "1", true);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForFalse()
		{
			PackRat<bool> packRat = new BooleanPackRat();
			PackRatTestHelper.TestUnpackModel<bool>(packRat, "0", false);
		}
		#endregion
	}
}
