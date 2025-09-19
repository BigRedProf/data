using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;
using BigRedProf.Data.Tape.Providers.Memory;

namespace BigRedProf.Data.Tape.Test
{
	public class LibrarianTests
	{
		#region Librarian methods
		[Trait("Region", "Librarian methods")]
		[Fact]
		public void FetchTape_ShouldThrow_WhenTapeIdIsEmptyGuid()
		{
			IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
			TapeProvider tapeProvider = new MemoryTapeProvider();
			Librarian librarian = new Librarian(piedPiper, tapeProvider);
			Assert.ThrowsAny<ArgumentException>(() =>
			{
				librarian.FetchTape(Guid.Empty);
			});
		}

		[Trait("Region", "Librarian methods")]
		[Fact]
		public void FetchTape_ShouldThrow_WhenTapeDoesNotExist()
		{
			IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
			TapeProvider tapeProvider = new MemoryTapeProvider();
			Librarian librarian = new Librarian(piedPiper, tapeProvider);
			Assert.ThrowsAny<Exception>(() =>
			{
				librarian.FetchTape(new Guid("00000000-4343-0000-0000-000000000001"));
			});
		}
		#endregion
	}
}
