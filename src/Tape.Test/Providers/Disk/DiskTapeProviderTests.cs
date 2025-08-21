using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Disk;
using Xunit;
using System;
using System.IO;

namespace BigRedProf.Data.Tape.Test.Providers.Disk
{
	public class DiskTapeProviderTests : IDisposable
	{
		#region fields
		private readonly string _testFilePath;
		private static readonly Guid TestTapeId = Guid.NewGuid();
		private const int MaxContentLength = 1_000_000_000; // 1 billion bits
		private const int MaxContentBytes = MaxContentLength / 8;
		#endregion

		#region constructors
		public DiskTapeProviderTests()
		{
			_testFilePath = Path.Combine(Path.GetTempPath(), "test.tape");
			// Ensure file exists for tests that expect it
			if (!File.Exists(_testFilePath))
			{
				using (var fs = File.Create(_testFilePath))
				{
					fs.SetLength(MaxContentBytes);
				}
			}
		}

		public void Dispose()
		{
			if (File.Exists(_testFilePath))
				File.Delete(_testFilePath);
		}
		#endregion

		#region DiskTapeProvider methods
		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Read_FromNegativeOffset_ShouldThrow()
		{
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Assert.ThrowsAny<Exception>(() =>
			{
				provider.ReadInternal(TestTapeId, -1, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndRead_AtEnd_ShouldWork()
		{
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			byte[] data = new byte[] { 0xFF };
			int offset = MaxContentBytes - 1;
			provider.WriteInternal(TestTapeId, data, offset, 1);
			var read = provider.ReadInternal(TestTapeId, offset, 1);
			Assert.Equal(data, read);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_ShouldWork()
		{
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			byte[] data = new byte[] { 0xAB, 0xCD, 0xEF };
			provider.WriteInternal(TestTapeId, data, 0, data.Length);
			var read = provider.ReadInternal(TestTapeId, 0, data.Length);
			Assert.Equal(data, read);
		}
		#endregion
	}
}
