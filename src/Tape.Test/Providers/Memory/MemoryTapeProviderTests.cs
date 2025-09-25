using System;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape.Providers.Memory;
using Xunit;

namespace BigRedProf.Data.Tape.Test.Providers.Memory
{
	public class MemoryTapeProviderTests
	{
		#region MemoryTapeProvider methods
		private static readonly Guid TestTapeId = new Guid("00000000-4343-0000-0000-000000000001");
		private const int MaxContentLength = 1_000_000_000; // 1 billion bits
		private const int MaxContentBytes = MaxContentLength / 8;

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Read_FromNegativeOffset_ShouldThrow()
		{
			var provider = new MemoryTapeProvider();
			provider.AddTapeInternal(Tape.CreateNew(provider, TestTapeId));
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				provider.ReadTapeInternal(TestTapeId, -1, 1);
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Read_PastEnd_ShouldThrow()
		{
			var provider = new MemoryTapeProvider();
			Assert.Throws<ArgumentException>(() =>
			{
				provider.ReadTapeInternal(Guid.Empty, 0, 1);
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void Write_PastEnd_ShouldThrow()
		{
			var provider = new MemoryTapeProvider();
			byte[] data = new byte[1];
			Assert.Throws<ArgumentException>(() =>
			{
				provider.WriteTapeInternal(Guid.Empty, data, 0, 1);
			});
		}

		[Trait("Region", "MemoryTapeProvider methods")]
		[Fact]
		public void WriteAndReadRoundTrip_ShouldWork()
		{
			var provider = new MemoryTapeProvider();
			byte[] data = new byte[] { 0xAB, 0xCD, 0xEF };
			TapeProviderHelper.TestWriteAndReadRoundTrip(provider, TestTapeId, data, 0);
		}
		#endregion
	}
}
