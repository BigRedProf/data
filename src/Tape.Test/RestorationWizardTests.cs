using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;
using System;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
	public class RestorationWizardTests
	{
		private static TapeLibrary NewLibrary() { return new MemoryLibrary(); }

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenLibraryIsNull()
		{
			Assert.Throws<ArgumentNullException>(() =>
				RestorationWizard.OpenExistingTapeSeries((TapeLibrary)null, Guid.Parse("11111111-1111-1111-1111-111111111111"), 0));
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenSeriesIdIsEmpty()
		{
			TapeLibrary lib = NewLibrary();
			Assert.Throws<ArgumentException>(() =>
				RestorationWizard.OpenExistingTapeSeries(lib, Guid.Empty, 0));
		}

		[Trait("Region", "RestorationWizard functions")]
		[Fact]
		public void OpenExistingTapeSeries_ShouldThrow_WhenOffsetIsOutOfRange()
		{
			TapeLibrary library = NewLibrary();
			Guid seriesId = Guid.Parse("22222222-2222-2222-2222-222222222222");

			// Create exactly 128 bits of data (16 * "10110011" = 128 bits).
			Code bits128 = string.Concat(Enumerable.Repeat("10110011", 16)); // 128 bits

			BackupWizard bw = BackupWizard.CreateNew(library, seriesId, "s", "d");
			using (CodeWriter writer = bw.Writer)
			{
				writer.WriteCode(bits128);
			}
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
			TapeLibrary library = NewLibrary();
			Guid seriesId = Guid.Parse("33333333-3333-3333-3333-333333333333");

			// 24-bit payload (easy to visualize): 10101100 01101001 11110000
			// Keep it as one contiguous bit string.
			Code payload = "101011000110100111110000"; // 24 bits

			BackupWizard bw = BackupWizard.CreateNew(library, seriesId, "series", "desc");
			using (CodeWriter w = bw.Writer)
			{
				w.WriteCode(payload);
			}
			bw.SetLatestCheckpoint("0");

			// Seek to bit 1 (0-based), read 16 bits
			using (RestorationWizard rw = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 1))
			{
				CodeReader reader = rw.CodeReader;
				Code sixteen = reader.Read(16);
				Code expected = "0101100011010011";

				Assert.Equal(expected, sixteen);
				Assert.Equal(1, rw.Bookmark);
			}

			// Seek to bit 13, read 8 bits
			using (RestorationWizard rw2 = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 13))
			{
				CodeReader reader2 = rw2.CodeReader;
				Code eight = reader2.Read(8);
				Code expected = "00111110";

				Assert.Equal(expected, eight);
				Assert.Equal(13, rw2.Bookmark);
			}
		}
	}
}
