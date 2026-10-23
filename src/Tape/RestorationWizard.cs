using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using System;

public sealed class RestorationWizard : IDisposable
{
	private readonly Librarian _librarian;
	private readonly Guid _seriesId;
	private readonly TapeSeriesBitStream _reader;

	private RestorationWizard(
		Librarian librarian,
		Guid seriesId,
		string seriesName,
		string seriesDescription,
		TapeSeriesBitStream reader,
		long bookmarkBits)
	{
		_librarian = librarian ?? throw new ArgumentNullException(nameof(librarian));
		if (seriesId == Guid.Empty) throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
		_seriesId = seriesId;
		_reader = reader ?? throw new ArgumentNullException(nameof(reader));
		Bookmark = bookmarkBits;
		SeriesName = seriesName ?? string.Empty;
		SeriesDescription = seriesDescription ?? string.Empty;
	}

	public BitStream Reader => _reader;
	public long Bookmark { get; private set; }
	public string SeriesName { get; }
	public string SeriesDescription { get; }

	public static RestorationWizard OpenExistingTapeSeries(TapeLibrary library, Guid seriesId, long offsetBits)
	{
		if (library == null) throw new ArgumentNullException(nameof(library));
		if (seriesId == Guid.Empty) throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
		if (offsetBits < 0) throw new ArgumentOutOfRangeException(nameof(offsetBits));

		Librarian librarian = library.Librarian;

		// Best-effort metadata
		var tapes = librarian.FetchTapesInSeries(seriesId);
		if (tapes == null) throw new InvalidOperationException("No tapes found in the requested series.");

		Tape? first = null;
		foreach (var t in tapes) { if (t != null) { first = t; break; } }
		if (first == null) throw new InvalidOperationException("No tapes found in the requested series.");
		var firstLabel = first.ReadLabel();
		string seriesName = firstLabel.SeriesName ?? string.Empty;
		string? seriesDesc;
		if (!firstLabel.TryGetSeriesDescription(out seriesDesc)) seriesDesc = string.Empty;

		TapeSeriesBitStream reader = new TapeSeriesBitStream(
			librarian,
			seriesId,
			TapeSeriesBitStream.OpenMode.Read,
			offsetBits
		);
		return new RestorationWizard(librarian, seriesId, seriesName, seriesDesc, reader, offsetBits);
	}

	public void Dispose()
	{
		_reader.Dispose();
	}
}
