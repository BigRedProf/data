using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape;
using System;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
	public class TapePlayerTests : IDisposable
	{
		#region fields
		private readonly TapeProvider _memoryTapeProvider;
		private readonly TapeProvider _diskTapeProvider;
		private bool _disposed;
		#endregion

		#region constructors
		public TapePlayerTests()
		{
			_memoryTapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			_diskTapeProvider = TapeProviderHelper.CreateDiskTapeProvider();
		}
		#endregion

		#region IDisposable
		public void Dispose()
		{
			if (!_disposed)
			{
				TapeProviderHelper.DestroyDiskTapeProvider();
				_disposed = true;
			}
		}
		#endregion

		#region unit tests
		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldReturnWrittenData(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			Code expectedCode = new Code("10101010");
			byte[] bytes = new byte[] { 0b01010101 };
			tapeProvider.WriteTapeInternal(tapeId, bytes, 0, bytes.Length);

			// Act
			TapePlayer tapePlayer = new TapePlayer();
			tapePlayer.InsertTape(tape);
			Code actualCode = tapePlayer.Play(expectedCode.Length);

			// Assert
			Assert.Equal(expectedCode, actualCode);
		}

		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldThrow_WhenTapeNotInserted(TapeProvider tapeProvider)
		{
			// Arrange
			var tapePlayer = new TapePlayer();

			// Act & Assert
			Assert.ThrowsAny<InvalidOperationException>(() => tapePlayer.Play(1));
		}

		[Trait("Region", "TapePlayer methods")]
		[Theory]
		[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
		public void Play_ShouldThrow_WhenLengthIsNegative(TapeProvider tapeProvider)
		{
			// Arrange
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			var tapePlayer = new TapePlayer();
			tapePlayer.InsertTape(tape);

			// Act & Assert
			Assert.ThrowsAny<ArgumentException>(() => tapePlayer.Play(-1));
		}
		#endregion
	}
}
