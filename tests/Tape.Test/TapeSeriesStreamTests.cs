using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape.Providers.Memory;

namespace BigRedProf.Data.Tape.Test
{
	public class TapeSeriesBitStreamTests
	{
		#region helpers
		private static Librarian CreateLibrarian()
		{
			return new Librarian(new MemoryTapeProvider());
		}

		private static byte[] ReadTapeBytes(Tape tape)
		{
			int bitLength = tape.Position;
			int byteLength = (bitLength + 7) / 8;
			if (byteLength == 0)
				return Array.Empty<byte>();

			return tape.TapeProvider.ReadTapeInternal(tape.Id, 0, byteLength);
		}
		#endregion

		#region constructor tests
		[Fact]
		public void Constructor_ShouldThrow_WhenLibrarianIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new TapeSeriesBitStream(null!, new Guid("10000000-0000-0000-0000-000000000001"), TapeSeriesBitStream.OpenMode.Read);
				}
			);
		}

		[Fact]
		public void Constructor_ShouldThrow_WhenSeriesIdIsEmpty()
		{
			Librarian librarian = CreateLibrarian();

			Assert.Throws<ArgumentException>(
				() =>
				{
					new TapeSeriesBitStream(librarian, Guid.Empty, TapeSeriesBitStream.OpenMode.Read);
				}
			);
		}

		[Fact]
		public void Constructor_ShouldThrow_WhenNoTapesInSeries()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000002");

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read);
				}
			);
		}
		#endregion

		#region append + readback basics
		[Fact]
		public void Append_ThenReadBack_ShouldRoundTrip()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000003");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Round-trip test");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("1");
				writer.WriteBits("010");
				writer.WriteBits(new Code(0, 1, 1, 1, 0, 0, 1, 0));
				writer.WriteBits(new Code(new byte[] { 0xF1 }));
				writer.WriteBits("001");
			}

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				Code a = reader.ReadBits(4);
				Code b = reader.ReadBits(8);
				Code c = reader.ReadBits(8);
				Code d = reader.ReadBits(3);

				Assert.Equal<Code>("1010", a);
				Assert.Equal<Code>("01110010", b);
				Assert.Equal<Code>("10001111", c);
				Assert.Equal<Code>("001", d);
			}
		}
		#endregion

		#region label position tracking
		[Fact]
		public void Append_ShouldAdvanceTapePosition_InBits()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000004");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Position test");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("101");
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(3, last.Position);

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("00101");
			}

			last = GetLastTape(librarian, seriesId);
			Assert.Equal(8, last.Position);
		}
		#endregion

		#region partial-byte packing
		[Fact]
		public void Read_ShouldPackPartialFinalByte_WhenTapeEndsMidByte()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000005");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Partial byte test");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("10110011");
				writer.WriteBits("001");
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(11, last.Position);

			// "10110011" is 0xCD. The final "001" is bit 2 of the next byte, 0x04.
			byte[] content = ReadTapeBytes(last);
			Assert.Equal(2, content.Length);
			Assert.Equal(0b11001101, content[0]);
			Assert.Equal(0b00000100, content[1]);

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				Assert.Equal<Code>("10110011", reader.ReadBits(8));
				Assert.Equal<Code>("001", reader.ReadBits(3));
			}
		}
		#endregion

		#region tape rollover regressions
		[Fact]
		public void Append_ShouldPreserveAllBits_WhenRollingOverToNewTape()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000006");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Rollover bug repro");

			Tape initialTape = GetLastTape(librarian, seriesId);
			TapeLabel initialLabel = initialTape.ReadLabel();
			initialTape.WriteLabel(initialLabel.WithTapePosition(Tape.MaxContentLength - 4));

			byte[] bytes = new byte[2];
			bytes[0] = 0b01010101;
			bytes[1] = 0b11110000;

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits(new Code(new byte[] { bytes[0] }));
				writer.WriteBits(new Code(new byte[] { bytes[1] }));
			}

			IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId);

			Assert.Equal(2, tapes.Count);

			Tape firstTape = tapes[0];
			Tape secondTape = tapes[1];

			Assert.Equal(Tape.MaxContentLength, firstTape.Position);
			Assert.Equal(12, secondTape.Position);
		}
		#endregion

		#region integration with AlignToNextByteBoundary
		[Fact]
		public void Append_WithAlignToNextByteBoundary_ShouldByteAlignBetweenWrites()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000007");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Align test");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("101");
				writer.AlignToNextByteBoundary();
				writer.WriteBits(new Code(new byte[] { 0xF1, 0xF1 }));
				writer.WriteBits("111");
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(27, last.Position);

			byte[] all = ReadTapeBytes(last);
			Assert.Equal(4, all.Length);
			Assert.Equal(0b00000101, all[0]);
			Assert.Equal(0xF1, all[1]);
			Assert.Equal(0xF1, all[2]);
			Assert.Equal(0b00000111, all[3]);
		}

		[Fact]
		public void AlignThenDispose_ShouldCommitMeaningfulBitsOnly()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000008");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Align dispose");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("101");
				writer.AlignToNextByteBoundary();
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(3, last.Position);
			byte[] content = ReadTapeBytes(last);
			Assert.Single(content);
			Assert.Equal(0b00000101, content[0]);
		}
		#endregion

		#region write/read large byte-aligned block
		[Fact]
		public void Append_AndReadBack_LargeByteAlignedBlock_ShouldMatch()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-000000000009");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Large block test");

			byte[] big = new byte[4096];
			for (int i = 0; i < big.Length; ++i)
				big[i] = (byte)(i % 251);

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.AlignToNextByteBoundary();
				writer.WriteBits(new Code(big));
			}

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				for (int i = 0; i < big.Length; ++i)
				{
					Code code = reader.ReadBits(8);
					Assert.Equal(big[i], code.ToByteArray()[0]);
				}
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(big.Length * 8, last.Position);
		}
		#endregion

		#region content length
		[Fact]
		public void Read_ShouldRespectContentLength_WhenTapePositionIsRewound()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-00000000000a");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Content length test");

			byte[] data = new byte[2];
			data[0] = 0xAA;
			data[1] = 0x55;

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.AlignToNextByteBoundary();
				writer.WriteBits(new Code(data));
			}

			Tape tape = GetLastTape(librarian, seriesId);
			TapeLabel label = tape.ReadLabel();
			tape.WriteLabel(label.WithTapePosition(0));

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				Assert.Equal<Code>(new Code(data), reader.ReadBits(16));
			}
		}
		#endregion

		#region empty tapes
		[Fact]
		public void Read_ShouldSkipAnEmptyMiddleTape()
		{
			MemoryTapeProvider provider = new MemoryTapeProvider();
			Librarian librarian = new Librarian(provider);
			Guid seriesId = new Guid("11111111-1111-1111-1111-111111111111");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "empty middle");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("11110000");
			}

			Tape empty = Tape.CreateNew(provider, new Guid("22222222-2222-2222-2222-222222222222"));
			empty.WriteLabel(
				empty.ReadLabel()
					.WithSeriesInfo(seriesId, "Test Series", 2)
					.WithName("Test Series")
					.WithSeriesDescription("empty middle")
			);

			Tape last = Tape.CreateNew(provider, new Guid("33333333-3333-3333-3333-333333333333"));
			last.WriteLabel(
				last.ReadLabel()
					.WithSeriesInfo(seriesId, "Test Series", 3)
					.WithName("Test Series")
					.WithSeriesDescription("empty middle")
			);

			using (TapeSeriesBitStream tail = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				tail.WriteBits("101");
			}

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				Assert.Equal<Code>("11110000", reader.ReadBits(8));
				Assert.Equal<Code>("101", reader.ReadBits(3));
				Assert.Throws<EndOfStreamException>(
					() =>
					{
						reader.ReadBits(1);
					}
				);
			}
		}
		#endregion

		#region read alignment failures
		[Fact]
		public void Align_WhenTheNextTapeLabelFails_FaultsTheReader()
		{
			// Ten bits on the first tape, opened at bit 6. One more bit leaves the wrapper at
			// position 1, so alignment still has to step onto the next tape and read its label.
			LabelFailingTapeProvider provider = new LabelFailingTapeProvider();
			Librarian librarian = new Librarian(provider);
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000001");

			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "align fault");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("1011001101");
			}

			Guid secondId = new Guid("aaaaaaaa-0000-0000-0000-000000000002");
			Tape second = Tape.CreateNew(provider, secondId);
			TapeLabel secondLabel = second.ReadLabel()
				.WithSeriesInfo(seriesId, "Test Series", 2)
				.WithName("Test Series")
				.WithSeriesDescription("align fault");
			second.WriteLabel(secondLabel);

			using (TapeSeriesBitStream secondWriter = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				secondWriter.WriteBits("11110000");
			}

			using (TapeSeriesBitStream reader = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read, 6))
			{
				reader.ReadBits(1);
				Assert.Equal(1, reader.Position);

				provider.FailTapeId = secondId;

				IOException failure = Assert.Throws<IOException>(
					() =>
					{
						reader.AlignToNextByteBoundary();
					}
				);
				Assert.Equal(1, provider.LabelReadFailures);
				Assert.Equal(1, reader.Position);

				InvalidOperationException faulted = Assert.Throws<InvalidOperationException>(
					() =>
					{
						reader.AlignToNextByteBoundary();
					}
				);
				Assert.Same(failure, faulted.InnerException);

				Assert.Throws<ArgumentOutOfRangeException>(
					() =>
					{
						reader.ReadBits(-1);
					}
				);
				Assert.Throws<NotSupportedException>(
					() =>
					{
						reader.WriteBits("1");
					}
				);
				Assert.Throws<InvalidOperationException>(
					() =>
					{
						reader.ReadBits(1);
					}
				);
				Assert.Throws<InvalidOperationException>(() => reader.Flush());
				Assert.Equal(1, reader.Position);

				reader.Dispose();
				ObjectDisposedException disposed = Assert.Throws<ObjectDisposedException>(
					() =>
					{
						reader.AlignToNextByteBoundary();
					}
				);
				Assert.Equal(nameof(TapeSeriesBitStream), disposed.ObjectName);
			}
		}
		#endregion

		#region negative capability tests
		[Fact]
		public void Read_ShouldThrow_WhenOpenedInAppendMode()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-00000000000b");
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "neg read");

			using (TapeSeriesBitStream stream = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				Assert.Throws<NotSupportedException>(
					() =>
					{
						stream.ReadBits(1);
					}
				);
			}
		}

		[Fact]
		public void Write_ShouldThrow_WhenOpenedInReadMode()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-00000000000c");
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "neg write");

			using (TapeSeriesBitStream stream = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Read))
			{
				Assert.Throws<NotSupportedException>(
					() =>
					{
						stream.WriteBits(new Code(new byte[] { 0x01 }));
					}
				);
			}
		}
		#endregion

		#region disposal tests
		[Fact]
		public void Dispose_ShouldPersistFinalPosition_InAppendMode()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = new Guid("10000000-0000-0000-0000-00000000000d");
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "dispose persist");

			using (TapeSeriesBitStream writer = new TapeSeriesBitStream(librarian, seriesId, TapeSeriesBitStream.OpenMode.Append))
			{
				writer.WriteBits("10110001");
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(8, last.Position);
		}
		#endregion

		#region private helpers
		private sealed class LabelFailingTapeProvider : MemoryTapeProvider
		{
			public Guid FailTapeId { get; set; }
			public int LabelReadFailures { get; private set; }

			public override byte[] ReadLabelInternal(Guid tapeId)
			{
				if (FailTapeId != Guid.Empty && tapeId == FailTapeId)
				{
					++LabelReadFailures;
					throw new IOException("injected label failure");
				}

				return base.ReadLabelInternal(tapeId);
			}
		}

		private static Tape GetLastTape(Librarian librarian, Guid seriesId)
		{
			Tape last = null!;
			foreach (Tape tape in librarian.FetchTapesInSeries(seriesId))
				last = tape;

			if (last == null)
				throw new InvalidOperationException("No tapes found in series.");

			return last;
		}
		#endregion
	}
}
