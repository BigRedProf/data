using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
		public class SeriesDigestEngineTests
		{
				#region unit tests
				[Fact]
				public void BaselineSeed_ShouldMatchKnownVector()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
						Multihash baseline = engine.ComputeBaseline();

						byte[] zeroBytes = new byte[1];
						Code zeroCode = new Code(zeroBytes, 8);
						Multihash expected = Multihash.FromCode(zeroCode, MultihashAlgorithm.SHA2_256);

						Assert.Equal(expected, baseline);
				}

				[Theory]
				[InlineData("00000000")]
				[InlineData("11111111")]
				[InlineData("10101010 01010101")]
				[InlineData("11110000 00001111 101")]
				public void FromCode_ShouldMatchKnownVectors(string bitPattern)
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);

						Code code = new Code(bitPattern);
						Multihash expected = Multihash.FromCode(code, MultihashAlgorithm.SHA2_256);
						Multihash actual = engine.FromCode(code);

						Assert.Equal(expected, actual);
				}

				[Fact]
				public void ComputeContentDigest_ShouldMatchTapeReadback()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
						TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
						Guid tapeId = new Guid("10000000-0000-0000-0000-000000000001");
						Tape tape = Tape.CreateNew(tapeProvider, tapeId);

						TapeRecorder recorder = new TapeRecorder();
						recorder.InsertTape(tape);
						Code content = new Code("10101010 11110000 00001111");
						recorder.Record(content);
						recorder.EjectTape();

						Multihash digest = engine.ComputeContentDigest(tape);
						Code readback = ReadTapeContent(tape, content.Length);
						Multihash expected = Multihash.FromCode(readback, engine.Algorithm);

						Assert.Equal(expected, digest);
				}

				[Fact]
				public void ComputeContentDigest_PartialByteContent_ShouldMatchReference()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
						TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
						Guid tapeId = new Guid("20000000-0000-0000-0000-000000000002");
						Tape tape = Tape.CreateNew(tapeProvider, tapeId);

						TapeRecorder recorder = new TapeRecorder();
						recorder.InsertTape(tape);
						Code content = new Code("1010111");
						recorder.Record(content);
						recorder.EjectTape();

						Multihash digest = engine.ComputeContentDigest(tape);
						Multihash expected = engine.FromCode(content);

						Assert.Equal(expected, digest);
				}

				[Fact]
				public void ZeroLengthTape_ShouldReturnBaseline()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
						TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
						Guid tapeId = new Guid("30000000-0000-0000-0000-000000000003");
						Tape tape = Tape.CreateNew(tapeProvider, tapeId);

						Multihash digest = engine.ComputeContentDigest(tape);
						Multihash baseline = engine.ComputeBaseline();

						Assert.Equal(baseline, digest);
				}

				[Fact]
				public void ComputeSeriesHeadDigest_ComposesParentAndContent()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);

						Code parentCode = new Code("11110000");
						Code contentCode = new Code("00001111 10101010");
						Multihash parent = engine.FromCode(parentCode);
						Multihash content = engine.FromCode(contentCode);

						Multihash actual = engine.ComputeSeriesHeadDigest(parent, content);

						byte[] parentBytes = parent.Digest;
						byte[] contentBytes = content.Digest;
						byte[] combined = new byte[parentBytes.Length + contentBytes.Length];
						Array.Copy(parentBytes, 0, combined, 0, parentBytes.Length);
						Array.Copy(contentBytes, 0, combined, parentBytes.Length, contentBytes.Length);
						Code combinedCode = new Code(combined);
						Multihash expected = engine.FromCode(combinedCode);

						Assert.Equal(expected, actual);
				}

				[Fact]
				public void ComputeContentDigest_MultipleWrites_ShouldMatchCombinedReference()
				{
						ISeriesDigestEngine engine = new SeriesDigestEngine(MultihashAlgorithm.SHA2_256);
						TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
						Guid tapeId = new Guid("40000000-0000-0000-0000-000000000004");
						Tape tape = Tape.CreateNew(tapeProvider, tapeId);

						TapeRecorder recorder = new TapeRecorder();
						recorder.InsertTape(tape);
						Code first = new Code("1010");
						Code second = new Code("111100001");
						recorder.Record(first);
						recorder.Record(second);
						recorder.EjectTape();

						Multihash digest = engine.ComputeContentDigest(tape);
						int combinedLength = first.Length + second.Length;
						Code combined = ReadTapeContent(tape, combinedLength);
						Multihash expected = Multihash.FromCode(combined, engine.Algorithm);

						Assert.Equal(expected, digest);
				}
				#endregion

				#region private static methods
				private static Code ReadTapeContent(Tape tape, int length)
				{
						if (tape == null)
								throw new ArgumentNullException(nameof(tape));

						if (length <= 0)
								throw new ArgumentOutOfRangeException(nameof(length));

						TapePlayer player = new TapePlayer();
						player.InsertTape(tape);
						player.RewindOrFastForwardTo(0);
						Code content = player.Play(length);
						player.EjectTape();
						return content;
				}
				#endregion
		}
}

