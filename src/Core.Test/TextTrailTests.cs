using BigRedProf.Data.Core;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class TextTrailTests
	{
		#region constructors
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowIfTrailIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				new TextTrail(null);
			});
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowIfTrailHasZeroSegments()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new TextTrail();
			});
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldSucceedForOneSegment()
		{
			TextTrail trail = new TextTrail("segment1");
			Assert.Single(trail.Segments);
			Assert.Equal("segment1", trail.Segments[0]);
		}
		[Fact]
		[Trait("Region", "constructors")]

		public void Constructor_ShouldSucceedForMultipleSegments()
		{
			TextTrail trail = new TextTrail("segment1", "segment2", "segment3");
			Assert.Equal(3, trail.Segments.Count);
			Assert.Equal("segment1", trail.Segments[0]);
			Assert.Equal("segment2", trail.Segments[1]);
			Assert.Equal("segment3", trail.Segments[2]);
		}
		#endregion

		#region properties
		[Fact]
		[Trait("Region", "properties")]
		public void Indexer_ShouldThrowIfIndexIsOutOfRange()
		{
			TextTrail trail = new TextTrail("seg1", "seg2");
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var s = trail.Segments[-1];
			});
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				var s = trail.Segments[2];
			});
		}

		[Fact]
		[Trait("Region", "properties")]
		public void Indexer_ShouldReturnSegments()
		{
			TextTrail trail = new TextTrail("seg1", "seg2", "seg3");
			Assert.Equal("seg1", trail.Segments[0]);
			Assert.Equal("seg2", trail.Segments[1]);
			Assert.Equal("seg3", trail.Segments[2]);
		}
		#endregion

		#region methods
		[Fact]
		[Trait("Region", "methods")]
		public void GetMultihash_ShouldReturnConsistentHash()
		{
			TextTrail trail = new TextTrail("seg1", "seg2", "seg3");
			Multihash hash1 = trail.GetMultihash(MultihashAlgorithm.Sha256);
			Multihash hash2 = trail.GetMultihash(MultihashAlgorithm.Sha256);
			Assert.Equal(hash1, hash2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetMultihash_ShouldReturnCorrectHash()
		{
			TextTrail trail = new TextTrail("one", "two", "three");
			string expectedHash = "bciqljmh4xwslot4f53thzef6haaqw2ytweiny4dlcofj66zejnqlm6y";

			Multihash hash = trail.GetMultihash(MultihashAlgorithm.Sha256);

			Assert.Equal(expectedHash, hash.ToMultibaseString());
		}
		#endregion
	}
}
