using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
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
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper, tokenizer);
			TokenizedModel<string> model = new TokenizedModel<string>
			{
				Token = new Code("0111"),
				Model = "Huskers"
			};

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
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper, tokenizer);

			tokenizer.DefineToken(new Code("0000"), "Illinois Fighting Illini");
			tokenizer.DefineToken(new Code("0001"), "Indiana Hoosiers");
			tokenizer.DefineToken(new Code("0010"), "Iowa Hawkeyes");
			tokenizer.DefineToken(new Code("0011"), "Maryland Terrapins");
			tokenizer.DefineToken(new Code("0100"), "Michigan State Spartans");
			tokenizer.DefineToken(new Code("0101"), "Michigan Wolverines");
			tokenizer.DefineToken(new Code("0110"), "Minnesota Golden Gophers");
			tokenizer.DefineToken(new Code("0111"), "Nebraska Cornhuskers");
			tokenizer.DefineToken(new Code("1000"), "Northwestern Wildcats");
			tokenizer.DefineToken(new Code("1001"), "Ohio State Buckeyes");
			tokenizer.DefineToken(new Code("1010"), "Oregon Ducks");
			tokenizer.DefineToken(new Code("1011"), "Penn State Nittany Lions");
			tokenizer.DefineToken(new Code("1100"), "Purdue Boilermakers");
			tokenizer.DefineToken(new Code("1101"), "Rutgers Scarlet Knights");
			tokenizer.DefineToken(new Code("1110"), "UCLA Bruins");
			tokenizer.DefineToken(new Code("1111"), "USC Trojans");
			tokenizer.DefineToken(new Code("10000"), "Washington Huskies");
			tokenizer.DefineToken(new Code("10001"), "Wisconsin Badgers");

			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0000"), Model = "Illinois Fighting Illini" },
				"11000000 0000"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0001"), Model = "Indiana Hoosiers" },
				"11000000 0001"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0010"), Model = "Iowa Hawkeyes" },
				"11000000 0010"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0011"), Model = "Maryland Terrapins" },
				"11000000 0011"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0100"), Model = "Michigan State Spartans" },
				"11000000 0100"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0101"), Model = "Michigan Wolverines" },
				"11000000 0101"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0110"), Model = "Minnesota Golden Gophers" },
				"11000000 0110"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("0111"), Model = "Nebraska Cornhuskers" },
				"11000000 0111"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1000"), Model = "Northwestern Wildcats" },
				"11000000 1000"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1001"), Model = "Ohio State Buckeyes" },
				"11000000 1001"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1010"), Model = "Oregon Ducks" },
				"11000000 1010"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1011"), Model = "Penn State Nittany Lions" },
				"11000000 1011"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1100"), Model = "Purdue Boilermakers" },
				"11000000 1100"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1101"), Model = "Rutgers Scarlet Knights" },
				"11000000 1101"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1110"), Model = "UCLA Bruins" },
				"11000000 1110"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("1111"), Model = "USC Trojans" },
				"11000000 1111"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("10000"), Model = "Washington Huskies" },
				"11010000 10000"
			);
			PackRatTestHelper.TestPackModel<TokenizedModel<string>>(
				packRat,
				new TokenizedModel<string> { Token = new Code("10001"), Model = "Wisconsin Badgers" },
				"11010000 10001"
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			PiedPiper piedPiper = new PiedPiper();
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper, tokenizer);

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
			Tokenizer<string> tokenizer = new Tokenizer<string>();
			TokenizedModelPackRat<string> packRat = new TokenizedModelPackRat<string>(piedPiper, tokenizer);

			tokenizer.DefineToken(new Code("0000"), "Illinois Fighting Illini");
			tokenizer.DefineToken(new Code("0001"), "Indiana Hoosiers");
			tokenizer.DefineToken(new Code("0010"), "Iowa Hawkeyes");
			tokenizer.DefineToken(new Code("0011"), "Maryland Terrapins");
			tokenizer.DefineToken(new Code("0100"), "Michigan State Spartans");
			tokenizer.DefineToken(new Code("0101"), "Michigan Wolverines");
			tokenizer.DefineToken(new Code("0110"), "Minnesota Golden Gophers");
			tokenizer.DefineToken(new Code("0111"), "Nebraska Cornhuskers");
			tokenizer.DefineToken(new Code("1000"), "Northwestern Wildcats");
			tokenizer.DefineToken(new Code("1001"), "Ohio State Buckeyes");
			tokenizer.DefineToken(new Code("1010"), "Oregon Ducks");
			tokenizer.DefineToken(new Code("1011"), "Penn State Nittany Lions");
			tokenizer.DefineToken(new Code("1100"), "Purdue Boilermakers");
			tokenizer.DefineToken(new Code("1101"), "Rutgers Scarlet Knights");
			tokenizer.DefineToken(new Code("1110"), "UCLA Bruins");
			tokenizer.DefineToken(new Code("1111"), "USC Trojans");
			tokenizer.DefineToken(new Code("10000"), "Washington Huskies");
			tokenizer.DefineToken(new Code("10001"), "Wisconsin Badgers");

			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0000",
				new TokenizedModel<string> { Token = new Code("0000"), Model = "Illinois Fighting Illini" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0001",
				new TokenizedModel<string> { Token = new Code("0001"), Model = "Indiana Hoosiers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0010",
				new TokenizedModel<string> { Token = new Code("0010"), Model = "Iowa Hawkeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0011",
				new TokenizedModel<string> { Token = new Code("0011"), Model = "Maryland Terrapins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0100",
				new TokenizedModel<string> { Token = new Code("0100"), Model = "Michigan State Spartans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0101",
				new TokenizedModel<string> { Token = new Code("0101"), Model = "Michigan Wolverines" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0110",
				new TokenizedModel<string> { Token = new Code("0110"), Model = "Minnesota Golden Gophers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 0111",
				new TokenizedModel<string> { Token = new Code("0111"), Model = "Nebraska Cornhuskers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1000",
				new TokenizedModel<string> { Token = new Code("1000"), Model = "Northwestern Wildcats" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1001",
				new TokenizedModel<string> { Token = new Code("1001"), Model = "Ohio State Buckeyes" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1010",
				new TokenizedModel<string> { Token = new Code("1010"), Model = "Oregon Ducks" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1011",
				new TokenizedModel<string> { Token = new Code("1011"), Model = "Penn State Nittany Lions" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1100",
				new TokenizedModel<string> { Token = new Code("1100"), Model = "Purdue Boilermakers" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1101",
				new TokenizedModel<string> { Token = new Code("1101"), Model = "Rutgers Scarlet Knights" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1110",
				new TokenizedModel<string> { Token = new Code("1110"), Model = "UCLA Bruins" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11000000 1111",
				new TokenizedModel<string> { Token = new Code("1111"), Model = "USC Trojans" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11010000 10000",
				new TokenizedModel<string> { Token = new Code("10000"), Model = "Washington Huskies" }
			);
			PackRatTestHelper.TestUnpackModel<TokenizedModel<string>>(
				packRat,
				"11010000 10001",
				new TokenizedModel<string> { Token = new Code("10001"), Model = "Wisconsin Badgers" }
			);
		}
		#endregion
	}
}
