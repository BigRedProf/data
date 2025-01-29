using BigRedProf.Data.Tape.Providers;
using BigRedProf.Data.Tape._TestHelpers;
using System;
using Xunit;

namespace BigRedProf.Data.Tape.Test.TapeProviders
{
	public class MemoryTapeProviderTests
	{
		#region MemoryTapeProvider methods
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
		public void WriteAndReadRoundTrip_AtEnd_ShouldWork()
		{
			// Arrange
			TapeProvider provider = new MemoryTapeProvider();
			Code content = new Code("11111111"); // 8 bits
			int offset = TapeProvider.MaxContentLength - 8;

			// Act & Assert
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, content, offset);
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
		#endregion
	}
}
