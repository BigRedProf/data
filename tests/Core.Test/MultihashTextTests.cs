using BigRedProf.Data.Core;
using System;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class MultihashTextTests
	{
		#region ToMultibaseString
		[Fact]
		[Trait("Region", "ToMultibaseString")]
		public void ToMultibaseString_ShouldEncodeBase32LowerKnownVector()
		{
			byte[] digest = new byte[32];
			Multihash multihash = new Multihash(digest, MultihashAlgorithm.Sha256);
			string expected = "bciqaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
			string actual = multihash.ToMultibaseString();
			Assert.Equal(expected, actual);
			Multihash parsed = Multihash.Parse(expected);
			Assert.Equal(MultihashAlgorithm.Sha256, parsed.Algorithm);
			Assert.Equal(digest, parsed.Digest);
			bool success = Multihash.TryParse(expected, out Multihash parsedTry);
			Assert.True(success);
			Assert.NotNull(parsedTry);
			Assert.Equal(MultihashAlgorithm.Sha256, parsedTry.Algorithm);
			Assert.Equal(digest, parsedTry.Digest);
		}

		[Fact]
		[Trait("Region", "ToMultibaseString")]
		public void ToMultibaseString_ShouldEncodeHexLowerKnownVector()
		{
			byte[] digest = new byte[32];
			Multihash multihash = new Multihash(digest, MultihashAlgorithm.Sha256);
			string expected = "f1220" + new string('0', 64);
			string actual = multihash.ToMultibaseString(MultibaseEncoding.HexLower);
			Assert.Equal(expected, actual);
			Multihash parsed = Multihash.Parse(expected);
			Assert.Equal(MultihashAlgorithm.Sha256, parsed.Algorithm);
			Assert.Equal(digest, parsed.Digest);
		}
		#endregion

		#region Parse
		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldThrowArgumentNullExceptionForNullOrWhitespace()
		{
			Assert.Throws<ArgumentNullException>(() => Multihash.Parse(null));
			Assert.Throws<FormatException>(() => Multihash.Parse(" "));
		}

		[Fact]
		[Trait("Region", "TryParse")]
		public void TryParse_ShouldFailWhenTrailingGarbagePresent()
		{
			byte[] digest = new byte[32];
			Multihash mh = new Multihash(digest, MultihashAlgorithm.Sha256);
			string s = mh.ToMultibaseString(); // 'b...'
											   // Append a harmless base32 char
			bool ok = Multihash.TryParse(s + "a", out _);
			Assert.False(ok);
			Assert.Throws<FormatException>(() => Multihash.Parse(s + "a"));
		}

		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldRejectUnknownMulticode()
		{
			// Build: varint(0x13) | varint(32) | 32 zeros
			byte[] payload = new byte[2 + 32];
			payload[0] = 0x13; // code
			payload[1] = 0x20; // length 32
			string hex = "f" + BitConverter.ToString(payload).Replace("-", "").ToLowerInvariant();

			Assert.False(Multihash.TryParse(hex, out _));
			Assert.Throws<FormatException>(() => Multihash.Parse(hex));
		}

		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldRoundTripRandomDigests()
		{
			var rng = new Random(12345);
			for (int i = 0; i < 12; i++)
			{
				byte[] digest = new byte[32];
				rng.NextBytes(digest);
				Multihash m = new Multihash(digest, MultihashAlgorithm.Sha256);

				string b32 = m.ToMultibaseString();
				Assert.Equal(m, Multihash.Parse(b32));

				string hx = m.ToMultibaseString(MultibaseEncoding.HexLower);
				Assert.Equal(m, Multihash.Parse(hx));
			}
		}

		[Fact]
		[Trait("Region", "ToMultibaseString")]
		public void ToMultibaseString_DefaultIsBase32Lower()
		{
			var m = new Multihash(new byte[32], MultihashAlgorithm.Sha256);
			Assert.Equal(m.ToMultibaseString(MultibaseEncoding.Base32Lower), m.ToMultibaseString());
		}

		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldRoundTripKnownDigests()
		{
			byte[][] digests = new byte[][]
			{
				CreateSequentialDigest(0),
				CreateSequentialDigest(16),
				CreateSequentialDigest(32)
			};

			foreach (byte[] digest in digests)
			{
				Multihash multihash = new Multihash(digest, MultihashAlgorithm.Sha256);
				string base32 = multihash.ToMultibaseString();
				Multihash parsedBase32 = Multihash.Parse(base32);
				Assert.Equal(MultihashAlgorithm.Sha256, parsedBase32.Algorithm);
				Assert.Equal(digest, parsedBase32.Digest);
				string hex = multihash.ToMultibaseString(MultibaseEncoding.HexLower);
				Multihash parsedHex = Multihash.Parse(hex);
				Assert.Equal(MultihashAlgorithm.Sha256, parsedHex.Algorithm);
				Assert.Equal(digest, parsedHex.Digest);
			}
		}

		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldThrowFormatExceptionForInvalidCharacters()
		{
			string knownBase32 = "bciqaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
			string invalidBase32 = knownBase32.Substring(0, 2) + "*" + knownBase32.Substring(2);
			Assert.Throws<FormatException>(() => Multihash.Parse(invalidBase32));
			string invalidHex = "f1220z";
			Assert.Throws<FormatException>(() => Multihash.Parse(invalidHex));
		}

		[Fact]
		[Trait("Region", "Parse")]
		public void Parse_ShouldRejectIncorrectDigestLength()
		{
			byte[] malformed = new byte[33];
			malformed[0] = 0x12;
			malformed[1] = 0x1F;
			string hexPayload = BitConverter.ToString(malformed).Replace("-", string.Empty).ToLowerInvariant();
			string hexText = "f" + hexPayload;
			Multihash parsedHex;
			bool hexSuccess = Multihash.TryParse(hexText, out parsedHex);
			Assert.False(hexSuccess);
			Assert.Throws<FormatException>(() => Multihash.Parse(hexText));
			string base32Payload = EncodeBase32Lower(malformed);
			string base32Text = "b" + base32Payload;
			Multihash parsedBase32;
			bool base32Success = Multihash.TryParse(base32Text, out parsedBase32);
			Assert.False(base32Success);
			Assert.Throws<FormatException>(() => Multihash.Parse(base32Text));
		}
		#endregion

		#region TryParse
		[Fact]
		[Trait("Region", "TryParse")]
		public void TryParse_ShouldReturnFalseForInvalidInputs()
		{
			bool success;
			Multihash result;
			success = Multihash.TryParse("xabcdef", out result);
			Assert.False(success);
			success = Multihash.TryParse(string.Empty, out result);
			Assert.False(success);
			success = Multihash.TryParse("b", out result);
			Assert.False(success);
			success = Multihash.TryParse("f1", out result);
			Assert.False(success);
			success = Multihash.TryParse("f12g", out result);
			Assert.False(success);
		}
		#endregion

		#region private methods
		private static byte[] CreateSequentialDigest(byte startValue)
		{
			byte[] digest = new byte[32];
			for (int i = 0; i < digest.Length; i++)
			{
				digest[i] = (byte)(startValue + i);
			}
			return digest;
		}

		private static string EncodeBase32Lower(byte[] data)
		{
			const string alphabet = "abcdefghijklmnopqrstuvwxyz234567";
			if (data.Length == 0)
				return string.Empty;
			StringBuilder builder = new StringBuilder((data.Length * 8 + 4) / 5);
			int buffer = data[0];
			int next = 1;
			int bitsLeft = 8;
			while (bitsLeft > 0 || next < data.Length)
			{
				if (bitsLeft < 5)
				{
					if (next < data.Length)
					{
						buffer = (buffer << 8) | data[next];
						next++;
						bitsLeft += 8;
					}
					else
					{
						int indexValue = (buffer << (5 - bitsLeft)) & 0x1F;
						builder.Append(alphabet[indexValue]);
						bitsLeft = 0;
						continue;
					}
				}
				int outputIndex = (buffer >> (bitsLeft - 5)) & 0x1F;
				builder.Append(alphabet[outputIndex]);
				bitsLeft -= 5;
			}
			return builder.ToString();
		}
		#endregion
	}
}
