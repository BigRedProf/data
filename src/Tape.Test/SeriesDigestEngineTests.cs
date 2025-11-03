using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape.Providers.Memory;
using System;
using Xunit;
using Xunit.Abstractions;

namespace BigRedProf.Data.Tape.Test
{
	public class SeriesDigestEngineTests
	{
		#region fields
		private readonly ITestOutputHelper _output;
		#endregion

		#region constructors
		public SeriesDigestEngineTests(ITestOutputHelper output)
		{
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			_output = output;
		}
		#endregion

		#region unit tests
		[Fact]
		public void ComputeBaseline_ShouldMatchKnownVector()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);

			Multihash baseline = engine.ComputeBaseline();
			string baselineText = baseline.ToMultibaseString();
			_output.WriteLine($"Baseline: {baselineText}");

			Assert.Equal("bciqg4naltt73g6uytssujzv3pafcy6eqdu73gnzyo2crdiygc6x2ahi", baselineText);
		}

		[Fact]
		public void FromCode_ShouldMatchKnownVectors()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);

			Code[] codes = new Code[]
			{
				new Code(new byte[] { 0x00 }, 8),
				new Code(new byte[] { 0xFF }, 8),
				new Code("10101010 01010101"),
				new Code("11110000 00001111 1")
			};

			string[] expected = new string[]
			{
				"bciqg4naltt73g6uytssujzv3pafcy6eqdu73gnzyo2crdiygc6x2ahi",
				"bciqkqeak42vbsqgqwzr3wmonizquf255xviyoey3slmtqgeypazoxci",
				"bciqgjcvmlrrtf45gbmefbmla5i5srqusyhcvf57bhnbrtqq6ecuprgy",
				"bciqcbjmcxoww3x3jtaaqbtxptnhk3lyjbdl7wf5pnqoxpolmjnm72lq"
			};

			for (int i = 0; i < codes.Length; i++)
			{
				Multihash digest = engine.FromCode(codes[i]);
				string actual = digest.ToMultibaseString();
				_output.WriteLine($"Vector {i}: {actual}");
				Assert.Equal(expected[i], actual);
			}
		}

		[Fact]
		public void ComputeContentDigest_ShouldReturnBaselineForZeroLengthTape()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
			MemoryTapeProvider provider = new MemoryTapeProvider();
			Guid tapeId = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
			Tape tape = Tape.CreateNew(provider, tapeId);

			Multihash digest = engine.ComputeContentDigest(tape);
			Multihash baseline = engine.ComputeBaseline();

			Assert.Equal(baseline, digest);
		}

		[Fact]
		public void ComputeContentDigest_ShouldMatchWrittenContent()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
			MemoryTapeProvider provider = new MemoryTapeProvider();
			Guid tapeId = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
			Tape tape = Tape.CreateNew(provider, tapeId);

			TapeRecorder recorder = new TapeRecorder();
			recorder.InsertTape(tape);
			Code content = new Code("10101010 11110000 00001111");
			recorder.Record(content);

			Multihash digest = engine.ComputeContentDigest(tape);
			Multihash expected = engine.FromCode(content);

			Assert.Equal(expected, digest);
		}

		[Fact]
		public void ComputeContentDigest_ShouldHandlePartialByteContent()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
			MemoryTapeProvider provider = new MemoryTapeProvider();
			Guid tapeId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
			Tape tape = Tape.CreateNew(provider, tapeId);

			TapeRecorder recorder = new TapeRecorder();
			recorder.InsertTape(tape);
			Code content = new Code("10101");
			recorder.Record(content);

			Multihash digest = engine.ComputeContentDigest(tape);
			Multihash expected = engine.FromCode(content);

			Assert.Equal(expected, digest);
		}

		[Fact]
		public void ComputeSeriesHeadDigest_ShouldComposeParentAndContent()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
			Code parentCode = new Code("01010101 10101010");
			Code contentCode = new Code("11110000 00001111");
			Multihash parentDigest = engine.FromCode(parentCode);
			Multihash contentDigest = engine.FromCode(contentCode);

			Multihash head = engine.ComputeSeriesHeadDigest(parentDigest, contentDigest);
			string headText = head.ToMultibaseString();
			_output.WriteLine($"SeriesHead: {headText}");

			Assert.Equal("bciqi7q345es2gjpkz6cjn47az32yaima7nd56eytw4xxwbyvafw77ii", headText);
		}

		[Fact]
		public void MultiTapeBoundary_PartialByteThenRollover()
		{
			SeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
			MemoryTapeProvider provider = new MemoryTapeProvider();

			Guid firstTapeId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd");
			Tape firstTape = Tape.CreateNew(provider, firstTapeId);
			TapeRecorder firstRecorder = new TapeRecorder();
			firstRecorder.InsertTape(firstTape);
			Code firstSegment = new Code("1110001");
			firstRecorder.Record(firstSegment);

			Guid secondTapeId = new Guid("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
			Tape secondTape = Tape.CreateNew(provider, secondTapeId);
			TapeRecorder secondRecorder = new TapeRecorder();
			secondRecorder.InsertTape(secondTape);
			Code secondSegment = new Code("0101");
			secondRecorder.Record(secondSegment);

			Multihash firstDigest = engine.ComputeContentDigest(firstTape);
			Multihash secondDigest = engine.ComputeContentDigest(secondTape);
			Multihash expectedFirst = engine.FromCode(firstSegment);
			Multihash expectedSecond = engine.FromCode(secondSegment);
			Multihash head = engine.ComputeSeriesHeadDigest(firstDigest, secondDigest);
			string headText = head.ToMultibaseString();
			_output.WriteLine($"ChainHead: {headText}");

			Assert.Equal(expectedFirst, firstDigest);
			Assert.Equal(expectedSecond, secondDigest);
			Assert.Equal("bciqetwxbjvld7fjoahyr4wf37q6eitouicdpu7hbaoqyjtxz3gnl3wi", headText);
		}
		#endregion
	}
}
