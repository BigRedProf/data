using BigRedProf.Data.Core;
using BigRedProf.Data.Tape._TestHelpers;

namespace BigRedProf.Data.Tape.Test
{
	public class LibrarianTests : IDisposable
	{
		#region fields
			private readonly TapeProvider _memoryTapeProvider;
			private readonly TapeProvider _diskTapeProvider;
			private bool _disposed;
		#endregion

		#region constructors
			public LibrarianTests()
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
			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void Constructor_ShouldThrow_WhenPiedPiperIsNull(TapeProvider tapeProvider)
			{
				Assert.ThrowsAny<ArgumentNullException>(() =>
				{
					var librarian = new Librarian(null!, tapeProvider);
				});
			}

			[Trait("Region", "Librarian methods")]
			[Fact]
			public void Constructor_ShouldThrow_WhenTapeProviderIsNull()
			{
				Assert.ThrowsAny<ArgumentNullException>(() =>
				{
					var librarian = new Librarian((TapeProvider)null!);
				});

				IPiedPiper piedPiper = TapeProviderHelper.CreatePiedPiper();
				Assert.ThrowsAny<ArgumentNullException>(() =>
				{
					var librarian = new Librarian(piedPiper, null!);
				});
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchTape_ShouldThrow_WhenTapeIdIsEmptyGuid(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Assert.ThrowsAny<ArgumentException>(() =>
				{
					librarian.FetchTape(Guid.Empty);
				});
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchTape_ShouldThrow_WhenTapeDoesNotExist(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Assert.ThrowsAny<Exception>(() =>
				{
					librarian.FetchTape(new Guid("00000000-4343-0000-0000-000000000001"));
				});
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchTape_ShouldThrow_WhenTapeIdIsEmptyGuid_Negative(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Assert.ThrowsAny<ArgumentException>(() =>
				{
					librarian.FetchTape(Guid.Empty);
				});
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchTape_ShouldSucceed_WhenTapeDoesExist(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Guid tapeId = new Guid("00000000-4343-0000-0000-000000000001");
				Tape tape = Tape.CreateNew(tapeProvider, tapeId);
				TapeLabel tapeLabel = new TapeLabel()
				.WithTapeId(tapeId);
				tape.WriteLabel(tapeLabel);
				librarian.AddTape(tape);

				Tape fetchedTape = librarian.FetchTape(tapeId);

				Assert.Equal(tapeId, fetchedTape.Id);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchTapesInSeries_ShouldThrow_WhenSeriesIdIsEmptyGuid(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Assert.ThrowsAny<ArgumentException>(() =>
				{
					librarian.FetchTapesInSeries(Guid.Empty).ToList();
				});
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchAllTapes_ShouldSucceed_WithNoTapes(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);

				IList<Tape> tapes = librarian.FetchAllTapes().ToList();

				Assert.Empty(tapes);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchAllTapes_ShouldSucceed_WithOneTape(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Guid tapeId = new Guid("00000000-4343-0000-0000-000000000001");
				Tape tape = Tape.CreateNew(tapeProvider, tapeId);
				librarian.AddTape(tape);

				IList<Tape> tapes = librarian.FetchAllTapes().ToList();

				Assert.Single(tapes);
				Assert.Equal(tapeId, tapes[0].Id);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchAllTapes_ShouldSucceed_WithTwoTapes(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Guid tape1Id = new Guid("00000000-4343-0000-0000-000000000001");
				Tape tape1 = Tape.CreateNew(tapeProvider, tape1Id);
				librarian.AddTape(tape1);
				Guid tape2Id = new Guid("00000000-4343-0000-0000-000000000002");
				Tape tape2 = Tape.CreateNew(tapeProvider, tape2Id);
				librarian.AddTape(tape2);

				IList<Tape> tapes = librarian.FetchAllTapes().ToList();

				Assert.Equal(2, tapes.Count);
				Assert.Contains(tapes, t => t.Id == tape1Id);
				Assert.Contains(tapes, t => t.Id == tape2Id);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchAllTapesInSeries_ShouldSucceed_WithNoTapesInSeries(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Guid seriesId = new Guid("00000000-4343-0000-0000-0000053c1351");
				Guid tape1Id = new Guid("00000000-4343-0000-0000-000000000001");
				Tape tape1 = Tape.CreateNew(tapeProvider, tape1Id);
				librarian.AddTape(tape1);

				IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId).ToList();

				Assert.Empty(tapes);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void FetchAllTapesInSeries_ShouldSucceed_WithOneTapesInSeries(TapeProvider tapeProvider)
			{
				Librarian librarian = new Librarian(tapeProvider);
				Guid seriesId = new Guid("00000000-4343-0000-0000-0000053c1351");
				Guid tape1Id = new Guid("00000000-4343-0000-0000-000000000001");
				Tape tape1 = Tape.CreateNew(tapeProvider, tape1Id);
				librarian.AddTape(tape1);
				Guid tape2Id = new Guid("00000000-4343-0000-0000-000000000002");
				Tape tape2 = Tape.CreateNew(tapeProvider, tape2Id);
				TapeLabel tape2Label = tape2.ReadLabel()
				.WithTapeId(tape2Id)
				.WithSeriesInfo(seriesId, "Test Series", 1);
				tape2.WriteLabel(tape2Label);
				librarian.AddTape(tape2);
				Guid tape3Id = new Guid("00000000-4343-0000-0000-000000000003");
				Tape tape3 = Tape.CreateNew(tapeProvider, tape3Id);
				librarian.AddTape(tape3);

				//IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId).ToList();
				IEnumerable<Tape> tapesEnum = librarian.FetchTapesInSeries(seriesId);
				IList<Tape> tapes = tapesEnum.ToList();

				Assert.Single(tapes);
				Assert.Equal(tape2Id, tapes[0].Id);
				TapeLabel fetchedLabel = tapes[0].ReadLabel();
				Assert.Equal(seriesId, fetchedLabel.SeriesId);
				Assert.Equal("Test Series", fetchedLabel.SeriesName);
				Assert.Equal(1, fetchedLabel.SeriesNumber);
			}

			[Trait("Region", "Librarian methods")]
			[Theory]
			[MemberData(nameof(TapeProviderHelper.TapeProviders), MemberType = typeof(TapeProviderHelper))]
			public void AddTape_ShouldThrow_WhenTapeIsNull(TapeProvider tapeProvider)
			{
				Assert.ThrowsAny<ArgumentNullException>(() =>
				{
					Librarian librarian = new Librarian(_memoryTapeProvider);
					librarian.AddTape(null!);
				});
			}
		#endregion
	}
}
