using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape;
using System;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
    public class TapeRecorderTests : IDisposable
    {
        #region fields
        private readonly TapeProvider _memoryTapeProvider;
        private readonly TapeProvider _diskTapeProvider;
        private bool _disposed;
        #endregion

        #region constructors
        public TapeRecorderTests()
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
        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void Record_ShouldWriteDataToTape(TapeProvider tapeProvider)
        {
            // Arrange
            Guid tapeId = Guid.NewGuid();
            Tape tape = Tape.CreateNew(tapeProvider, tapeId);
            var tapeRecorder = new TapeRecorder();
            tapeRecorder.InsertTape(tape);

			// Act
			Code code = new Code("10101010 00000000 11111111 00001111 010");
            byte[] expectedBytes = new byte[] { 0b01010101, 0b00000000, 0b11111111, 0b11110000, 0b010 };
			tapeRecorder.Record(code);

            // Assert
            byte[] actualBytes = tapeProvider.ReadTapeInternal(tapeId, 0, 5);
			Assert.Equal(expectedBytes, actualBytes);
		}

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void Record_ShouldThrow_WhenContentIsNull(TapeProvider tapeProvider)
        {
            // Arrange
            Guid tapeId = Guid.NewGuid();
            Tape tape = Tape.CreateNew(tapeProvider, tapeId);
            var tapeRecorder = new TapeRecorder();
            tapeRecorder.InsertTape(tape);

            // Act & Assert
            Assert.ThrowsAny<ArgumentNullException>(() => tapeRecorder.Record(null!));
        }

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void Record_ShouldThrow_WhenTapeNotInserted(TapeProvider tapeProvider)
        {
            // Arrange
            var tapeRecorder = new TapeRecorder();
            var codeToWrite = new Code("10101010");

            // Act & Assert
            Assert.ThrowsAny<InvalidOperationException>(() => tapeRecorder.Record(codeToWrite));
        }

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void InsertTape_ShouldThrow_WhenTapeIsNull(TapeProvider tapeProvider)
        {
            // Arrange
            var tapeRecorder = new TapeRecorder();

            // Act & Assert
            Assert.ThrowsAny<ArgumentNullException>(() => tapeRecorder.InsertTape(null!));
        }

        [Trait("Region", "TapeRecorder methods")]
        [Theory]
        [MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
        public void InsertTape_ShouldThrow_WhenTapeAlreadyInserted(TapeProvider tapeProvider)
        {
            // Arrange
            Guid tapeId = Guid.NewGuid();
            Tape tape = Tape.CreateNew(tapeProvider, tapeId);
            var tapeRecorder = new TapeRecorder();
            tapeRecorder.InsertTape(tape);

            // Act & Assert
            Assert.ThrowsAny<InvalidOperationException>(() => tapeRecorder.InsertTape(tape));
        }
        #endregion
    }
}
