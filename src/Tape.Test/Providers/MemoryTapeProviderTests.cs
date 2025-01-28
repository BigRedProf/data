using BigRedProf.Data.Tape.Providers;
using System;
using Xunit;

namespace BigRedProf.Data.Tape.Test.TapeProviders
{
	public class MemoryTapeProviderTests
	{
		#region MemoryTapeProvider methods
		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadBasicCode()
		{
			// Arrange
			var provider = new MemoryTapeProvider();
			var content = new Code("10101010"); // 8 bits

			// Act
			provider.Write(content, 0);
			var result = provider.Read(8, 0);

			// Assert
			Assert.Equal(content, result);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadPartialBits()
		{
			// Arrange
			var provider = new MemoryTapeProvider();
			var content = new Code("110"); // 3 bits

			// Act
			provider.Write(content, 5); // Write at offset 5
			var result = provider.Read(3, 5);

			// Assert
			Assert.Equal(content, result);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadAtBoundary()
		{
			// Arrange
			var provider = new MemoryTapeProvider();
			var content = new Code("11111111"); // 8 bits
			var lastOffset = TapeProvider.MaxContentLength - 8;

			// Act
			provider.Write(content, lastOffset); // Write at the last valid position
			var result = provider.Read(8, lastOffset);

			// Assert
			Assert.Equal(content, result);
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteBeyondBoundaryThrowsException()
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
		public void ReadBeyondBoundaryThrowsException()
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
