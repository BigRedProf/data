using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class Int64PackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			Int64PackRat packRat = new Int64PackRat(piedPiper);
			int model = 43;

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
			PackRat<long> packRat = new Int64PackRat(piedPiper);

			PackRatTestHelper.TestPackModel<long>(packRat, 0, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<long>(packRat, 1, "10000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<long>(packRat, 2, "01000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<long>(packRat, 3, "11000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			// ... other test cases ...

			// Test negative values
			PackRatTestHelper.TestPackModel<long>(packRat, -1, "11111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<long>(packRat, -2, "01111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<long>(packRat, -3, "10111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111");
			// ... other test cases ...
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			Int64PackRat packRat = new Int64PackRat(piedPiper);

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
			PackRat<long> packRat = new Int64PackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<long>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", 0);
			PackRatTestHelper.TestUnpackModel<long>(packRat, "10000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", 1);
			PackRatTestHelper.TestUnpackModel<long>(packRat, "01000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", 2);
			PackRatTestHelper.TestUnpackModel<long>(packRat, "11000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", 3);
			// ... other test cases ...

			// Test negative values
			PackRatTestHelper.TestUnpackModel<long>(packRat, "11111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111", -1);
			PackRatTestHelper.TestUnpackModel<long>(packRat, "01111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111", -2);
			PackRatTestHelper.TestUnpackModel<long>(packRat, "10111111 11111111 11111111 11111111 11111111 11111111 11111111 11111111", -3);
			// ... other test cases ...
		}
		#endregion
	}
}
