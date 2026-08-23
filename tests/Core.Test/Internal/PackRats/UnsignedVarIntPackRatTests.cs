using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class UnsignedVarIntPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new UnsignedVarIntPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() => packRat.PackModel(null, 42));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new UnsignedVarIntPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() => packRat.UnpackModel(null));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowOnNegativeValue()
		{
			PiedPiper piedPiper = new PiedPiper();
			PackRat<int> packRat = new UnsignedVarIntPackRat(piedPiper);

			Assert.Throws<ArgumentOutOfRangeException>(() => packRat.PackModel(new CodeWriter(new MemoryStream()), -1));
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForValidValues()
		{
			PiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			PackRat<int> packRat = new UnsignedVarIntPackRat(piedPiper);

			PackRatTestHelper.TestPackModel<int>(packRat, 0, PackRatTestHelper.ReverseBytes("00000000"));
			PackRatTestHelper.TestPackModel<int>(packRat, 1, PackRatTestHelper.ReverseBytes("00000001"));
			PackRatTestHelper.TestPackModel<int>(packRat, 127, PackRatTestHelper.ReverseBytes("01111111"));
			PackRatTestHelper.TestPackModel<int>(packRat, 128, PackRatTestHelper.ReverseBytes("10000000 00000001"));
			PackRatTestHelper.TestPackModel<int>(packRat, 255, PackRatTestHelper.ReverseBytes("11111111 00000001"));
			PackRatTestHelper.TestPackModel<int>(packRat, 300, PackRatTestHelper.ReverseBytes("10101100 00000010"));
			PackRatTestHelper.TestPackModel<int>(packRat, 16384, PackRatTestHelper.ReverseBytes("10000000 10000000 00000001"));
			PackRatTestHelper.TestPackModel<int>(packRat, 2097151, PackRatTestHelper.ReverseBytes("11111111 11111111 01111111"));
			PackRatTestHelper.TestPackModel<int>(packRat, 2147483647, PackRatTestHelper.ReverseBytes("11111111 11111111 11111111 11111111 00000111"));
		}


		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForValidEncodings()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			PackRat<int> packRat = new UnsignedVarIntPackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("00000000"), 0);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("00000001"), 1);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("01111111"), 127);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("10000000 00000001"), 128);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("11111111 00000001"), 255);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("10101100 00000010"), 300);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("10000000 10000000 00000001"), 16384);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("11111111 11111111 01111111"), 2097151);
			PackRatTestHelper.TestUnpackModel<int>(packRat, PackRatTestHelper.ReverseBytes("11111111 11111111 11111111 11111111 00000111"), 2147483647);
		}
		#endregion

		#region private functions
		private static IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			return piedPiper;
		}
		#endregion
	}
}
