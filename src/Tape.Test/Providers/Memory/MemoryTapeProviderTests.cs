using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape.Providers.Memory;

namespace BigRedProf.Data.Tape.Test.Providers.Memory
{
	public class MemoryTapeProviderTests
	{
		#region MemoryTapeProvider methods
		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Read_FromNegativeOffset_ShouldThrow()
		{
			// Arrange
			var provider = new MemoryTapeProvider();

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Read(-1, 1);
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Read_FromPastEnd_ShouldThrow()
		{
			// Arrange
			var provider = new MemoryTapeProvider();

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Read(TapeProvider.MaxContentLength, 1); // Attempt to read from very end
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Read_PastEnd_ShouldThrow()
		{
			// Arrange
			var provider = new MemoryTapeProvider();

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Read(1, TapeProvider.MaxContentLength); // Attempt to read past the end
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Write_PastEnd_ShouldThrow()
		{
			// Arrange
			var provider = new MemoryTapeProvider();
			var content = new Code("1"); // 1 bit

			// Act & Assert
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.Write(content, TapeProvider.MaxContentLength); // Attempt to write past the end
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Aligned_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			var content = new Code("10101010"); // 8 bits
			int offset = 0;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Aligned2_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			var content = new Code("10101010 11011011 10110101 11101100 10100101 01110010"); // 8 bits
			int offset = 32343 * 8;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_Unaligned_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			Code content = new Code("110"); // 3 bits
			int offset = 5;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_UnalignedAcrossByteBoundary_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			Code content = new Code("11011101");
			int offset = 5;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_AtEnd_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			Code content = new Code("11111111"); // 8 bits
			int offset = TapeProvider.MaxContentLength - 8;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
		}
		#endregion
	}
}
