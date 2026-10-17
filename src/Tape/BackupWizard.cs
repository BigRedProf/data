using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// Orchestrates writing content to a tape series and maintains series metadata.
	/// Design note: the "latest" client checkpoint is stored on the highest-numbered
	/// (latest) tape in the series. Older tapes are left intact and may retain their
	/// historical checkpoints; they are not rewritten when advancing to new tapes.
	/// </summary>
	public class BackupWizard
	{
		#region static fields
		private const MultihashAlgorithm SeriesDigestAlgorithm = MultihashAlgorithm.Sha256;
		private static readonly ISeriesDigestEngine DefaultDigestEngine = new SeriesDigestEngine(SeriesDigestAlgorithm);
		#endregion

		#region fields
		private readonly Librarian _librarian;
		private readonly Guid _seriesId;
		private readonly string _seriesName;
		private readonly string _seriesDescription;
		private readonly ISeriesDigestEngine _digestEngine;

		private Tape _currentTape;
		private int _currentSeriesNumber;
		private Multihash _seriesParentDigest;
		private Multihash _currentSeriesHeadDigest;
		private Multihash _currentTapeContentDigest;

		private TapeSeriesBitStream? _writer;
		#endregion

		#region constructors
		private BackupWizard(
			Librarian librarian,
			Guid seriesId,
			Tape currentTape,
			string seriesName,
			string seriesDescription,
			ISeriesDigestEngine digestEngine,
			int currentSeriesNumber,
			Multihash seriesParentDigest,
			Multihash currentSeriesHeadDigest,
			Multihash currentTapeContentDigest)
		{
			_librarian = librarian ?? throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
			_currentTape = currentTape ?? throw new ArgumentNullException(nameof(currentTape));
			_seriesId = seriesId;
			_seriesName = seriesName ?? throw new ArgumentNullException(nameof(seriesName));
			_seriesDescription = seriesDescription ?? string.Empty;
			Debug.Assert(digestEngine != null);
			_digestEngine = digestEngine;
			_currentSeriesNumber = currentSeriesNumber;
			_seriesParentDigest = seriesParentDigest ?? throw new ArgumentNullException(nameof(seriesParentDigest));
			_currentSeriesHeadDigest = currentSeriesHeadDigest ?? throw new ArgumentNullException(nameof(currentSeriesHeadDigest));
			_currentTapeContentDigest = currentTapeContentDigest ?? throw new ArgumentNullException(nameof(currentTapeContentDigest));
		}
		#endregion

		#region properties
		/// <summary>Gets a bit stream for appending data to the series.</summary>
		public BitStream Writer
		{
			get
			{
				return EnsureWriter();
			}
		}
		#endregion

		#region functions
		public static BackupWizard CreateNew(TapeLibrary library, Guid seriesId, string seriesName, string seriesDescription, ISeriesDigestEngine? digestEngine = null)
		{
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			return CreateNew(library.Librarian, seriesId, seriesName, seriesDescription, digestEngine);
		}

		public static BackupWizard CreateNew(Librarian librarian, Guid seriesId, string seriesName, string seriesDescription, ISeriesDigestEngine? digestEngine = null)
		{
			if (librarian == null)
				throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
			if (string.IsNullOrWhiteSpace(seriesName))
				throw new ArgumentException("Series name cannot be null or whitespace.", nameof(seriesName));

			string actualDescription = seriesDescription ?? string.Empty;
			ISeriesDigestEngine engine = ResolveDigestEngine(digestEngine);
			Multihash baselineDigest = engine.ComputeBaseline();
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(librarian.TapeProvider, tapeId);

			TapeLabel label = tape.ReadLabel();
			label = label.WithSeriesInfo(seriesId, seriesName, 1)
				.WithName(seriesName)
				.WithSeriesDescription(actualDescription)
				.WithContentMultihash(baselineDigest)
				.WithSeriesParentMultihash(baselineDigest)
				.WithSeriesHeadMultihash(baselineDigest);
			tape.WriteLabel(label);

			return new BackupWizard(
				librarian,
				seriesId,
				tape,
				seriesName,
				actualDescription,
				engine,
				1,
				baselineDigest,
				baselineDigest,
				baselineDigest);
		}

		public static BackupWizard OpenExisting(TapeLibrary library, Guid seriesId, ISeriesDigestEngine? digestEngine = null)
		{
			if (library == null)
				throw new ArgumentNullException(nameof(library));

			return OpenExisting(library.Librarian, seriesId, digestEngine);
		}

		public static BackupWizard OpenExisting(Librarian librarian, Guid seriesId, ISeriesDigestEngine? digestEngine = null)
		{
			if (librarian == null)
				throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));

			ISeriesDigestEngine engine = ResolveDigestEngine(digestEngine);
			Multihash baselineDigest = engine.ComputeBaseline();

			IEnumerable<Tape> tapes = librarian.FetchTapesInSeries(seriesId);
			Tape latestTape = SelectLatestTape(tapes);
			TapeLabel label = latestTape.ReadLabel();

			string seriesName = label.SeriesName;
			string? seriesDescription;
			if (!label.TryGetSeriesDescription(out seriesDescription))
				seriesDescription = string.Empty;

			Multihash? parentDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesParentDigest, out parentDigest))
				parentDigest = baselineDigest;

			Multihash? headDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesHeadDigest, out headDigest))
				headDigest = parentDigest;

			Multihash? contentDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.ContentDigest, out contentDigest))
				contentDigest = baselineDigest;

			int seriesNumber = label.SeriesNumber;

			return new BackupWizard(
				librarian,
				seriesId,
				latestTape,
				seriesName,
				seriesDescription,
				engine,
				seriesNumber,
				parentDigest,
				headDigest,
				contentDigest);
		}
		#endregion

		#region methods
		/// <summary>Appends code to the current tape series.</summary>
		public void Append(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));
			Writer.WriteBits(code);
		}

		public Code GetLatestCheckpoint()
		{
			TapeLabel label = _currentTape.ReadLabel();
			if (!label.TryGetClientCheckpoint(out Code? checkpoint))
				throw new InvalidOperationException("No checkpoint has been recorded for the current tape.");
			return checkpoint;
		}

		/// <summary>
		/// Sets the latest client checkpoint for the series.
		/// Disposes the current writer to persist any partial-byte state before labeling.
		/// </summary>
		public void SetLatestCheckpoint(Code clientCheckpointCode)
		{
			if (clientCheckpointCode == null)
				throw new ArgumentNullException(nameof(clientCheckpointCode));

			FlushWriterStream();
			EnsureWriterDisposed();

			TapeLabel label = _currentTape.ReadLabel();
			label = ApplySeriesMetadata(label);

			Multihash contentDigest = _digestEngine.ComputeContentDigest(_currentTape);
			Multihash seriesHeadDigest = _digestEngine.ComputeSeriesHeadDigest(_seriesParentDigest, contentDigest);
			
			label = label
				.WithContentMultihash(contentDigest)
				.WithSeriesParentMultihash(_seriesParentDigest)
				.WithSeriesHeadMultihash(seriesHeadDigest)
				.WithClientCheckpoint(clientCheckpointCode);
			_currentTape.WriteLabel(label);

			_currentTapeContentDigest = contentDigest;
			_currentSeriesHeadDigest = seriesHeadDigest;
		}
		#endregion

		#region internals
		private static ISeriesDigestEngine ResolveDigestEngine(ISeriesDigestEngine? digestEngine)
		{
			if (digestEngine != null)
				return digestEngine;

			return DefaultDigestEngine;
		}

		private TapeLabel ApplySeriesMetadata(TapeLabel label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label));

			return label
				.WithSeriesInfo(_seriesId, _seriesName, _currentSeriesNumber)
				.WithName(_seriesName)
				.WithSeriesDescription(_seriesDescription);
		}

		private void FlushWriterStream()
		{
			_writer?.Flush();
		}

		private void EnsureWriterDisposed()
		{
			_writer?.Dispose();
		}

		private TapeSeriesBitStream EnsureWriter()
		{
			if (_writer != null)
				return _writer;

			TapeSeriesBitStream stream = new TapeSeriesBitStream(_librarian, _seriesId, TapeSeriesBitStream.OpenMode.Append);
			// Subscribe to tape changes and reactively finalize/seed labels.
			stream.CurrentTapeChanged += OnCurrentTapeChanged;
			stream.Closed += OnWriterClosed;

			_writer = stream;
			return stream;
		}

		private void OnCurrentTapeChanged(object? sender, TapeSeriesBitStream.RolloverEventArgs e)
		{
			// e.FinishedTape might be null if this is the very first tape creation from ctor,
			// but in practice rollover only fires after we had a current tape.
			if (e.FinishedTape != null)
			{
				// Finalize finished tape's digests
				Multihash finishedContent = _digestEngine.ComputeContentDigest(e.FinishedTape);
				Multihash finishedHead = _digestEngine.ComputeSeriesHeadDigest(_seriesParentDigest, finishedContent);

				TapeLabel finishedLabel = e.FinishedTape.ReadLabel();
				finishedLabel = ApplySeriesMetadata(finishedLabel)
					.WithContentMultihash(finishedContent)
					.WithSeriesParentMultihash(_seriesParentDigest)
					.WithSeriesHeadMultihash(finishedHead);
				e.FinishedTape.WriteLabel(finishedLabel);

				// Advance chain state for the new tape
				_seriesParentDigest = finishedContent;
				_currentSeriesHeadDigest = finishedHead;
			}

			// Switch to new tape and seed its label
			_currentTape = e.NewTape;
			TapeLabel newLabel = e.NewTape.ReadLabel();
			_currentSeriesNumber = newLabel.SeriesNumber;

			Multihash baselineDigest = _digestEngine.ComputeBaseline();
			TapeLabel seeded = ApplySeriesMetadata(newLabel)
				.WithContentMultihash(baselineDigest)
				.WithSeriesParentMultihash(_seriesParentDigest)
				.WithSeriesHeadMultihash(_currentSeriesHeadDigest);
			_currentTape.WriteLabel(seeded);

			_currentTapeContentDigest = baselineDigest;
		}

		private void OnWriterClosed(object? sender, EventArgs e)
		{
			if (_writer != null)
			{
				_writer.CurrentTapeChanged -= OnCurrentTapeChanged;
				_writer.Closed -= OnWriterClosed;
			}

			_writer = null;
		}

		private static Tape SelectLatestTape(IEnumerable<Tape> tapes)
		{
			if (tapes == null)
				throw new ArgumentNullException(nameof(tapes));

			Tape? latestTape = null;
			int highestSeriesNumber = 0;

			foreach (Tape tape in tapes)
			{
				if (tape == null)
					continue;

				TapeLabel label = tape.ReadLabel();
				if (!label.TryGetTrait<int>(CoreTrait.SeriesNumber, out int seriesNumber))
					continue;

				if (latestTape == null || seriesNumber > highestSeriesNumber)
				{
					latestTape = tape;
					highestSeriesNumber = seriesNumber;
				}
			}

			if (latestTape == null)
				throw new InvalidOperationException("No tapes found in the requested series.");

			return latestTape;
		}
		#endregion

	}
}
