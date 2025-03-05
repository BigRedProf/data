using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Tests
{
	public class WholeNumberPackRatTests
	{
		private readonly IPiedPiper _piedPiper;

		public WholeNumberPackRatTests()
		{
			_piedPiper = PackRatTestHelper.GetPiedPiper();
		}

		[Theory]
		[InlineData(1, 0, "0")]
		[InlineData(1, 1, "1")]
		[InlineData(2, 0, "00")]
		[InlineData(2, 1, "10")]
		[InlineData(2, 2, "01")]
		[InlineData(2, 3, "11")]
		[InlineData(3, 0, "000")]
		[InlineData(3, 7, "111")]
		[InlineData(4, 0, "0000")]
		[InlineData(4, 15, "1111")]
		[InlineData(5, 0, "00000")]
		[InlineData(5, 31, "11111")]
		[InlineData(6, 0, "000000")]
		[InlineData(6, 43, "110101")]
		[InlineData(6, 63, "111111")]
		[InlineData(7, 0, "0000000")]
		[InlineData(7, 127, "1111111")]
		[InlineData(8, 0, "00000000")]
		[InlineData(8, 43, "11010100")]
		[InlineData(8, 255, "11111111")]
		[InlineData(9, 0, "000000000")]
		[InlineData(9, 511, "111111111")]
		[InlineData(10, 0, "0000000000")]
		[InlineData(10, 1023, "1111111111")]
		[InlineData(11, 0, "00000000000")]
		[InlineData(11, 2047, "11111111111")]
		[InlineData(12, 0, "000000000000")]
		[InlineData(12, 4095, "111111111111")]
		[InlineData(13, 0, "0000000000000")]
		[InlineData(13, 43, "11010100 00000")]
		[InlineData(13, 8191, "1111111111111")]
		[InlineData(14, 0, "00000000000000")]
		[InlineData(14, 16383, "11111111111111")]
		[InlineData(15, 0, "000000000000000")]
		[InlineData(15, 32767, "111111111111111")]
		[InlineData(16, 0, "0000000000000000")]
		[InlineData(16, 43, "11010100 00000000")]
		[InlineData(16, 65535, "1111111111111111")]
		[InlineData(17, 0, "00000000000000000")]
		[InlineData(17, 131071, "11111111111111111")]
		[InlineData(18, 0, "000000000000000000")]
		[InlineData(18, 262143, "111111111111111111")]
		[InlineData(19, 0, "0000000000000000000")]
		[InlineData(19, 524287, "1111111111111111111")]
		[InlineData(20, 0, "00000000000000000000")]
		[InlineData(20, 1048575, "11111111111111111111")]
		[InlineData(21, 0, "000000000000000000000")]
		[InlineData(21, 2097151, "111111111111111111111")]
		[InlineData(22, 0, "0000000000000000000000")]
		[InlineData(22, 4194303, "1111111111111111111111")]
		[InlineData(23, 0, "00000000000000000000000")]
		[InlineData(23, 8388607, "11111111111111111111111")]
		[InlineData(24, 0, "000000000000000000000000")]
		[InlineData(24, 16777215, "111111111111111111111111")]
		[InlineData(25, 0, "0000000000000000000000000")]
		[InlineData(25, 33554431, "1111111111111111111111111")]
		[InlineData(26, 0, "00000000000000000000000000")]
		[InlineData(26, 67108863, "11111111111111111111111111")]
		[InlineData(27, 0, "000000000000000000000000000")]
		[InlineData(27, 134217727, "111111111111111111111111111")]
		[InlineData(28, 0, "0000000000000000000000000000")]
		[InlineData(28, 268435455, "1111111111111111111111111111")]
		[InlineData(29, 0, "00000000000000000000000000000")]
		[InlineData(29, 536870911, "11111111111111111111111111111")]
		[InlineData(30, 0, "000000000000000000000000000000")]
		[InlineData(30, 43, "11010100 00000000 00000000 000000")]
		[InlineData(30, 1073741823, "111111111111111111111111111111")]
		[InlineData(31, 0, "0000000000000000000000000000000")]
		[InlineData(31, 43, "11010100 00000000 00000000 0000000")]
		[InlineData(31, 2147483647, "1111111111111111111111111111111")]
		public void WholeNumberPackRat_PackUnpack_Success(int bitLength, int value, string expectedCodeString)
		{
			// Arrange
			var packRat = new WholeNumberPackRat(_piedPiper, bitLength);
			var expectedCode = new Code(expectedCodeString);

			// Act & Assert
			PackRatTestHelper.TestPackModel(packRat, value, expectedCode);
			PackRatTestHelper.TestUnpackModel(packRat, expectedCode, value);
		}

		[Theory]
		[InlineData(1, 2)]
		[InlineData(2, 4)]
		[InlineData(3, 8)]
		[InlineData(4, 16)]
		[InlineData(5, 32)]
		[InlineData(6, 64)]
		[InlineData(7, 128)]
		[InlineData(8, 256)]
		[InlineData(9, 512)]
		[InlineData(10, 1024)]
		[InlineData(11, 2048)]
		[InlineData(12, 4096)]
		[InlineData(13, 8192)]
		[InlineData(14, 16384)]
		[InlineData(15, 32768)]
		[InlineData(16, 65536)]
		[InlineData(17, 131072)]
		[InlineData(18, 262144)]
		[InlineData(19, 524288)]
		[InlineData(20, 1048576)]
		[InlineData(21, 2097152)]
		[InlineData(22, 4194304)]
		[InlineData(23, 8388608)]
		[InlineData(24, 16777216)]
		[InlineData(25, 33554432)]
		[InlineData(26, 67108864)]
		[InlineData(27, 134217728)]
		[InlineData(28, 268435456)]
		[InlineData(29, 536870912)]
		[InlineData(30, 1073741824)]
		[InlineData(31, -1)]
		public void WholeNumberPackRat_PackModel_ThrowsArgumentOutOfRangeException(int bitLength, int value)
		{
			// Arrange
			var packRat = new WholeNumberPackRat(_piedPiper, bitLength);

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() => PackRatTestHelper.TestPackModel(packRat, value, null));
		}
	}
}
