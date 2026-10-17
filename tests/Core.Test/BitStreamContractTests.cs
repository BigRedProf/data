using BigRedProf.Data.Core;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace BigRedProf.Data.Test
{
	public class BitStreamContractTests
	{
		#region constants
		/// <summary>
		/// Fixed seed for the randomized sequences. Logged when that test runs.
		/// </summary>
		private const int RandomSeed = unchecked((int)0x0B1757EA);
		#endregion

		#region fields
		private readonly ITestOutputHelper _output;
		#endregion

		#region constructors
		public BitStreamContractTests(ITestOutputHelper output)
		{
			_output = output;
		}
		#endregion

		#region hand-worked bytes
		[Fact]
		public void IssueExample_Is05Then0F()
		{
			// Write [1,0,1], align, write [1,1,1,1,0,0,0,0]. Hand result: 0x05 0x0F.
			byte[] hand = new byte[] { 0x05, 0x0F };

			ReferenceBitModel model = new ReferenceBitModel();
			model.WriteBits("101");
			model.Align();
			model.WriteBits("11110000");
			Assert.Equal(hand, model.ToByteArray());

			MemoryStream stream = new MemoryStream();
			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				writer.WriteBits("101");
				Assert.Equal(3, writer.Position);
				writer.AlignToNextByteBoundary();
				Assert.Equal(8, writer.Position);
				writer.WriteBits("11110000");
				Assert.Equal(16, writer.Position);
			}

			Assert.Equal(hand, stream.ToArray());

			using (StreamBitReader reader = new StreamBitReader(new MemoryStream(hand)))
			{
				Assert.Equal(0, reader.Position);
				Assert.Equal<Code>("101", reader.ReadBits(3));
				Assert.Equal(3, reader.Position);
				reader.AlignToNextByteBoundary();
				Assert.Equal(8, reader.Position);
				Assert.Equal<Code>("11110000", reader.ReadBits(8));
				Assert.Equal(16, reader.Position);
			}
		}

		[Fact]
		public void EveryStartingBitOffset_MatchesHandWorkedBytes()
		{
			// Prefix of k zero bits, then 101, then the dispose pad. Worked by hand.
			byte[][] hand = new byte[][]
			{
				new byte[] { 0x05 },
				new byte[] { 0x0A },
				new byte[] { 0x14 },
				new byte[] { 0x28 },
				new byte[] { 0x50 },
				new byte[] { 0xA0 },
				new byte[] { 0x40, 0x01 },
				new byte[] { 0x80, 0x02 }
			};

			for (int offset = 0; offset < 8; ++offset)
			{
				ReferenceBitModel model = new ReferenceBitModel();
				if (offset > 0)
					model.WriteBits(new Code(offset));

				model.WriteBits("101");
				Assert.Equal(hand[offset], model.ToByteArray());

				MemoryStream stream = new MemoryStream();
				using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
				{
					Assert.Equal(0, writer.Position);
					if (offset > 0)
						writer.WriteBits(new Code(offset));

					writer.WriteBits("101");
				}

				Assert.Equal(hand[offset], stream.ToArray());

				using (StreamBitReader reader = new StreamBitReader(new MemoryStream(hand[offset])))
				{
					if (offset > 0)
						Assert.Equal(new Code(offset), reader.ReadBits(offset));

					Assert.Equal<Code>("101", reader.ReadBits(3));
				}
			}
		}
		#endregion

		#region exhaustive and random
		[Fact]
		public void ExhaustiveSmallCodes_MatchTheReferenceModel()
		{
			for (int offset = 0; offset < 8; ++offset)
			{
				for (int length = 0; length <= 8; ++length)
				{
					int patterns = 1 << length;
					for (int pattern = 0; pattern < patterns; ++pattern)
						AssertWriteMatchesModel(offset, Pattern(length, pattern));
				}
			}

			for (int lengthA = 0; lengthA <= 4; ++lengthA)
			{
				int patternsA = 1 << lengthA;
				for (int patternA = 0; patternA < patternsA; ++patternA)
				{
					Code first = Pattern(lengthA, patternA);
					for (int lengthB = 0; lengthB <= 4; ++lengthB)
					{
						int patternsB = 1 << lengthB;
						for (int patternB = 0; patternB < patternsB; ++patternB)
						{
							Code second = Pattern(lengthB, patternB);
							AssertSequenceMatchesModel(first, false, false, second);
							AssertSequenceMatchesModel(first, true, false, second);
							AssertSequenceMatchesModel(first, false, true, second);
						}
					}
				}
			}
		}

		[Fact]
		public void RandomSequences_MatchTheReferenceModel()
		{
			_output.WriteLine("BitStream random seed 0x0B1757EA");

			Random random = new Random(RandomSeed);
			for (int trial = 0; trial < 20; ++trial)
			{
				int operationCount = 40 + random.Next(40);
				ReferenceBitModel model = new ReferenceBitModel();
				List<Operation> operations = new List<Operation>();
				MemoryStream stream = new MemoryStream();
				using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
				{
					for (int i = 0; i < operationCount; ++i)
					{
						int kind = random.Next(5);
						if (kind <= 2)
						{
							Code code = Pattern(random.Next(0, 41), random);
							writer.WriteBits(code);
							model.WriteBits(code);
							operations.Add(new Operation(OperationKind.Write, code));
						}
						else if (kind == 3)
						{
							writer.AlignToNextByteBoundary();
							model.Align();
							operations.Add(new Operation(OperationKind.Align, new Code(0)));
						}
						else
						{
							writer.Flush();
							writer.Flush();
							model.Flush();
							operations.Add(new Operation(OperationKind.Flush, new Code(0)));
						}

						Assert.Equal(model.Length, writer.Position);
					}
				}

				byte[] expectedBytes = model.ToByteArray();
				byte[] actualBytes = stream.ToArray();
				Assert.Equal(expectedBytes, actualBytes);

				using (StreamBitReader reader = new StreamBitReader(new MemoryStream(expectedBytes)))
				{
					int cursor = 0;
					foreach (Operation operation in operations)
					{
						if (operation.Kind == OperationKind.Write)
						{
							Code expected = model.Read(cursor, operation.Code.Length);
							Code actual = reader.ReadBits(operation.Code.Length);
							Assert.Equal(expected, actual);
							cursor += operation.Code.Length;
							Assert.Equal(cursor, reader.Position);
						}
						else if (operation.Kind == OperationKind.Align)
						{
							reader.AlignToNextByteBoundary();
							int remainder = cursor % 8;
							if (remainder != 0)
								cursor += 8 - remainder;

							Assert.Equal(cursor, reader.Position);
						}
						else
						{
							reader.Flush();
							reader.Flush();
							Assert.Equal(cursor, reader.Position);
						}
					}
				}
			}
		}
		#endregion

		#region streams
		[Fact]
		public void ShortReadsAndNonSeekableStreams_RoundTrip()
		{
			byte[] payload = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC };
			TrickleStream trickle = new TrickleStream(payload, 1);
			Assert.False(trickle.CanSeek);

			using (StreamBitReader reader = new StreamBitReader(trickle, leaveOpen: true))
			{
				Assert.False(reader.CanSeek);
				Assert.Throws<NotSupportedException>(
					() =>
					{
						reader.Position = 0;
					}
				);
				Assert.Equal(new Code(payload, 80), reader.ReadBits(80));
				Assert.Equal(80, reader.Position);
			}

			TrickleStream truncated = new TrickleStream(new byte[] { 0xFF, 0xFF }, 1);
			StreamBitReader truncatedReader = new StreamBitReader(truncated);
			EndOfStreamException end = Assert.Throws<EndOfStreamException>(
				() =>
				{
					truncatedReader.ReadBits(24);
				}
			);
			InvalidOperationException faulted = Assert.Throws<InvalidOperationException>(
				() =>
				{
					truncatedReader.ReadBits(0);
				}
			);
			Assert.Same(end, faulted.InnerException);
			Assert.Equal(0, truncatedReader.Position);

			ForwardOnlyStream forward = new ForwardOnlyStream();
			using (StreamBitWriter writer = new StreamBitWriter(forward, leaveOpen: true))
			{
				Assert.False(writer.CanRead);
				Assert.False(writer.CanSeek);
				Assert.Throws<NotSupportedException>(
					() =>
					{
						writer.Position = 3;
					}
				);
				writer.WriteBits("101");
				writer.AlignToNextByteBoundary();
				writer.WriteBits("11110000");
			}

			Assert.Equal(new byte[] { 0x05, 0x0F }, forward.ToArray());
			Assert.Throws<ArgumentException>(() => new StreamBitReader(forward));
		}

		[Fact]
		public void Position_StartsAtZeroAndCountsAlignment()
		{
			MemoryStream stream = new MemoryStream();
			stream.WriteByte(0xAB);
			Assert.Equal(1, stream.Position);

			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				Assert.Equal(0, writer.Position);
				writer.WriteBits("101");
				Assert.Equal(3, writer.Position);
				writer.AlignToNextByteBoundary();
				Assert.Equal(8, writer.Position);
				writer.Flush();
				Assert.Equal(8, writer.Position);
			}

			Assert.Equal(new byte[] { 0xAB, 0x05 }, stream.ToArray());

			MemoryStream source = new MemoryStream(new byte[] { 0xFF, 0x05, 0x0F });
			source.Position = 1;
			using (StreamBitReader reader = new StreamBitReader(source, leaveOpen: true))
			{
				Assert.Equal(0, reader.Position);
				Assert.Equal<Code>("101", reader.ReadBits(3));
				reader.AlignToNextByteBoundary();
				Assert.Equal(8, reader.Position);
			}
		}
		#endregion

		#region failures
		[Fact]
		public void InjectedWriteFailure_FaultsAndDoesNotWriteAgain()
		{
			InjectedStream stream = new InjectedStream();
			stream.FailOnWriteAttempt = 1;
			StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true);

			InjectedException first = Assert.Throws<InjectedException>(
				() =>
				{
					writer.WriteBits(new Code(new byte[] { 0xA5 }));
				}
			);
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Empty(stream.CommittedBytes);

			InvalidOperationException second = Assert.Throws<InvalidOperationException>(
				() =>
				{
					writer.WriteBits(new Code(new byte[] { 0xFF }));
				}
			);
			Assert.Same(first, second.InnerException);
			Assert.Equal(1, stream.WriteAttempts);

			Assert.Equal(0, writer.Position);
			writer.Dispose();
			writer.Dispose();
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Empty(stream.CommittedBytes);
			Assert.Equal(0, stream.DisposeCalls);
		}

		[Fact]
		public void InjectedFlushFailure_DoesNotPadAndFaultsFlush()
		{
			InjectedStream stream = new InjectedStream();
			StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true);
			writer.WriteBits("101");
			writer.Flush();
			Assert.Equal(1, stream.FlushCalls);
			Assert.Empty(stream.CommittedBytes);

			stream.FailFlush = true;
			Assert.Throws<InjectedException>(() => writer.Flush());
			Assert.Equal(2, stream.FlushCalls);

			Assert.Throws<InvalidOperationException>(() => writer.Flush());
			Assert.Equal(2, stream.FlushCalls);
			Assert.Empty(stream.CommittedBytes);

			writer.Dispose();
			Assert.Empty(stream.CommittedBytes);
			Assert.Equal(0, stream.WriteAttempts);
		}

		[Fact]
		public void DisposePaddingFailure_DisposesOwnedStreamOnce()
		{
			InjectedStream stream = new InjectedStream();
			stream.FailOnWriteAttempt = 1;
			StreamBitWriter writer = new StreamBitWriter(stream);
			writer.WriteBits("101");

			Assert.Throws<InjectedException>(() => writer.Dispose());
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Equal(1, stream.DisposeCalls);
			Assert.False(stream.CanWrite);
			Assert.Empty(stream.CommittedBytes);

			writer.Dispose();
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Equal(1, stream.DisposeCalls);
		}

		[Fact]
		public void DisposePaddingAndCleanupFailure_RethrowsTheOriginal()
		{
			InjectedStream stream = new InjectedStream();
			stream.FailFlush = true;
			stream.FailDispose = true;
			StreamBitWriter writer = new StreamBitWriter(stream);
			writer.WriteBits("101");

			InjectedException thrown = Assert.Throws<InjectedException>(() => writer.Dispose());
			Assert.Equal("flush", thrown.Message);
			Assert.IsNotType<AggregateException>(thrown);

			object cleanup = thrown.Data[BitStream.DisposeExceptionDataKey];
			InjectedException disposeFailure = Assert.IsType<InjectedException>(cleanup);
			Assert.Equal("dispose", disposeFailure.Message);

			Assert.Equal(new byte[] { 0x05 }, stream.CommittedBytes);
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Equal(1, stream.DisposeCalls);

			writer.Dispose();
			Assert.Equal(1, stream.WriteAttempts);
			Assert.Equal(1, stream.DisposeCalls);
			Assert.False(writer.CanWrite);
			Assert.False(writer.CanRead);
			Assert.False(writer.CanSeek);
		}

		[Fact]
		public void InjectedReadFailure_FaultsTheReader()
		{
			InjectedStream stream = new InjectedStream(new byte[] { 0xFF, 0xFF });
			stream.FailRead = true;
			StreamBitReader reader = new StreamBitReader(stream, leaveOpen: true);

			InjectedException first = Assert.Throws<InjectedException>(() => reader.ReadBits(8));
			InvalidOperationException second = Assert.Throws<InvalidOperationException>(() => reader.ReadBits(1));
			Assert.Same(first, second.InnerException);
			Assert.Equal(0, reader.Position);

			stream.FailFlush = true;
			Assert.Throws<InvalidOperationException>(() => reader.Flush());
			Assert.Equal(0, stream.FlushCalls);

			InjectedStream slow = new InjectedStream(new byte[] { 0x05 });
			slow.FailRead = true;
			StreamBitReader bitReader = new StreamBitReader(slow, leaveOpen: true);
			Assert.Throws<InjectedException>(() => bitReader.ReadBits(1));
			Assert.Throws<InvalidOperationException>(() => bitReader.ReadBits(1));
		}
		#endregion

		#region precedence and lifetime
		[Fact]
		public void ExceptionPrecedence_FollowsTheContractOrder()
		{
			InjectedStream faultedSource = new InjectedStream();
			faultedSource.FailOnWriteAttempt = 1;
			StreamBitWriter faultedWriter = new StreamBitWriter(faultedSource, leaveOpen: true);
			Assert.Throws<InjectedException>(
				() =>
				{
					faultedWriter.WriteBits(new Code(new byte[] { 0x01 }));
				}
			);

			Assert.Throws<NotSupportedException>(() => faultedWriter.ReadBits(-1));
			Assert.Throws<NotSupportedException>(() => faultedWriter.ReadBits(1));
			Assert.Throws<ArgumentNullException>(() => faultedWriter.WriteBits(null!));
			Assert.Throws<NotSupportedException>(
				() =>
				{
					faultedWriter.Position = 0;
				}
			);
			Assert.Equal(0, faultedWriter.Position);

			faultedWriter.Dispose();
			ObjectDisposedException disposedWrite = Assert.Throws<ObjectDisposedException>(
				() =>
				{
					faultedWriter.WriteBits(null!);
				}
			);
			Assert.Equal(nameof(StreamBitWriter), disposedWrite.ObjectName);
			Assert.Throws<ObjectDisposedException>(() => faultedWriter.ReadBits(-1));
			Assert.Throws<ObjectDisposedException>(
				() =>
				{
					long ignored = faultedWriter.Position;
				}
			);
			Assert.Throws<ObjectDisposedException>(() => faultedWriter.AlignToNextByteBoundary());
			Assert.Throws<ObjectDisposedException>(() => faultedWriter.Flush());

			InjectedStream readerSource = new InjectedStream(new byte[] { 0xFF });
			readerSource.FailRead = true;
			StreamBitReader faultedReader = new StreamBitReader(readerSource, leaveOpen: true);
			Assert.Throws<InjectedException>(() => faultedReader.ReadBits(8));

			Assert.Throws<ArgumentOutOfRangeException>(() => faultedReader.ReadBits(-1));
			Assert.Throws<NotSupportedException>(() => faultedReader.WriteBits(null!));
			Assert.Throws<NotSupportedException>(
				() =>
				{
					faultedReader.Position = 4;
				}
			);
			Assert.True(faultedReader.Position >= 0);

			faultedReader.Dispose();
			ObjectDisposedException disposedRead = Assert.Throws<ObjectDisposedException>(
				() =>
				{
					faultedReader.ReadBits(-1);
				}
			);
			Assert.Equal(nameof(StreamBitReader), disposedRead.ObjectName);
			Assert.False(faultedReader.CanRead);
			Assert.False(faultedReader.CanWrite);
			Assert.False(faultedReader.CanSeek);
		}

		[Fact]
		public void LeaveOpen_KeepsOrDisposesTheUnderlyingStream()
		{
			MemoryStream kept = new MemoryStream();
			StreamBitWriter writer = new StreamBitWriter(kept, leaveOpen: true);
			writer.WriteBits("1");
			writer.Dispose();
			writer.Dispose();
			kept.WriteByte(0x7E);
			Assert.Equal(new byte[] { 0x01, 0x7E }, kept.ToArray());
			Assert.False(writer.CanWrite);

			MemoryStream owned = new MemoryStream();
			StreamBitWriter owningWriter = new StreamBitWriter(owned, leaveOpen: false);
			owningWriter.WriteBits("1");
			owningWriter.Dispose();
			owningWriter.Dispose();
			Assert.Throws<ObjectDisposedException>(() => owned.WriteByte(1));
			Assert.False(owningWriter.CanRead);
			Assert.False(owningWriter.CanWrite);
			Assert.False(owningWriter.CanSeek);

			MemoryStream keptReaderSource = new MemoryStream(new byte[] { 0x05 });
			StreamBitReader reader = new StreamBitReader(keptReaderSource, leaveOpen: true);
			Assert.Equal<Code>("101", reader.ReadBits(3));
			reader.Dispose();
			reader.Dispose();
			Assert.True(keptReaderSource.CanRead);
			Assert.Equal(1, keptReaderSource.Position);
			Assert.Equal(-1, keptReaderSource.ReadByte());

			MemoryStream ownedReaderSource = new MemoryStream(new byte[] { 0x05 });
			StreamBitReader owningReader = new StreamBitReader(ownedReaderSource, leaveOpen: false);
			owningReader.Dispose();
			Assert.Throws<ObjectDisposedException>(() => ownedReaderSource.ReadByte());
			ObjectDisposedException disposed = Assert.Throws<ObjectDisposedException>(() => owningReader.ReadBits(1));
			Assert.Equal(nameof(StreamBitReader), disposed.ObjectName);
		}

		[Fact]
		public void EarlyEndOfStream_ThrowsEndOfStreamException()
		{
			StreamBitReader reader = new StreamBitReader(new MemoryStream(new byte[] { 0xFF }));
			EndOfStreamException end = Assert.Throws<EndOfStreamException>(() => reader.ReadBits(9));
			InvalidOperationException faulted = Assert.Throws<InvalidOperationException>(() => reader.ReadBits(1));
			Assert.Same(end, faulted.InnerException);
			Assert.Equal(0, reader.Position);
		}
		#endregion

		#region helpers
		private static Code Pattern(int length, int pattern)
		{
			if (length == 0)
				return new Code(0);

			CodeBuilder builder = new CodeBuilder(length);
			for (int i = 0; i < length; ++i)
				builder[i] = (pattern >> i) & 1;

			return builder.Build();
		}

		private static Code Pattern(int length, Random random)
		{
			if (length == 0)
				return new Code(0);

			CodeBuilder builder = new CodeBuilder(length);
			for (int i = 0; i < length; ++i)
				builder[i] = random.Next(2);

			return builder.Build();
		}

		private static void AssertWriteMatchesModel(int prefixZeros, Code code)
		{
			ReferenceBitModel model = new ReferenceBitModel();
			if (prefixZeros > 0)
				model.WriteBits(new Code(prefixZeros));

			model.WriteBits(code);

			MemoryStream stream = new MemoryStream();
			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				if (prefixZeros > 0)
					writer.WriteBits(new Code(prefixZeros));

				writer.WriteBits(code);
				Assert.Equal(model.Length, writer.Position);
			}

			byte[] expected = model.ToByteArray();
			byte[] actual = stream.ToArray();
			Assert.Equal(expected, actual);

			using (StreamBitReader reader = new StreamBitReader(new MemoryStream(expected)))
			{
				int cursor = 0;
				if (prefixZeros > 0)
				{
					Assert.Equal(model.Read(cursor, prefixZeros), reader.ReadBits(prefixZeros));
					cursor += prefixZeros;
				}

				Assert.Equal(model.Read(cursor, code.Length), reader.ReadBits(code.Length));
			}
		}

		private static void AssertSequenceMatchesModel(Code first, bool align, bool flush, Code second)
		{
			ReferenceBitModel model = new ReferenceBitModel();
			MemoryStream stream = new MemoryStream();
			using (StreamBitWriter writer = new StreamBitWriter(stream, leaveOpen: true))
			{
				writer.WriteBits(first);
				model.WriteBits(first);
				if (align)
				{
					writer.AlignToNextByteBoundary();
					model.Align();
				}

				if (flush)
				{
					writer.Flush();
					writer.Flush();
					model.Flush();
				}

				writer.WriteBits(second);
				model.WriteBits(second);
				Assert.Equal(model.Length, writer.Position);
			}

			byte[] expected = model.ToByteArray();
			Assert.Equal(expected, stream.ToArray());

			using (StreamBitReader reader = new StreamBitReader(new MemoryStream(expected)))
			{
				Assert.Equal(model.Read(0, first.Length), reader.ReadBits(first.Length));
				if (align)
					reader.AlignToNextByteBoundary();

				if (flush)
				{
					reader.Flush();
					reader.Flush();
				}

				int cursor = first.Length;
				if (align && (cursor % 8) != 0)
					cursor += 8 - (cursor % 8);

				Assert.Equal(model.Read(cursor, second.Length), reader.ReadBits(second.Length));
				Assert.Equal(cursor + second.Length, reader.Position);
			}
		}
		#endregion

		#region reference model
		/// <summary>
		/// Least-significant-bit-first bit buffer. This is the oracle. It never calls
		/// <see cref="BitStream"/>.
		/// </summary>
		private sealed class ReferenceBitModel
		{
			#region fields
			private readonly List<int> _bits;
			#endregion

			#region constructors
			public ReferenceBitModel()
			{
				_bits = new List<int>();
			}
			#endregion

			#region properties
			public int Length
			{
				get
				{
					return _bits.Count;
				}
			}
			#endregion

			#region methods
			public void WriteBits(Code code)
			{
				for (int i = 0; i < code.Length; ++i)
					_bits.Add(code[i] == 1 ? 1 : 0);
			}

			public void Align()
			{
				int remainder = _bits.Count % 8;
				if (remainder == 0)
					return;

				int padding = 8 - remainder;
				for (int i = 0; i < padding; ++i)
					_bits.Add(0);
			}

			public void Flush()
			{
			}

			public byte[] ToByteArray()
			{
				int byteCount = (_bits.Count + 7) / 8;
				byte[] bytes = new byte[byteCount];
				for (int i = 0; i < _bits.Count; ++i)
				{
					if (_bits[i] != 0)
						bytes[i / 8] |= (byte)(1 << (i % 8));
				}

				return bytes;
			}

			public Code Read(int start, int bitCount)
			{
				if (bitCount == 0)
					return new Code(0);

				CodeBuilder builder = new CodeBuilder(bitCount);
				for (int i = 0; i < bitCount; ++i)
					builder[i] = _bits[start + i];

				return builder.Build();
			}
			#endregion
		}
		#endregion

		#region operation log
		private enum OperationKind
		{
			Write,
			Align,
			Flush
		}

		private struct Operation
		{
			public Operation(OperationKind kind, Code code)
			{
				Kind = kind;
				Code = code;
			}

			public OperationKind Kind { get; }
			public Code Code { get; }
		}
		#endregion

		#region injected stream
		private sealed class InjectedException : IOException
		{
			public InjectedException(string message)
				: base(message)
			{
			}
		}

		private sealed class InjectedStream : Stream
		{
			#region fields
			private readonly MemoryStream _inner;
			#endregion

			#region constructors
			public InjectedStream()
			{
				_inner = new MemoryStream();
			}

			public InjectedStream(byte[] bytes)
			{
				_inner = new MemoryStream();
				_inner.Write(bytes, 0, bytes.Length);
				_inner.Position = 0;
			}
			#endregion

			#region properties
			public int FailOnWriteAttempt { get; set; }
			public bool FailFlush { get; set; }
			public bool FailDispose { get; set; }
			public bool FailRead { get; set; }
			public int WriteAttempts { get; private set; }
			public int FlushCalls { get; private set; }
			public int DisposeCalls { get; private set; }
			public byte[] CommittedBytes
			{
				get
				{
					return _inner.ToArray();
				}
			}
			#endregion

			#region Stream properties
			public override bool CanRead => _inner.CanRead;
			public override bool CanSeek => _inner.CanSeek;
			public override bool CanWrite => _inner.CanWrite;
			public override long Length => _inner.Length;
			public override long Position
			{
				get => _inner.Position;
				set => _inner.Position = value;
			}
			#endregion

			#region Stream methods
			public override void Flush()
			{
				++FlushCalls;
				if (FailFlush)
					throw new InjectedException("flush");

				_inner.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (FailRead)
					throw new InjectedException("read");

				return _inner.Read(buffer, offset, count);
			}

			public override int ReadByte()
			{
				if (FailRead)
					throw new InjectedException("read");

				return _inner.ReadByte();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				return _inner.Seek(offset, origin);
			}

			public override void SetLength(long value)
			{
				_inner.SetLength(value);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				Commit(buffer, offset, count);
			}

			public override void WriteByte(byte value)
			{
				Commit(new byte[] { value }, 0, 1);
			}

			protected override void Dispose(bool disposing)
			{
				++DisposeCalls;
				if (FailDispose)
					throw new InjectedException("dispose");

				_inner.Dispose();
				base.Dispose(disposing);
			}
			#endregion

			#region private methods
			private void Commit(byte[] buffer, int offset, int count)
			{
				++WriteAttempts;
				if (FailOnWriteAttempt > 0 && WriteAttempts == FailOnWriteAttempt)
					throw new InjectedException("write");

				_inner.Write(buffer, offset, count);
			}
			#endregion
		}

		private sealed class ForwardOnlyStream : Stream
		{
			#region fields
			private readonly MemoryStream _inner;
			#endregion

			#region constructors
			public ForwardOnlyStream()
			{
				_inner = new MemoryStream();
			}
			#endregion

			#region Stream properties
			public override bool CanRead => false;
			public override bool CanSeek => false;
			public override bool CanWrite => true;
			public override long Length => throw new NotSupportedException();
			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}
			#endregion

			#region Stream methods
			public override void Flush()
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				_inner.Write(buffer, offset, count);
			}

			public override void WriteByte(byte value)
			{
				_inner.WriteByte(value);
			}

			public byte[] ToArray()
			{
				return _inner.ToArray();
			}
			#endregion
		}
		#endregion
	}
}
