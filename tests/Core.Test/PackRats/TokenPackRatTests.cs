using BigRedProf.Data.Core;
using BigRedProf.Data.Core.PackRats;
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
			Code token = new Code("0111");
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

			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Illinois Fighting Illini",
				"11000000 0000"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Indiana Hoosiers",
				"11000000 0001"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Iowa Hawkeyes",
				"11000000 0010"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Maryland Terrapins",
				"11000000 0011"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Michigan State Spartans",
				"11000000 0100"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Michigan Wolverines",
				"11000000 0101"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Minnesota Golden Gophers",
				"11000000 0110"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Nebraska Cornhuskers",
				"11000000 0111"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Northwestern Wildcats",
				"11000000 1000"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Ohio State Buckeyes",
				"11000000 1001"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Oregon Ducks",
				"11000000 1010"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Penn State Nittany Lions",
				"11000000 1011"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Purdue Boilermakers",
				"11000000 1100"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Rutgers Scarlet Knights",
				"11000000 1101"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"UCLA Bruins",
				"11000000 1110"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"USC Trojans",
				"11000000 1111"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Washington Huskies",
				"11010000 10000"
			);
			PackRatTestHelper.TestPackModel<string>(
				packRat,
				"Wisconsin Badgers",
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

			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0000",
				"Illinois Fighting Illini"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0001",
				"Indiana Hoosiers"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0010",
				"Iowa Hawkeyes"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0011",
				"Maryland Terrapins"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0100",
				"Michigan State Spartans"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0101",
				"Michigan Wolverines"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0110",
				"Minnesota Golden Gophers"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 0111",
				"Nebraska Cornhuskers"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1000",
				"Northwestern Wildcats"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1001",
				"Ohio State Buckeyes"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1010",
				"Oregon Ducks"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1011",
				"Penn State Nittany Lions"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1100",
				"Purdue Boilermakers"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1101",
				"Rutgers Scarlet Knights"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1110",
				"UCLA Bruins"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11000000 1111",
				"USC Trojans"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11010000 10000",
				"Washington Huskies"
			);
			PackRatTestHelper.TestUnpackModel<string>(
				packRat,
				"11010000 10001",
				"Wisconsin Badgers"
			);
		}
		#endregion
	}
}
