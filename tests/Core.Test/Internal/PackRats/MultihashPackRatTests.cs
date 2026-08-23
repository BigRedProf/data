using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class MultihashPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			var packRat = new MultihashPackRat(piedPiper);

			var digest = new byte[32];
			var multihash = new Multihash(digest, MultihashAlgorithm.Sha256);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.PackModel(null, multihash);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenModelIsNull()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			var packRat = new MultihashPackRat(piedPiper);

			var writer = new CodeWriter(new MemoryStream());

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.PackModel(writer, null);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			var packRat = new MultihashPackRat(piedPiper);

			Assert.Throws<ArgumentNullException>(() =>
			{
				packRat.UnpackModel(null);
			});
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldMatchExpectedCode()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			var packRat = new MultihashPackRat(piedPiper);

			// Multihash: 0x12 (SHA2-256), length: 32, digest: 32 zero bytes
			byte[] digest = new byte[32];
			var multihash = new Multihash(digest, MultihashAlgorithm.Sha256);

			// Expected code: varint 0x12 = "00010010", varint 0x20 = "00100000", 32 zero bytes
			IEnumerable<string> repeatedStrings = PackRatTestHelper.RepeatString(new string('0', 8), 32);
			string expected = PackRatTestHelper.ReverseBytes("00010010 00100000") + new string(' ', 1) + string.Join(" ", repeatedStrings);
			// helper to match that expected pattern
			PackRatTestHelper.TestPackModel<Multihash>(packRat, multihash, expected);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackAndUnpackModel_ShouldRoundTrip()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			var packRat = new MultihashPackRat(piedPiper);

			// Example digest: 32 bytes [0, 1, 2, ..., 31]
			byte[] digest = new byte[32];
			for (int i = 0; i < digest.Length; i++)
				digest[i] = (byte)i;

			var original = new Multihash(digest, MultihashAlgorithm.Sha256);
			PackRatTestHelper.TestPackUnpackModel<Multihash>(packRat, original);
		}
		#endregion

		#region private functions
		private static IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			return piedPiper;
		}
		#endregion
	}
}
