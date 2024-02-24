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

			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper);
			TokenizedModel<string> model = new TokenizedModel<string>();
			model.Token = "0111";
			model.Model = "Huskers";

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
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper);

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

			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0000", Model = "Illinois Fighting Illini" },
				"11 000000 0000"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0001", Model = "Indiana Hoosiers" },
				"11 000000 0001"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0010", Model = "Iowa Hawkeyes" },
				"11 000000 0010"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0011", Model = "Maryland Terrapins" },
				"11 000000 0011"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0100", Model = "Michigan Wolverines" },
				"11 000000 0100"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0101", Model = "Michigan State Spartans" },
				"11 000000 0101"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0110", Model = "Minnesota Golden Gophers" },
				"11 000000 0110"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "0111", Model = "Nebraska Cornhuskers" },
				"11 000000 0111"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1000", Model = "Northwestern Wildcats" },
				"11 000000 1000"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1001", Model = "Ohio State Buckeyes" },
				"11 000000 1001"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1010", Model = "Penn State Nittany Lions" },
				"11 000000 1010"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1011", Model = "Purdue Boilermakers" },
				"11 000000 1011"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1100", Model = "Rutgers Scarlet Knights" },
				"11 000000 1100"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1101", Model = "UCLA Bruins" },
				"11 000000 1101"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1110", Model = "USC Trojans" },
				"11 000000 1110"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = "1111", Model = "Wisconsin Badgers" },
				"11 000000 1111"
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper);

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
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper);

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

			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0000",
				new TokenizedModel<string> { Token = "0000", Model = "Illinois Fighting Illini" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0001",
				new TokenizedModel<string> { Token = "0001", Model = "Indiana Hoosiers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0010",
				new TokenizedModel<string> { Token = "0010", Model = "Iowa Hawkeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0011",
				new TokenizedModel<string> { Token = "0011", Model = "Maryland Terrapins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0100",
				new TokenizedModel<string> { Token = "0100", Model = "Michigan Wolverines" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0101",
				new TokenizedModel<string> { Token = "0101", Model = "Michigan State Spartans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0110",
				new TokenizedModel<string> { Token = "0110", Model = "Minnesota Golden Gophers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 0111",
				new TokenizedModel<string> { Token = "0111", Model = "Nebraska Cornhuskers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1000",
				new TokenizedModel<string> { Token = "1000", Model = "Northwestern Wildcats" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1001",
				new TokenizedModel<string> { Token = "1001", Model = "Ohio State Buckeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1010",
				new TokenizedModel<string> { Token = "1010", Model = "Penn State Nittany Lions" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1011",
				new TokenizedModel<string> { Token = "1011", Model = "Purdue Boilermakers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1100",
				new TokenizedModel<string> { Token = "1100", Model = "Rutgers Scarlet Knights" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1101",
				new TokenizedModel<string> { Token = "1101", Model = "UCLA Bruins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1110",
				new TokenizedModel<string> { Token = "1110", Model = "USC Trojans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11 000000 1111",
				new TokenizedModel<string> { Token = "1111", Model = "Wisconsin Badgers" }
			);
		}
		#endregion
	}
}
