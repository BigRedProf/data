using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class DateTimePackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			DateTimePackRat packRat = new DateTimePackRat(piedPiper);
			DateTime model = DateTime.MinValue;

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
			PackRat<DateTime> packRat = new DateTimePackRat(piedPiper);

			PackRatTestHelper.TestPackModel<DateTime>(packRat, DateTime.MinValue, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00");
			PackRatTestHelper.TestPackModel<DateTime>(packRat, DateTime.MaxValue, "11111111 11111100 11101100 00101111 10101110 00010100 01010011 11010100 00");
			PackRatTestHelper.TestPackModel<DateTime>(packRat, new DateTime(0, DateTimeKind.Utc), "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 01");
			PackRatTestHelper.TestPackModel<DateTime>(packRat, new DateTime(0, DateTimeKind.Local), "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 10");
			PackRatTestHelper.TestPackModel<DateTime>(packRat, DateTime.Parse("November 25, 1971"), "00000000 00000010 01011111 11100011 10100001 00000101 10000101 00010000 00");
			PackRatTestHelper.TestPackModel<DateTime>(packRat, DateTime.Parse("August 30, 2023"), "00000000 00000011 11100011 10110000 00110111 00010101 11011011 00010000 00");
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
			PackRat<DateTime> packRat = new DateTimePackRat(piedPiper);

			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00", DateTime.MinValue);
			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "11111111 11111100 11101100 00101111 10101110 00010100 01010011 11010100 00", DateTime.MaxValue);
			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 01", new DateTime(0, DateTimeKind.Utc));
			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 10", new DateTime(0, DateTimeKind.Local));
			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "00000000 00000010 01011111 11100011 10100001 00000101 10000101 00010000 00", DateTime.Parse("November 25, 1971"));
			PackRatTestHelper.TestUnpackModel<DateTime>(packRat, "00000000 00000011 11100011 10110000 00110111 00010101 11011011 00010000 00", DateTime.Parse("August 30, 2023"));
		}
		#endregion
	}
}
