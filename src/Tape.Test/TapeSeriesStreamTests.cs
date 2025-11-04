using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Memory;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace BigRedProf.Data.Tape.Test
{
	public class TapeSeriesStreamTests
	{
		#region helpers
		private static Librarian CreateLibrarian()
		{
			// Adjust this to however you normally create a Librarian backed by an in-memory TapeProvider.
			// For example, if you have an InMemoryTapeProvider:
			//   return new Librarian(new InMemoryTapeProvider());
			//
			// If your code uses a TapeLibrary wrapper, you can expose .Librarian from there.
			return new Librarian(new MemoryTapeProvider());
		}

		private static Guid NewSeriesId()
		{
			return Guid.NewGuid();
		}
		#endregion

		#region constructor tests
		[Fact]
		public void Constructor_ShouldThrow_WhenLibrarianIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new TapeSeriesStream(null!, Guid.NewGuid(), TapeSeriesStream.OpenMode.Read);
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
					new TapeSeriesStream(librarian, Guid.Empty, TapeSeriesStream.OpenMode.Read);
				}
			);
		}

		[Fact]
		public void Constructor_ShouldThrow_WhenNoTapesInSeries()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			// No tapes created yet for this seriesId
			Assert.Throws<InvalidOperationException>(
				() =>
				{
					new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read);
				}
			);
		}
		#endregion

		#region append + readback basics
		[Fact]
		public void Append_ThenReadBack_WithCodeReaderWriter_ShouldRoundTrip()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			// Start a new series/tape with proper labels/metadata
			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Round-trip test");

			// Append a mixed sequence:
			//   1) "1"                  (1 bit)
			//   2) "010"                (+3 bits)  => total so far 4 bits (fills low nibble of first byte)
			//   3) 8-bit code 01110010  (+8 bits)  => fills the rest of byte0 and all of byte1 per LSB-first
			//   4) 0xF1                 (+8 bits)  => bits LSB-first: 10001111
			//   5) "001"                (+3 bits)
			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode("1");                                        // 1
				writer.WriteCode("010");                                      // +3 => 4
				writer.WriteCode(new Code(0, 1, 1, 1, 0, 0, 1, 0));           // +8 => 12
				writer.WriteCode(new Code(new byte[] { 0xF1 }));              // +8 => 20
				writer.WriteCode("001");                                      // +3 => 23
			}

			using (TapeSeriesStream readStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			using (CodeReader reader = new CodeReader(readStream))
			{
				// Read back in the same chunk sizes, crossing byte boundaries.
				// LSB-first within each byte:
				//   - After first 4 bits: "1010"
				//   - Next 8 bits (positions 4..11) are the 8-bit code: "01110010"
				//   - Next 8 bits (positions 12..19) are 0xF1 LSB-first: "10001111"
				//   - Final 3 bits: "001"
				Code a = reader.Read(4);
				Code b = reader.Read(8);
				Code c = reader.Read(8);
				Code d = reader.Read(3);

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
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Position test");

			// Append 3 bits
			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode("101");
			}

			// Verify last tape's Position is 3
			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(3, last.Position);

			// Append 5 more bits (to complete a byte)
			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode("00101");
			}

			last = GetLastTape(librarian, seriesId);
			Assert.Equal(8, last.Position); // 3 + 5 = 8 bits
		}
		#endregion

		#region partial-byte packing
		[Fact]
		public void Read_ShouldPackPartialFinalByte_WhenTapeEndsMidByte()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Partial byte test");

			// Write 11 bits total. The read-stream when asked for bytes will need to pack the final 3 bits.
			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				// 8 bits (one byte), then +3 bits
				writer.WriteCode("10110011");
				writer.WriteCode("001");
			}

			// Consume via Stream.Read(byte[]...) to ensure the stream packs the last partial byte
			using (TapeSeriesStream readStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			{
				byte[] buffer = new byte[3];
				int n = readStream.Read(buffer, 0, buffer.Length);

				// We expect 2 bytes returned (LSB-first within each byte):
				//   First byte: bits "10110011" map to b0..b7 => 11001101 (0xCD = 205)
				//   Second byte: final 3 bits "001" => 0b00000001 (1)				
				Assert.Equal(2, n);
				Assert.Equal(0b11001101, buffer[0]);
				Assert.Equal(0b00000100, buffer[1]);
			}
		}
		#endregion

		#region tape rollover regressions
		[Fact]
		public void Append_ShouldPreserveAllBits_WhenRollingOverToNewTape()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Rollover bug repro");

			Tape initialTape = GetLastTape(librarian, seriesId);
			TapeLabel initialLabel = initialTape.ReadLabel();
			initialLabel.AddTrait(new Trait<int>(TapeTrait.TapePosition, Tape.MaxContentLength - 4));
			initialTape.WriteLabel(initialLabel);

			byte[] bytes = new byte[2];
			bytes[0] = 0b01010101;
			bytes[1] = 0b11110000;

			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode(new Code(new byte[] { bytes[0] }));
				writer.WriteCode(new Code(new byte[] { bytes[1] }));
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
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Align test");

			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode("101");                   // 3 bits -> partial byte
				writer.AlignToNextByteBoundary();          // finish the current byte (should write 00000101)
				writer.WriteCode(new Code(new byte[] { 0xF1, 0xF1 })); // 16 bits
				writer.WriteCode("111");                   // +3 bits (tail)
			}

			using (TapeSeriesStream readStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			{
				byte[] all = new byte[4];
				int n = readStream.Read(all, 0, all.Length);

				// Expect:
				//   Byte 0: 00000101
				//   Byte 1: 0xF1
				//   Byte 2: 0xF1
				//   Byte 3: packed from "111" -> 0b00000111
				Assert.Equal(4, n);
				Assert.Equal(0b00000101, all[0]);
				Assert.Equal(0xF1, all[1]);
				Assert.Equal(0xF1, all[2]);
				Assert.Equal(0b00000111, all[3]);
			}
		}
		#endregion

		#region write/read large byte-aligned block
		[Fact]
		public void Append_AndReadBack_LargeByteAlignedBlock_ShouldMatch()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Large block test");

			byte[] big = new byte[4096];
			for (int i = 0; i < big.Length; ++i)
				big[i] = (byte)(i % 251);

			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.AlignToNextByteBoundary();
				writer.WriteCode(new Code(big));
			}

			using (TapeSeriesStream readStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			{
				byte[] buffer = new byte[big.Length];
				int read = 0;
				while (read < buffer.Length)
				{
					int n = readStream.Read(buffer, read, buffer.Length - read);
					if (n == 0)
						break;
					read += n;
				}

				Assert.Equal(big.Length, read);
				for (int i = 0; i < big.Length; ++i)
					Assert.Equal(big[i], buffer[i]);
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(big.Length * 8, last.Position);
		}
		#endregion

		#region x
		[Fact]
		public void Read_ShouldRespectContentLength_WhenTapePositionIsRewound()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, "Test Series", "Content length test");

			byte[] data = new byte[2];
			data[0] = 0xAA;
			data[1] = 0x55;

			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.AlignToNextByteBoundary();
				writer.WriteCode(new Code(data));
			}

			Tape tape = GetLastTape(librarian, seriesId);
			TapeLabel label = tape.ReadLabel();
			label.AddTrait(new Trait<int>(TapeTrait.TapePosition, 0));
			tape.WriteLabel(label);

			using (TapeSeriesStream readStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			{
				byte[] buffer = new byte[data.Length];
				int read = readStream.Read(buffer, 0, buffer.Length);

				Assert.Equal(data.Length, read);
				for (int i = 0; i < data.Length; ++i)
				{
					Assert.Equal(data[i], buffer[i]);
				}
			}
		}
		#endregion

		#region negative capability tests
		[Fact]
		public void Read_ShouldThrow_WhenOpenedInAppendMode()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "neg read");

			using (TapeSeriesStream stream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			{
				Assert.Throws<NotSupportedException>(
					() =>
					{
						byte[] buffer = new byte[1];
						stream.Read(buffer, 0, 1);
					}
				);
			}
		}

		[Fact]
		public void Write_ShouldThrow_WhenOpenedInReadMode()
		{
			Librarian librarian = CreateLibrarian();
			Guid seriesId = NewSeriesId();
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "neg write");

			using (TapeSeriesStream stream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read))
			{
				Assert.Throws<NotSupportedException>(
					() =>
					{
						byte[] buffer = new byte[1] { 0x01 };
						stream.Write(buffer, 0, 1);
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
			Guid seriesId = NewSeriesId();
			BackupWizard.CreateNew(librarian, seriesId, "Test Series", "dispose persist");

			using (TapeSeriesStream writeStream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Append))
			using (CodeWriter writer = new CodeWriter(writeStream))
			{
				writer.WriteCode("10110001");
				// Dispose() of writer/stream should ensure position is persisted
			}

			Tape last = GetLastTape(librarian, seriesId);
			Assert.Equal(8, last.Position);
		}
		#endregion

		#region private helpers
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
