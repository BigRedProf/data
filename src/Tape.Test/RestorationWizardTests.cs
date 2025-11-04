using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;
using System;
using System.Linq;
using Xunit;

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

			// Create exactly 128 bits of data (16 * "10110011" = 128 bits).
			string bits128 = string.Concat(Enumerable.Repeat("10110011", 16)); // 128 bits

			var bw = BackupWizard.CreateNew(library, seriesId, "s", "d");
			using (var writer = bw.Writer)
				writer.WriteCode(bits128);
			bw.SetLatestCheckpoint("0");

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

			// 24-bit payload (easy to visualize): 10101100 01101001 11110000
			// Keep it as one contiguous bit string.
			string payload = "101011000110100111110000"; // 24 bits

			var bw = BackupWizard.CreateNew(library, seriesId, "series", "desc");
			using (var w = bw.Writer)
				w.WriteCode(payload);
			bw.SetLatestCheckpoint("0");

			// Seek to bit 1 (0-based), read 16 bits
			using (var rw = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 1))
			{
				var reader = rw.CodeReader;
				Code sixteen = reader.Read(16);
				string expected = payload.Substring(1, 16);

				Assert.Equal((Code)expected, sixteen);
				Assert.Equal(1, rw.Bookmark);
			}

			// Seek to bit 13, read 8 bits
			using (var rw2 = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 13))
			{
				var reader2 = rw2.CodeReader;
				Code eight = reader2.Read(8);
				string expected = payload.Substring(13, 8);

				Assert.Equal((Code)expected, eight);
				Assert.Equal(13, rw2.Bookmark);
			}
		}
	}
}
