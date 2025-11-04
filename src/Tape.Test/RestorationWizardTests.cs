using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;

namespace BigRedProf.Data.Tape.Test
{
	public class RestorationWizardTests
	{
		private static TapeLibrary NewLibrary() => new MemoryLibrary();

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenLibraryIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
				RestorationWizard.OpenExistingTapeSeries(null!, Guid.NewGuid(), 0));
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenSeriesIdIsEmpty()
		{
			var lib = NewLibrary();
			Assert.Throws<ArgumentException>(() =>
				RestorationWizard.OpenExistingTapeSeries(lib, Guid.Empty, 0));
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenOffsetIsOutOfRange()
		{
			var library = NewLibrary();
			var seriesId = Guid.NewGuid();

			// Create a small series with 128 bits of data.
			var bw = BackupWizard.CreateNew(library, seriesId, "s", "d");
			var bytes = Enumerable.Range(0, 16).Select(i => (byte)i).ToArray(); // 16*8 = 128 bits
			using (var writer = bw.Writer)
				writer.WriteCode(new Code(bytes));
			bw.SetLatestCheckpoint(new Code("0"));

			// Negative
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				RestorationWizard.OpenExistingTapeSeries(library, seriesId, -1));

			// Beyond EOF (129 > 128)
			Assert.Throws<ArgumentOutOfRangeException>(() =>
				RestorationWizard.OpenExistingTapeSeries(library, seriesId, 129));
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldSupportBitLevelBookmark()
		{
			var library = NewLibrary();
			var seriesId = Guid.NewGuid();

			// Write a known 3-byte pattern so shifts are checkable.
			byte[] payload = new byte[] { 0b_1010_1100, 0b_0110_1001, 0b_1111_0000 }; // 0xAC, 0x69, 0xF0

			var bw = BackupWizard.CreateNew(library, seriesId, "series", "desc");
			using (var w = bw.Writer)
				w.WriteCode(new Code(payload));
			bw.SetLatestCheckpoint(new Code("0"));

			// Seek to 1 bit; read 16 bits; compare to the 2 expected bytes
			using (var rw = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 1))
			{
				var reader = rw.CodeReader;
				Code twoBytes = reader.Read(16);

				byte b0 = (byte)((payload[0] >> 1) | ((payload[1] & 0x01) << 7));
				byte b1 = (byte)((payload[1] >> 1) | ((payload[2] & 0x01) << 7));
				var expected = new Code(new byte[] { b0, b1 });

				Assert.Equal(expected, twoBytes);
				Assert.Equal(1, rw.Bookmark);
			}

			// Seek to 13 bits; read 16 bits.
			using (var rw2 = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 13))
			{
				var reader2 = rw2.CodeReader;
				Code twoBytes = reader2.Read(16);

				// Build expected by slicing the concatenated bitstream
				int acc = (payload[0]) | (payload[1] << 8) | (payload[2] << 16);
				int bitStart = 13;
				byte e0 = (byte)((acc >> bitStart) & 0xFF);
				byte e1 = (byte)((acc >> (bitStart + 8)) & 0xFF);
				var expected = new Code(new byte[] { e0, e1 });

				Assert.Equal(expected, twoBytes);
				Assert.Equal(13, rw2.Bookmark);
			}
		}
	}
}
