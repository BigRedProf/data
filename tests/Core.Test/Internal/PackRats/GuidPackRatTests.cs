using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class GuidPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			GuidPackRat packRat = new GuidPackRat(piedPiper);
			Guid model = Guid.Empty;

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
			PackRat<Guid> packRat = new GuidPackRat(piedPiper);
			PackRatTestHelper.TestPackModel<Guid>(packRat, Guid.Empty, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000");
			PackRatTestHelper.TestPackModel<Guid>(packRat, new Guid("58DA6851-5B00-43DA-A5C5-2B1598EEED56"), "10001010 00010110 01011011 00011010 00000000 11011010 01011011 11000010 10100101 10100011 11010100 10101000 00011001 01110111 10110111 01101010");
			PackRatTestHelper.TestPackModel<Guid>(packRat, new Guid("{F28A607C-E9F2-4407-BD7B-0FFD5F7F5383}"), "00111110 00000110 01010001 01001111 01001111 10010111 11100000 00100010 10111101 11011110 11110000 10111111 11111010 11111110 11001010 11000001");
			PackRatTestHelper.TestPackModel<Guid>(packRat, new Guid("82c8114c-9ba3-47b5-8693-d5e60efc5f99"), "00110010 10001000 00010011 01000001 11000101 11011001 10101101 11100010 01100001 11001001 10101011 01100111 01110000 00111111 11111010 10011001");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			GuidPackRat packRat = new GuidPackRat(piedPiper);

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
			PackRat<Guid> packRat = new GuidPackRat(piedPiper);
			PackRatTestHelper.TestUnpackModel<Guid>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000", Guid.Empty);
			PackRatTestHelper.TestUnpackModel<Guid>(packRat, "10001010 00010110 01011011 00011010 00000000 11011010 01011011 11000010 10100101 10100011 11010100 10101000 00011001 01110111 10110111 01101010", new Guid("58DA6851-5B00-43DA-A5C5-2B1598EEED56"));
			PackRatTestHelper.TestUnpackModel<Guid>(packRat, "00111110 00000110 01010001 01001111 01001111 10010111 11100000 00100010 10111101 11011110 11110000 10111111 11111010 11111110 11001010 11000001", new Guid("{F28A607C-E9F2-4407-BD7B-0FFD5F7F5383}"));
			PackRatTestHelper.TestUnpackModel<Guid>(packRat, "00110010 10001000 00010011 01000001 11000101 11011001 10101101 11100010 01100001 11001001 10101011 01100111 01110000 00111111 11111010 10011001", new Guid("82c8114c-9ba3-47b5-8693-d5e60efc5f99"));
		}
		#endregion
	}
}
