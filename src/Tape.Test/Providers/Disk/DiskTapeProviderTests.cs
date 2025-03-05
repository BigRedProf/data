using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape.Providers.Disk;

namespace BigRedProf.Data.Tape.Test.Providers.Disk
{
	public class DiskTapeProviderTests : IDisposable
	{
		#region fields
		private readonly string _testFilePath;
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
				provider.Read(-1, 1);
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Read_FromPastEnd_ShouldThrow()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Read(TapeProvider.MaxContentLength, 1); // Attempt to read from very end
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Read_PastEnd_ShouldThrow()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Read(1, TapeProvider.MaxContentLength); // Attempt to read past the end
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void Write_PastEnd_ShouldThrow()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("1"); // 1 bit

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Write(content, TapeProvider.MaxContentLength); // Attempt to write past the end
			});
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Aligned_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("10101010"); // 8 bits
			int offset = 0;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Aligned2_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("10101010 11011011 10110101 11101100 10100101 01110010"); // 8 bits
			int offset = 32343 * 8;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Unaligned_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("110"); // 3 bits
			int offset = 5;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_UnalignedAcrossByteBoundary_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("11011101");
			int offset = 5;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "DiskTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_AtEnd_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new DiskTapeProvider(_testFilePath);
			Code content = new Code("11111111"); // 8 bits
			int offset = TapeProvider.MaxContentLength - 8;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}
		#endregion
	}
}
