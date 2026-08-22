using BigRedProf.Data.Core;
using System;
using Xunit;

namespace BigRedProf.Data.Core.Test
{
	public class CodeBuilderTests
	{
		#region indexer tests
		[Fact]
		[Trait("Region", "indexers")]
		public void IndexerShouldSetCorrectValues()
		{
			CodeBuilder builder = new CodeBuilder(9);

			builder[0] = 0;
			builder[1] = 1;
			builder[2] = 0;
			builder[3] = 1;
			builder[4] = 0;
			builder[5] = 1;
			builder[6] = 0;
			builder[7] = 0;
			builder[8] = 1;

			Assert.Equal<Code>("010101001", builder.Build());
		}

		[Fact]
		[Trait("Region", "indexers")]
		public void IndexerShouldChangeValues()
		{
			CodeBuilder builder = new CodeBuilder(new Code(1, 0, 0, 1, 0, 1));

			builder[0] = 0;
			builder[1] = 1;
			builder[3] = 1;
			builder[5] = 1;

			Assert.Equal<Code>("010101", builder.Build());
		}

		[Fact]
		[Trait("Region", "indexers")]
		public void IndexerShouldThrowWhenOffsetIsOutOfRange()
		{
			CodeBuilder builder = new CodeBuilder(4);

			Assert.Throws<ArgumentOutOfRangeException>(() => builder[4] = 1);
			Assert.Throws<ArgumentOutOfRangeException>(() => builder[-1] = 1);
		}
		#endregion

		#region SetRange tests
		[Fact]
		[Trait("Region", "methods")]
		public void SetRangeShouldSetCorrectValues()
		{
			CodeBuilder builder = new CodeBuilder(12);

			builder.SetRange(0, "1010");
			builder.SetRange(4, "1111");
			builder.SetRange(8, "1100");

			Assert.Equal<Code>("1010 1111 1100", builder.Build());
		}

		[Fact]
		[Trait("Region", "methods")]
		public void SetRangeShouldChangeValues()
		{
			CodeBuilder builder = new CodeBuilder(new Code(0, 0, 0, 0, 1, 1, 1, 1));

			builder.SetRange(0, new Code("101"));
			builder.SetRange(5, new Code("010"));

			Assert.Equal<Code>(new Code(1, 0, 1, 0, 1, 0, 1, 0), builder.Build());
		}

		[Fact]
		[Trait("Region", "methods")]
		public void SetRangeShouldTakeTheByteAlignedFastPath()
		{
			// The old Code range setter had a latent bug here: it computed the whole-byte count
			// as (length % 8) rather than (length / 8), so anything byte-aligned and 8 bits or
			// longer copied the wrong number of bytes. Nothing exercised it.
			CodeBuilder builder = new CodeBuilder(24);

			builder.SetRange(0, "10110001 01110010");
			builder.SetRange(16, "11001110");

			Assert.Equal<Code>("10110001 01110010 11001110", builder.Build());
		}
		#endregion

		#region method tests
		[Fact]
		[Trait("Region", "methods")]
		public void Build_ShouldNotShareStorageWithTheBuilder()
		{
			// Otherwise a code could still change after it was built, which is the whole thing
			// this type exists to prevent.
			CodeBuilder builder = new CodeBuilder(4);
			builder[0] = 1;

			Code built = builder.Build();
			builder[1] = 1;

			Assert.Equal<Code>("1000", built);
			Assert.Equal<Code>("1100", builder.Build());
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Constructor_ShouldNotAliasTheSeedCode()
		{
			// The seed is immutable and must stay that way while the builder is written to.
			// Aliasing its backing array would change a value other code may already have
			// compared, fingerprinted, or used as a dictionary key.
			Code seed = new Code("0000 0000");
			CodeBuilder builder = new CodeBuilder(seed);
		
			builder[0] = 1;
			builder[7] = 1;
		
			Assert.Equal<Code>("0000 0000", seed);
			Assert.Equal<Code>("1000 0001", builder.Build());
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Constructor_ShouldSeedFromAnExistingCode()
		{
			CodeBuilder builder = new CodeBuilder(new Code("1011"));

			Assert.Equal(4, builder.Length);
			Assert.Equal<Code>("1011", builder.Build());
		}
		#endregion
	}
}
