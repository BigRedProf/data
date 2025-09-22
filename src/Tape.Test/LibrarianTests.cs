using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;

namespace BigRedProf.Data.Tape.Test
{
	public class LibrarianTests
	{
		#region static methods
		public static IEnumerable<object[]> TapeProviders()
		{
			yield return new object[] { TapeProviderHelper.CreateMemoryTapeProvider() };
			yield return new object[] { TapeProviderHelper.CreateDiskTapeProvider("TestTapes") };
		} 
		#endregion

		#region Librarian methods
		[Trait("Region", "Librarian methods")]
		[Theory]
		[MemberData(nameof(TapeProviders))]
		public void FetchTape_ShouldThrow_WhenTapeIdIsEmptyGuid(TapeProvider tapeProvider)
		{
			IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
			Librarian librarian = new Librarian(piedPiper, tapeProvider);
			Assert.ThrowsAny<ArgumentException>(() =>
			{
				librarian.FetchTape(Guid.Empty);
			});
		}

		[Trait("Region", "Librarian methods")]
		[Theory]
		[MemberData(nameof(TapeProviders))]
		public void FetchTape_ShouldThrow_WhenTapeDoesNotExist(TapeProvider tapeProvider)
		{
			IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
			Librarian librarian = new Librarian(piedPiper, tapeProvider);
			Assert.ThrowsAny<Exception>(() =>
			{
				librarian.FetchTape(new Guid("00000000-4343-0000-0000-000000000001"));
			});
		}

		[Trait("Region", "Librarian methods")]
		[Theory]
		[MemberData(nameof(TapeProviders))]
		public void FetchTape_ShouldSucceed_WhenTapeDoesExist(TapeProvider tapeProvider)
		{
			IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
			Librarian librarian = new Librarian(piedPiper, tapeProvider);
			Guid tapeId = new Guid("00000000-4343-0000-0000-000000000001");
			Tape tape = new Tape(tapeProvider, tapeId);
			TapeLabel tapeLabel = new TapeLabel()
				.WithTapeId(tapeId);
			tape.WriteLabel(tapeLabel);
			librarian.AddTape(tape);

			Tape fetchedTape = librarian.FetchTape(tapeId);
			
			Assert.Equal(tapeId, fetchedTape.Id);
		}
		#endregion
	}
}
