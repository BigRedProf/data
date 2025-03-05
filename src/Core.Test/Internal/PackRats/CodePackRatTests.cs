using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class CodePackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();

			CodePackRat packRat = new CodePackRat(piedPiper);
			Code model = "1";

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForOneByteCodes()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<Code> packRat = new CodePackRat(piedPiper);
			
			PackRatTestHelper.TestPackModel<Code>(packRat, "0", "1010 0");
			PackRatTestHelper.TestPackModel<Code>(packRat, "1", "1010 1");
			PackRatTestHelper.TestPackModel<Code>(packRat, "101", "1011 101");
			PackRatTestHelper.TestPackModel<Code>(packRat, "111", "1011 111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkMultiByteCodes()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<Code> packRat = new CodePackRat(piedPiper);

			PackRatTestHelper.TestPackModel<Code>(packRat, "0000", "11000000 0000");
			PackRatTestHelper.TestPackModel<Code>(packRat, "0101", "11000000 0101");
			PackRatTestHelper.TestPackModel<Code>(packRat, "11110001", "11000100 11110001");
			PackRatTestHelper.TestPackModel<Code>(packRat, "10100101", "11000100 10100101");
			PackRatTestHelper.TestPackModel<Code>(
				packRat, 
				"01010010 11010010 00101110 0101", 
				"11000011 01010010 11010010 00101110 0101"
			);
			PackRatTestHelper.TestPackModel<Code>(
				packRat, 
				"01110000 11000010 10111101 0110", 
				"11000011 01110000 11000010 10111101 0110"
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodePackRat packRat = new CodePackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForOneByteCodes()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<Code> packRat = new CodePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<Code>(packRat, "1010 0", "0");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "1010 1", "1");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "1011 101", "101");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "1011 111", "111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForMultiByteCodes()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			PackRat<Code> packRat = new CodePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<Code>(packRat, "11000000 0000", "0000");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "11000000 0101", "0101");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "11000100 11110001", "11110001");
			PackRatTestHelper.TestUnpackModel<Code>(packRat, "11000100 10100101", "10100101");
			PackRatTestHelper.TestUnpackModel<Code>(
				packRat,
				"11000011 01010010 11010010 00101110 0101",
				"01010010 11010010 00101110 0101"
			);
			PackRatTestHelper.TestUnpackModel<Code>(
				packRat,
				"11000011 01110000 11000010 10111101 0110",
				"01110000 11000010 10111101 0110"
			);
		}
		#endregion
	}
}
