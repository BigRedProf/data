using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class Int32PackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			Int32PackRat packRat = new Int32PackRat();
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
			PackRat<int> packRat = new Int32PackRat();
			PackRatTestHelper.TestPackModel<int>(packRat, 0, "00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1, "10000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 2, "01000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 3, "11000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 4, "00100000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 43, "11010100 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1024, "00000000 00100000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1234567, "11100001 01101011 01001000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1234567890, "01001011 01000000 01101001 10010010");
			PackRatTestHelper.TestPackModel<int>(packRat, 2147483647, "11111111 11111111 11111111 11111110");
			PackRatTestHelper.TestPackModel<int>(packRat, -1, "11111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -2, "01111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -3, "10111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -4, "00111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -43, "10101011 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -1024, "00000000 00111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -1234567, "10011110 10010100 10110111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, -1234567890, "01110100 10111111 10010110 01101101");
			PackRatTestHelper.TestPackModel<int>(packRat, -2147483647, "10000000 00000000 00000000 00000001");
			PackRatTestHelper.TestPackModel<int>(packRat, -2147483648, "00000000 00000000 00000000 00000001");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			Int32PackRat packRat = new Int32PackRat();

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
			PackRat<int> packRat = new Int32PackRat();
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00000000 00000000 00000000 00000000", 0);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "10000000 00000000 00000000 00000000", 1);
		}
		#endregion
	}
}
