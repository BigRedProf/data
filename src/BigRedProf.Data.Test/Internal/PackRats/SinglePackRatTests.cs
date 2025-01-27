using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class SinglePackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			SinglePackRat packRat = new SinglePackRat(piedPiper);
			float model = 43;

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
			PiedPiper piedPiper = new PiedPiper();
			PackRat<float> packRat = new SinglePackRat(piedPiper);
			PackRatTestHelper.TestPackModel<float>(packRat, 0.0f, "00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<float>(packRat, -2.0f, "00000000 00000000 00000000 00000011");
			PackRatTestHelper.TestPackModel<float>(packRat, 43.43f, "01001010 00011101 10110100 01000010");
			PackRatTestHelper.TestPackModel<float>(packRat, float.NegativeInfinity, "00000000 00000000 00000001 11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			SinglePackRat packRat = new SinglePackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWork()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<float> packRat = new SinglePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<float>(packRat, "00000000 00000000 00000000 00000000", 0.0f);
			PackRatTestHelper.TestUnpackModel<float>(packRat, "00000000 00000000 00000000 00000011", -2.0f);
			PackRatTestHelper.TestUnpackModel<float>(packRat, "01001010 00011101 10110100 01000010", 43.43f);
			PackRatTestHelper.TestUnpackModel<float>(packRat, "00000000 00000000 00000001 11111111", float.NegativeInfinity);
		}
		#endregion
	}
}
