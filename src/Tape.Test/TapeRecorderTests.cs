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
			var expectedCode = new Code("10101010");
			tapeRecorder.Record(expectedCode);

            // Assert
            byte[] bytes = tapeProvider.ReadTapeInternal(tapeId, 0, expectedCode.Length);
            var actualCode = new Code(bytes, expectedCode.Length);
			Assert.Equal(expectedCode, actualCode);
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
