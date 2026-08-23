using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class VarIntPackRatTests
	{
		#region PackRat methods

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			VarIntPackRat packRat = new VarIntPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.PackModel(null, 123);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			VarIntPackRat packRat = new VarIntPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.UnpackModel(null);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowOnOverflow()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			VarIntPackRat packRat = new VarIntPackRat(piedPiper);

			// 6 continuation bytes → overflow for 32-bit int
			Code code = "11111111 11111111 11111111 11111111 11111111 11111111";
			var reader = PackRatTestHelper.CreateCodeReader(code);

			Assert.Throws<InvalidOperationException>(() =>
			{
				packRat.UnpackModel(reader);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForBoundaryCases()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			VarIntPackRat packRat = new VarIntPackRat(piedPiper);

			PackRatTestHelper.TestPackModel(packRat, 0, "00000000");
			PackRatTestHelper.TestPackModel(packRat, 1, "00000001");
			PackRatTestHelper.TestPackModel(packRat, 127, "01111111"); // last 1-byte value
			PackRatTestHelper.TestPackModel(packRat, 128, "10000000 00000001"); // first 2-byte value
			PackRatTestHelper.TestPackModel(packRat, 300, "10101100 00000010"); // arbitrary value
			PackRatTestHelper.TestPackModel(packRat, int.MaxValue, "11111111 11111111 11111111 11111111 00000111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForBoundaryCases()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			VarIntPackRat packRat = new VarIntPackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel(packRat, "00000000", 0);
			PackRatTestHelper.TestUnpackModel(packRat, "00000001", 1);
			PackRatTestHelper.TestUnpackModel(packRat, "01111111", 127);
			PackRatTestHelper.TestUnpackModel(packRat, "10000000 00000001", 128);
			PackRatTestHelper.TestUnpackModel(packRat, "10101100 00000010", 300);
			PackRatTestHelper.TestUnpackModel(packRat, "11111111 11111111 11111111 11111111 00000111", int.MaxValue);
		}

		#endregion
	}
}
