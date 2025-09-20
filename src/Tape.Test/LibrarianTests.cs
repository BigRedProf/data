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
			Tape tape = new Tape(tapeProvider);
			FlexModel label = new FlexModel();
			label.AddTrait(new Trait<Guid>(CoreTrait.Id, tapeId));
			librarian.AddTape(tape);

			Tape fetchedTape = librarian.FetchTape(tapeId);
			
			FlexModel fetchedLabel = fetchedTape.ReadLabel();
			Assert.True(fetchedLabel.TryGetTrait<Guid>(CoreTrait.Id, out Guid fetchedTapeId));
			Assert.Equal(tapeId, fetchedTapeId);
		}
		#endregion
	}
}
