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
			TokenValue<string> model = new TokenValue<string>();
			model.Token = "0111";
			model.Value = "Huskers";

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

			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0000", Value = "Illinois Fighting Illini" },
				"11 000000 0000"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0001", Value = "Indiana Hoosiers" },
				"11 000000 0001"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0010", Value = "Iowa Hawkeyes" },
				"11 000000 0010"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0011", Value = "Maryland Terrapins" },
				"11 000000 0011"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0100", Value = "Michigan Wolverines" },
				"11 000000 0100"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0101", Value = "Michigan State Spartans" },
				"11 000000 0101"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0110", Value = "Minnesota Golden Gophers" },
				"11 000000 0110"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "0111", Value = "Nebraska Cornhuskers" },
				"11 000000 0111"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1000", Value = "Northwestern Wildcats" },
				"11 000000 1000"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1001", Value = "Ohio State Buckeyes" },
				"11 000000 1001"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1010", Value = "Penn State Nittany Lions" },
				"11 000000 1010"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1011", Value = "Purdue Boilermakers" },
				"11 000000 1011"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1100", Value = "Rutgers Scarlet Knights" },
				"11 000000 1100"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1101", Value = "UCLA Bruins" },
				"11 000000 1101"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1110", Value = "USC Trojans" },
				"11 000000 1110"
			);
			PackRatTestHelper.TestPackModel<TokenValue<string>>(
				packRat,
				new TokenValue<string> { Token = "1111", Value = "Wisconsin Badgers" },
				"11 000000 1111"
			);
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

			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0000",
				new TokenValue<string> { Token = "0000", Value = "Illinois Fighting Illini" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0001",
				new TokenValue<string> { Token = "0001", Value = "Indiana Hoosiers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0010",
				new TokenValue<string> { Token = "0010", Value = "Iowa Hawkeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0011",
				new TokenValue<string> { Token = "0011", Value = "Maryland Terrapins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0100",
				new TokenValue<string> { Token = "0100", Value = "Michigan Wolverines" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0101",
				new TokenValue<string> { Token = "0101", Value = "Michigan State Spartans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0110",
				new TokenValue<string> { Token = "0110", Value = "Minnesota Golden Gophers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 0111",
				new TokenValue<string> { Token = "0111", Value = "Nebraska Cornhuskers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1000",
				new TokenValue<string> { Token = "1000", Value = "Northwestern Wildcats" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1001",
				new TokenValue<string> { Token = "1001", Value = "Ohio State Buckeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1010",
				new TokenValue<string> { Token = "1010", Value = "Penn State Nittany Lions" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1011",
				new TokenValue<string> { Token = "1011", Value = "Purdue Boilermakers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1100",
				new TokenValue<string> { Token = "1100", Value = "Rutgers Scarlet Knights" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1101",
				new TokenValue<string> { Token = "1101", Value = "UCLA Bruins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1110",
				new TokenValue<string> { Token = "1110", Value = "USC Trojans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenValue<string>>(
				packRat,
				"11 000000 1111",
				new TokenValue<string> { Token = "1111", Value = "Wisconsin Badgers" }
			);
		}
		#endregion
	}
}
