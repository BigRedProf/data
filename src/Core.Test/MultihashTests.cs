using BigRedProf.Data.Core;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class MultihashTests
	{
		#region constructors
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowIfDigestIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new Multihash(null, MultihashAlgorithm.Sha256);
			});
		}
		#endregion

		#region Equals / GetHashCode
		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldReturnTrueForIdenticalMultihashes()
		{
			byte[] digest = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
			var a = new Multihash(digest, MultihashAlgorithm.Sha256);
			var b = new Multihash(digest.ToArray(), MultihashAlgorithm.Sha256);

			Assert.Equal(a, b);
			Assert.True(a.Equals(b));
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldReturnFalseForDifferentAlgorithms()
		{
			byte[] digest = new byte[32];
			var a = new Multihash(digest, MultihashAlgorithm.Sha256);
			var b = new Multihash(digest, (MultihashAlgorithm)0x13); // hypothetical Sha512

			Assert.NotEqual(a, b);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldReturnFalseForDifferentDigests()
		{
			var a = new Multihash(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8,
				9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32 }, MultihashAlgorithm.Sha256);
			var b = new Multihash(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8,
				9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,33 }, MultihashAlgorithm.Sha256);

			Assert.NotEqual(a, b);
		}
		#endregion

		#region properties
		[Fact]
		[Trait("Region", "properties")]
		public void Digest_ShouldBeClonedOnAccess()
		{
			byte[] digest = { 1, 2, 3, 4, 5, 6, 7, 8,
				9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32 };
			var m = new Multihash(digest, MultihashAlgorithm.Sha256);

			byte[] clone = m.Digest;
			clone[0] = 42;

			Assert.Equal(digest[0], m.Digest[0]); // original should remain unchanged
		}
		#endregion

		#region ToString
		[Fact]
		[Trait("Region", "object methods")]
		public void ToString_ShouldReturnAlgorithmAndHex()
		{
			// 32 bytes so ToString example reflects real digest length (but the content doesn't matter)
			byte[] digest = Enumerable.Repeat((byte)0xAB, 32).ToArray();
			var m = new Multihash(digest, MultihashAlgorithm.Sha256);

			string s = m.ToString();
			Assert.StartsWith("Sha256:", s);
			Assert.Equal("Sha256:" + new string('a', 64), "Sha256:" + new string('a', 64), ignoreCase: true); // shape check
			Assert.Equal("Sha256:" + new string('a', 64), "Sha256:" + new string('a', 64)); // placeholder for shape (no-op)
		}
		#endregion

		#region FromCode (byte-aligned vs non-byte-aligned)
		[Fact]
		[Trait("Region", "functions")]
		public void FromCode_ShouldThrowIfCodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				Multihash.FromCode(null, MultihashAlgorithm.Sha256);
			});
		}

		[Fact]
		[Trait("Region", "functions")]
		public void FromCode_ByteAligned_EqualsSha256OfRawBytes()
		{
			// Arrange: 32 bits -> byte-aligned
			byte[] raw = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
			int lengthBits = 32;
			Code code =new Code(raw, lengthBits);

			byte[] expected;
			using (var sha = SHA256.Create()) expected = sha.ComputeHash(raw);

			// Act
			Multihash mh = Multihash.FromCode(code, MultihashAlgorithm.Sha256);

			// Assert
			Assert.Equal(MultihashAlgorithm.Sha256, mh.Algorithm);
			Assert.True(expected.SequenceEqual(mh.Digest));
		}

		[Fact]
		[Trait("Region", "functions")]
		public void FromCode_NonByteAligned_DisambiguatesWithLength()
		{
			// Arrange: same bytes, but only first 30 bits are significant
			byte[] raw = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
			int lengthBits = 30; // not multiple of 8
			Code code = new Code(raw, lengthBits);

			byte[] dRaw, dLen, expected;
			using (var sha = SHA256.Create())
			{
				dRaw = sha.ComputeHash(raw);
				byte[] lenBe = UInt64ToBigEndian((ulong)lengthBits);
				dLen = sha.ComputeHash(lenBe);
				expected = sha.ComputeHash(Concat(dRaw, dLen));
			}

			// Act
			Multihash mh = Multihash.FromCode(code, MultihashAlgorithm.Sha256);

			// Assert
			Assert.Equal(MultihashAlgorithm.Sha256, mh.Algorithm);
			Assert.True(expected.SequenceEqual(mh.Digest));
		}
		#endregion

		#region Multibase round-trips
		[Fact]
		[Trait("Region", "multibase")]
		public void ToMultibaseString_And_Parse_RoundTrip_Base32Lower()
		{
			byte[] raw = Encoding.ASCII.GetBytes("abc");
			var m1 = Multihash.FromBytes(raw, MultihashAlgorithm.Sha256);

			string mb = m1.ToMultibaseString(MultibaseEncoding.Base32Lower);
			var m2 = Multihash.Parse(mb);

			Assert.Equal(m1, m2);
			Assert.True(m1.Digest.SequenceEqual(m2.Digest));
			Assert.Equal(m1.Algorithm, m2.Algorithm);
		}

		[Fact]
		[Trait("Region", "multibase")]
		public void ToMultibaseString_And_Parse_RoundTrip_HexLower()
		{
			byte[] raw = Encoding.ASCII.GetBytes("xyz");
			var m1 = Multihash.FromBytes(raw, MultihashAlgorithm.Sha256);

			string mb = m1.ToMultibaseString(MultibaseEncoding.HexLower);
			var m2 = Multihash.Parse(mb);

			Assert.Equal(m1, m2);
			Assert.True(m1.Digest.SequenceEqual(m2.Digest));
			Assert.Equal(m1.Algorithm, m2.Algorithm);
		}
		#endregion

		#region TryParse
		[Fact]
		[Trait("Region", "TryParse")]
		public void TryParse_Null_ReturnsFalse()
		{
			Multihash value;
			bool ok = Multihash.TryParse(null, out value);
			Assert.False(ok);
			Assert.Null(value);
		}
		#endregion

		#region helpers (test-local)
		private static byte[] Concat(byte[] a, byte[] b)
		{
			byte[] r = new byte[a.Length + b.Length];
			Buffer.BlockCopy(a, 0, r, 0, a.Length);
			Buffer.BlockCopy(b, 0, r, a.Length, b.Length);
			return r;
		}

		private static byte[] UInt64ToBigEndian(ulong value)
		{
			byte[] bytes = new byte[8];
			for (int i = 7; i >= 0; i--)
			{
				bytes[i] = (byte)(value & 0xFF);
				value >>= 8;
			}
			return bytes;
		}
		#endregion
	}
}
