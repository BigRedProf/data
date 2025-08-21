using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Disk;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Tape.Test.Providers.Disk
{
	public class DiskTapeProviderTests : IDisposable
	{
		#region fields
		private readonly string _testFilePath;
		private static readonly Guid TestTapeId = Guid.NewGuid();
		private const int MaxContentLength = 1_000_000_000; // 1 billion bits
		#endregion

		#region constructors
		public DiskTapeProviderTests()
		{
			_testFilePath = Path.Combine(Path.GetTempPath(), "test.tape");
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
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.ReadInternal(TestTapeId, -1, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Read_PastEnd_ShouldThrow()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);

			// Act & Assert
			Assert.Throws<ArgumentException>(() =>
			{
				provider.ReadInternal(Guid.Empty, 0, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Write_PastEnd_ShouldThrow()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			byte[] data = new byte[1];
			// Act & Assert
			Assert.Throws<ArgumentException>(() =>
			{
				provider.WriteInternal(Guid.Empty, data, 0, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			byte[] data = new byte[] { 0xAB, 0xCD, 0xEF };

			// Act
			provider.WriteInternal(TestTapeId, data, 0, data.Length);
			var read = provider.ReadInternal(TestTapeId, 0, data.Length);

			// Assert
			Assert.Equal(data, read);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndRead_AtEnd_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			byte[] data = new byte[] { 0xFF };
			int offset = (MaxContentLength / 8) - 1;

			// Act
			provider.WriteInternal(TestTapeId, data, offset, 1);
			var read = provider.ReadInternal(TestTapeId, offset, 1);

			// Assert
			Assert.Equal(data, read);
		}
		#endregion
	}
}
