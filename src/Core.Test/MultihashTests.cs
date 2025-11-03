using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Linq;
using System.Security.Cryptography;
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
			var b = new Multihash(digest, (MultihashAlgorithm)0x13); // hypothetical SHA2-512

			Assert.NotEqual(a, b);
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldReturnFalseForDifferentDigests()
		{
			var a = new Multihash(new byte[] { 1, 2, 3 }, MultihashAlgorithm.Sha256);
			var b = new Multihash(new byte[] { 1, 2, 4 }, MultihashAlgorithm.Sha256);

			Assert.NotEqual(a, b);
		}
		#endregion

		#region properties
		[Fact]
		[Trait("Region", "properties")]
		public void Digest_ShouldBeClonedOnAccess()
		{
			byte[] digest = { 1, 2, 3 };
			var m = new Multihash(digest, MultihashAlgorithm.Sha256);

			byte[] clone = m.Digest;
			clone[0] = 42;

			Assert.Equal(1, m.Digest[0]); // original should remain unchanged
		}
		#endregion

		#region ToString
		[Fact]
		[Trait("Region", "object methods")]
		public void ToString_ShouldReturnAlgorithmAndHex()
		{
			byte[] digest = { 0xAB, 0xCD, 0xEF };
			var m = new Multihash(digest, MultihashAlgorithm.Sha256);

			Assert.Equal("Sha256:abcdef", m.ToString());
		}
		#endregion

		#region FromCode
		[Fact]
		[Trait("Region", "functions")]
		public void FromCode_ShouldThrowIfCodeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				Multihash.FromCode(null, MultihashAlgorithm.Sha256);
			});
		}

		//[Fact]
		//[Trait("Region", "functions")]
		//public void FromCode_ShouldHashPackedCode()
		//{
		//	var piedPiper = PackRatTestHelper.GetPiedPiper();

		//	Code inputCode = new Code("1001"); // 1-bit value: 1
		//	var mh = Multihash.FromCode(inputCode, MultihashAlgorithm.SHA2_256);

		//	Code packed = piedPiper.EncodeModel<Code>(inputCode, CoreSchema.Code);
		//	using var sha256 = SHA256.Create();
		//	byte[] expectedDigest = sha256.ComputeHash(packed.ToByteArray());

		//	Assert.Equal(expectedDigest, mh.Digest);
		//	Assert.Equal(MultihashAlgorithm.SHA2_256, mh.Algorithm);
		//}
		#endregion
	}
}
