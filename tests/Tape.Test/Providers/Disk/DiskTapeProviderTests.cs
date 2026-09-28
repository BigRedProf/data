using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;
using BigRedProf.Data.Tape.Providers.Disk;

namespace BigRedProf.Data.Tape.Test.Providers.Disk
{
	public class DiskTapeProviderTests : IDisposable
	{
		#region fields
		private readonly string _testDirectoryPath;
		private static readonly Guid TestTapeId = new Guid("00000000-0000-0000-0000-000000000001");
		private const int MaxContentLength = 1_000_000_000; // 1 billion bits
		private const int MaxContentBytes = MaxContentLength / 8;
		#endregion

		#region constructors
		public DiskTapeProviderTests()
		{
			_testDirectoryPath = Path.Combine(Path.GetTempPath(), "DiskTapeProviderTest");
			Directory.CreateDirectory(_testDirectoryPath);
		}

		public void Dispose()
		{
			if (Directory.Exists(_testDirectoryPath))
			{
				Directory.Delete(_testDirectoryPath, true);
			}
		}
		#endregion

		#region DiskTapeProvider methods
		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Read_FromNegativeOffset_ShouldThrow()
		{
			TapeProvider provider = new DiskTapeProvider(_testDirectoryPath);
			Assert.ThrowsAny<Exception>(() =>
			{
				provider.ReadTapeInternal(TestTapeId, -1, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Write_AtAnOffsetPastTheData_ShouldLandThere()
		{
			// The offset is on the tape, not in the data: a three-byte write at byte 1,000 is
			// ordinary, and was refused until the check stopped comparing the two.
			TapeProvider provider = new DiskTapeProvider(_testDirectoryPath);
			Tape.CreateNew(provider, TestTapeId);
			byte[] content = new byte[] { 0x0A, 0x0B, 0x0C };

			provider.WriteTapeInternal(TestTapeId, content, 1_000, content.Length);

			Assert.Equal(content, provider.ReadTapeInternal(TestTapeId, 1_000, content.Length));
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Write_PastTheEndOfTheTape_ShouldThrow()
		{
			TapeProvider provider = new DiskTapeProvider(_testDirectoryPath);
			Tape.CreateNew(provider, TestTapeId);

			Assert.Throws<ArgumentOutOfRangeException>(
				() => provider.WriteTapeInternal(TestTapeId, new byte[] { 1 }, MaxContentBytes, 1));
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void BackupAndRestore_OnDisk_ShouldRoundTripUnalignedCodesAcrossSessions()
		{
			// The whole path a backup takes, on disk: codes that are not whole bytes, written in
			// two sessions with a checkpoint between, read back exactly.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			PackRat<Code> codePackRat = piedPiper.GetPackRat<Code>(CoreSchema.Code);
			DiskLibrary library = new DiskLibrary(_testDirectoryPath);
			Guid seriesId = new Guid("dddddddd-0000-0000-0000-00000000000d");
			Code[] codes = new Code[] { "0", "00", "10101", "1100110011", "1" };

			BackupWizard first = BackupWizard.CreateNew(library, seriesId, "disk", "on disk");
			first.Append(piedPiper.PackModel(codes[0], CoreSchema.Code));
			first.Append(piedPiper.PackModel(codes[1], CoreSchema.Code));
			first.Append(piedPiper.PackModel(codes[2], CoreSchema.Code));
			first.SetLatestCheckpoint("01");
			BackupWizard second = BackupWizard.OpenExisting(library, seriesId);
			second.Append(piedPiper.PackModel(codes[3], CoreSchema.Code));
			second.Append(piedPiper.PackModel(codes[4], CoreSchema.Code));
			second.SetLatestCheckpoint("10");

			using (RestorationWizard restoration = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 0))
			{
				foreach (Code expected in codes)
					Assert.Equal(expected, codePackRat.UnpackModel(restoration.CodeReader));
			}
		}
		#endregion
	}
}
