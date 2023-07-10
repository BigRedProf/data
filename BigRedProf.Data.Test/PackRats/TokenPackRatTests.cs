using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test.PackRats
{
	public class TokenPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();

			TokenPackRat<string> packRat = new TokenPackRat<string>(piedPiper);
			string model = "Huskers";

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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TokenPackRat<string> packRat = new TokenPackRat<string>(piedPiper);

			packRat.DefineToken("0000", "Illinois Fighting Illini");
			packRat.DefineToken("0001", "Indiana Hoosiers");
			packRat.DefineToken("0010", "Iowa Hawkeyes");
			packRat.DefineToken("0011", "Maryland Terrapins");
			packRat.DefineToken("0100", "Michigan Wolverines");
			packRat.DefineToken("0101", "Michigan State Spartans");
			packRat.DefineToken("0110", "Minnesota Golden Gophers");
			packRat.DefineToken("0111", "Nebraska Cornhuskers");
			packRat.DefineToken("1000", "Northwestern Wildcats");
			packRat.DefineToken("1001", "Ohio State Buckeyes");
			packRat.DefineToken("1010", "Penn State Nittany Lions");
			packRat.DefineToken("1011", "Purdue Boilermakers");
			packRat.DefineToken("1100", "Rutgers Scarlet Knights");
			packRat.DefineToken("1101", "UCLA Bruins");
			packRat.DefineToken("1110", "USC Trojans");
			packRat.DefineToken("1111", "Wisconsin Badgers");

			PackRatTestHelper.TestPackModel<string>(packRat, "Illinois Fighting Illini", "11 000000 0000");
			PackRatTestHelper.TestPackModel<string>(packRat, "Indiana Hoosiers", "11 000000 0001");
			PackRatTestHelper.TestPackModel<string>(packRat, "Iowa Hawkeyes", "11 000000 0010");
			PackRatTestHelper.TestPackModel<string>(packRat, "Maryland Terrapins", "11 000000 0011");
			PackRatTestHelper.TestPackModel<string>(packRat, "Michigan Wolverines", "11 000000 0100");
			PackRatTestHelper.TestPackModel<string>(packRat, "Michigan State Spartans", "11 000000 0101");
			PackRatTestHelper.TestPackModel<string>(packRat, "Minnesota Golden Gophers", "11 000000 0110");
			PackRatTestHelper.TestPackModel<string>(packRat, "Nebraska Cornhuskers", "11 000000 0111");
			PackRatTestHelper.TestPackModel<string>(packRat, "Northwestern Wildcats", "11 000000 1000");
			PackRatTestHelper.TestPackModel<string>(packRat, "Ohio State Buckeyes", "11 000000 1001");
			PackRatTestHelper.TestPackModel<string>(packRat, "Penn State Nittany Lions", "11 000000 1010");
			PackRatTestHelper.TestPackModel<string>(packRat, "Purdue Boilermakers", "11 000000 1011");
			PackRatTestHelper.TestPackModel<string>(packRat, "Rutgers Scarlet Knights", "11 000000 1100");
			PackRatTestHelper.TestPackModel<string>(packRat, "UCLA Bruins", "11 000000 1101");
			PackRatTestHelper.TestPackModel<string>(packRat, "USC Trojans", "11 000000 1110");
			PackRatTestHelper.TestPackModel<string>(packRat, "Wisconsin Badgers", "11 000000 1111");
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			TokenPackRat<string> packRat = new TokenPackRat<string>(piedPiper);

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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			TokenPackRat<string> packRat = new TokenPackRat<string>(piedPiper);

			packRat.DefineToken("0000", "Illinois Fighting Illini");
			packRat.DefineToken("0001", "Indiana Hoosiers");
			packRat.DefineToken("0010", "Iowa Hawkeyes");
			packRat.DefineToken("0011", "Maryland Terrapins");
			packRat.DefineToken("0100", "Michigan Wolverines");
			packRat.DefineToken("0101", "Michigan State Spartans");
			packRat.DefineToken("0110", "Minnesota Golden Gophers");
			packRat.DefineToken("0111", "Nebraska Cornhuskers");
			packRat.DefineToken("1000", "Northwestern Wildcats");
			packRat.DefineToken("1001", "Ohio State Buckeyes");
			packRat.DefineToken("1010", "Penn State Nittany Lions");
			packRat.DefineToken("1011", "Purdue Boilermakers");
			packRat.DefineToken("1100", "Rutgers Scarlet Knights");
			packRat.DefineToken("1101", "UCLA Bruins");
			packRat.DefineToken("1110", "USC Trojans");
			packRat.DefineToken("1111", "Wisconsin Badgers");

			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0000", "Illinois Fighting Illini");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0001", "Indiana Hoosiers");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0010", "Iowa Hawkeyes");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0011", "Maryland Terrapins");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0100", "Michigan Wolverines");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0101", "Michigan State Spartans");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0110", "Minnesota Golden Gophers");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 0111", "Nebraska Cornhuskers");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1000", "Northwestern Wildcats");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1001", "Ohio State Buckeyes");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1010", "Penn State Nittany Lions");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1011", "Purdue Boilermakers");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1100", "Rutgers Scarlet Knights");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1101", "UCLA Bruins");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1110", "USC Trojans");
			PackRatTestHelper.TestUnpackModel<string>(packRat, "11 000000 1111", "Wisconsin Badgers");
		}
		#endregion
	}
}
