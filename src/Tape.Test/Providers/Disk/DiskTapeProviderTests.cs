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
		#endregion
	}
}
