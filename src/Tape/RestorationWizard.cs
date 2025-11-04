using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using System;
using System.IO;

public sealed class RestorationWizard : IDisposable
{
	private readonly Librarian _librarian;
	private readonly Guid _seriesId;
	private TapeSeriesStream _seriesStream;
	private CodeReader _codeReader;

	private RestorationWizard(
		Librarian librarian,
		Guid seriesId,
		string seriesName,
		string seriesDescription,
		TapeSeriesStream seriesStream,
		CodeReader codeReader,
		long bookmarkBits)
	{
		_librarian = librarian ?? throw new ArgumentNullException(nameof(librarian));
		if (seriesId == Guid.Empty) throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
		_seriesId = seriesId;
		_seriesStream = seriesStream ?? throw new ArgumentNullException(nameof(seriesStream));
		_codeReader = codeReader ?? throw new ArgumentNullException(nameof(codeReader));
		Bookmark = bookmarkBits;
		SeriesName = seriesName ?? string.Empty;
		SeriesDescription = seriesDescription ?? string.Empty;
	}

	public CodeReader CodeReader => _codeReader;
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

		Tape first = null;
		foreach (var t in tapes) { if (t != null) { first = t; break; } }
		if (first == null) throw new InvalidOperationException("No tapes found in the requested series.");
		var firstLabel = first.ReadLabel();
		string seriesName = firstLabel.SeriesName ?? string.Empty;
		string seriesDesc;
		if (!firstLabel.TryGetSeriesDescription(out seriesDesc)) seriesDesc = string.Empty;

		var stream = new TapeSeriesStream(librarian, seriesId, TapeSeriesStream.OpenMode.Read);

		// This Seek interprets offset as BITS
		stream.Seek(offsetBits, SeekOrigin.Begin);

		var reader = new CodeReader(stream);
		return new RestorationWizard(librarian, seriesId, seriesName, seriesDesc, stream, reader, offsetBits);
	}

	public void Dispose()
	{
		_codeReader?.Dispose();
		_seriesStream?.Dispose();
		_codeReader = null;
		_seriesStream = null;
	}
}
