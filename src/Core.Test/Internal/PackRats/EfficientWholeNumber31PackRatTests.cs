using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class EfficientWholeNumber31PackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			EfficientWholeNumber31PackRat packRat = new EfficientWholeNumber31PackRat(piedPiper);
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
		public void PackModel_ShouldWorkForTinyNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestPackModel<int>(packRat, 0, "1000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1, "1010");
			PackRatTestHelper.TestPackModel<int>(packRat, 2, "1001");
			PackRatTestHelper.TestPackModel<int>(packRat, 3, "1011");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForSmallNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestPackModel<int>(packRat, 4, "11000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 5, "11010000");
			PackRatTestHelper.TestPackModel<int>(packRat, 7, "11011000");
			PackRatTestHelper.TestPackModel<int>(packRat, 10, "11001100");
			PackRatTestHelper.TestPackModel<int>(packRat, 21, "11010001");
			PackRatTestHelper.TestPackModel<int>(packRat, 28, "11000011");
			PackRatTestHelper.TestPackModel<int>(packRat, 34, "11001111");
			PackRatTestHelper.TestPackModel<int>(packRat, 35, "11011111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForMediumNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestPackModel<int>(packRat, 36, "11100000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 37, "11101000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1970, "11100111 00011110");
			PackRatTestHelper.TestPackModel<int>(packRat, 1976, "11100010 10011110");
			PackRatTestHelper.TestPackModel<int>(packRat, 2143, "11101101 11000001");
			PackRatTestHelper.TestPackModel<int>(packRat, 4000, "11100011 11101111");
			PackRatTestHelper.TestPackModel<int>(packRat, 4096, "11100011 10111111");
			PackRatTestHelper.TestPackModel<int>(packRat, 4131, "11101111 11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForLargeNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestPackModel<int>(packRat, 4132, "11110000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 4133, "11111000 00000000 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 5000, "11110010 01101100 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 43000, "11110010 10111110 10010000");
			PackRatTestHelper.TestPackModel<int>(packRat, 690000, "11110011 01001110 11100101");
			PackRatTestHelper.TestPackModel<int>(packRat, 1000000, "11110011 10000100 11001111");
			PackRatTestHelper.TestPackModel<int>(packRat, 1052706, "11110111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, 1052707, "11111111 11111111 11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForHugeNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestPackModel<int>(packRat, 1052708, "00010010 00000100 00000100 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 1052709, "01010010 00000100 00000100 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 2000000, "00000000 10010000 10111100 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 5000000, "00000001 01101001 00011001 00000000");
			PackRatTestHelper.TestPackModel<int>(packRat, 100000000, "00000000 01000011 11010111 11010000");
			PackRatTestHelper.TestPackModel<int>(packRat, 2000000000, "00000000 00010100 11010110 01110111");
			PackRatTestHelper.TestPackModel<int>(packRat, 2147483646, "00111111 11111111 11111111 11111111");
			PackRatTestHelper.TestPackModel<int>(packRat, 2147483647, "01111111 11111111 11111111 11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			EfficientWholeNumber31PackRat packRat = new EfficientWholeNumber31PackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenCodeIsInvalid()
		{
			PiedPiper piedPiper = new PiedPiper();
			EfficientWholeNumber31PackRat packRat = new EfficientWholeNumber31PackRat(piedPiper);

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					// this is the equivalent of 1052707 (MinValueFor32BitPacking - 1) packed 
					// into 32 bits
					Code code = "01100010 00000100 00000100 00000000";
					CodeReader reader = PackRatTestHelper.CreateCodeReader(code);
					packRat.UnpackModel(reader);
				}
			);

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					// this is the equivalent of zero packed into 32 bits
					Code code = "00000000 00000000 00000000 00000000";
					CodeReader reader = PackRatTestHelper.CreateCodeReader(code);
					packRat.UnpackModel(reader);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForTinyNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "1000", 0);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "1010", 1);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "1001", 2);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "1011", 3);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForSmallNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11000000", 4);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11010000", 5);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11011000", 7);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11001100", 10);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11010001", 21);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11000011", 28);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11001111", 34);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11011111", 35);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForMediumNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11100000 00000000", 36);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11101000 00000000", 37);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11100111 00011110", 1970);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11100010 10011110", 1976);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11101101 11000001", 2143);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11100011 11101111", 4000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11100011 10111111", 4096);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11101111 11111111", 4131);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForLargeNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110000 00000000 00000000", 4132);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11111000 00000000 00000000", 4133);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110010 01101100 00000000", 5000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110010 10111110 10010000", 43000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110011 01001110 11100101", 690000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110011 10000100 11001111", 1000000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11110111 11111111 11111111", 1052706);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "11111111 11111111 11111111", 1052707);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForHugeNumbers()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new EfficientWholeNumber31PackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00010010 00000100 00000100 00000000", 1052708);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "01010010 00000100 00000100 00000000", 1052709);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00000000 10010000 10111100 00000000", 2000000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00000001 01101001 00011001 00000000", 5000000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00000000 01000011 11010111 11010000", 100000000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00000000 00010100 11010110 01110111", 2000000000);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "00111111 11111111 11111111 11111111", 2147483646);
			PackRatTestHelper.TestUnpackModel<int>(packRat, "01111111 11111111 11111111 11111111", 2147483647);
		}
		#endregion
	}
}
