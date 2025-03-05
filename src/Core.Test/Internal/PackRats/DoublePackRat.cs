using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class DoublePackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			DoublePackRat packRat = new DoublePackRat(piedPiper);
			double model = 43;

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
			PackRat<double> packRat = new DoublePackRat(piedPiper);
			PackRatTestHelper.TestPackModel<double>(packRat, 0.0d, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<double>(packRat, -2.0d, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000011");
			PackRatTestHelper.TestPackModel<double>(packRat, 43.43d, "11101011 11000101 00001110 10111100 01010000 11101101 10100010 00000010");
			PackRatTestHelper.TestPackModel<double>(packRat, float.NegativeInfinity, "00000000 00000000 00000000 00000000 00000000 00000000 00001111 11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			DoublePackRat packRat = new DoublePackRat(piedPiper);

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
			PackRat<double> packRat = new DoublePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<double>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", 0.0d);
			PackRatTestHelper.TestUnpackModel<double>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000011", -2.0d);
			PackRatTestHelper.TestUnpackModel<double>(packRat, "11101011 11000101 00001110 10111100 01010000 11101101 10100010 00000010", 43.43d);
			PackRatTestHelper.TestUnpackModel<double>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00001111 11111111", float.NegativeInfinity);
		}
		#endregion
	}
}
