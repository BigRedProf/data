using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class BytePackRatTests
	{
		#region PackRat methods

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			BytePackRat packRat = new BytePackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.PackModel(null, 43);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWork()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<byte> packRat = new BytePackRat(piedPiper);

			PackRatTestHelper.TestPackModel<byte>(packRat, 0, "00000000");
			PackRatTestHelper.TestPackModel<byte>(packRat, 1, "10000000");
			PackRatTestHelper.TestPackModel<byte>(packRat, 2, "01000000");
			PackRatTestHelper.TestPackModel<byte>(packRat, 3, "11000000");
			PackRatTestHelper.TestPackModel<byte>(packRat, 4, "00100000");
			PackRatTestHelper.TestPackModel<byte>(packRat, 43, "11010100");
			PackRatTestHelper.TestPackModel<byte>(packRat, 119, "11101110");
			PackRatTestHelper.TestPackModel<byte>(packRat, 127, "11111110");
			PackRatTestHelper.TestPackModel<byte>(packRat, 128, "00000001");
			PackRatTestHelper.TestPackModel<byte>(packRat, 129, "10000001");
			PackRatTestHelper.TestPackModel<byte>(packRat, 254, "01111111");
			PackRatTestHelper.TestPackModel<byte>(packRat, 255, "11111111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			BytePackRat packRat = new BytePackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.UnpackModel(null);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWork()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<byte> packRat = new BytePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<byte>(packRat, "00000000", 0);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "10000000", 1);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "01000000", 2);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "11000000", 3);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "00100000", 4);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "11010100", 43);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "11101110", 119);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "11111110", 127);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "00000001", 128);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "10000001", 129);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "01111111", 254);
			PackRatTestHelper.TestUnpackModel<byte>(packRat, "11111111", 255);
		}

		#endregion
	}
}
