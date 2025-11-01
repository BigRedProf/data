using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using BigRedProf.Data.Core.Internal;
using System.IO;
using System;
using System.Collections.Generic;

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
		private const MultihashAlgorithm DefaultSeriesDigestAlgorithm = MultihashAlgorithm.SHA2_256;
		#endregion

		#region fields
		private readonly ISeriesDigestEngine _digests;
		private readonly Librarian _librarian;
		private readonly Guid _seriesId;
		private readonly string _seriesName;
		private readonly string _seriesDescription;

		private Tape _currentTape;
		private int _currentSeriesNumber;
		private Multihash _seriesParentDigest;
		private Multihash _currentSeriesHeadDigest;
		private Multihash _currentTapeContentDigest;
		private bool _isTapeContentDirty;

		private TapeSeriesStream? _seriesStream;
		private CodeWriter? _codeWriter;
		#endregion

		#region constructors
		private BackupWizard(
			Librarian librarian,
			Guid seriesId,
			ISeriesDigestEngine digests,
			Tape currentTape,
			string seriesName,
			string seriesDescription,
			int currentSeriesNumber,
			Multihash seriesParentDigest,
			Multihash currentSeriesHeadDigest,
			Multihash currentTapeContentDigest)
		{
			_librarian = librarian ?? throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));
			_digests = digests ?? throw new ArgumentNullException(nameof(digests));
			_currentTape = currentTape ?? throw new ArgumentNullException(nameof(currentTape));
			_seriesId = seriesId;
			_seriesName = seriesName ?? throw new ArgumentNullException(nameof(seriesName));
			_seriesDescription = seriesDescription ?? string.Empty;
			_currentSeriesNumber = currentSeriesNumber;
			_seriesParentDigest = seriesParentDigest ?? throw new ArgumentNullException(nameof(seriesParentDigest));
			_currentSeriesHeadDigest = currentSeriesHeadDigest ?? throw new ArgumentNullException(nameof(currentSeriesHeadDigest));
			_currentTapeContentDigest = currentTapeContentDigest ?? throw new ArgumentNullException(nameof(currentTapeContentDigest));
			_isTapeContentDirty = false;
		}
		#endregion

		#region properties
		/// <summary>Gets a code writer for appending data to the series.</summary>
		public CodeWriter Writer => EnsureCodeWriter();
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
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(librarian.TapeProvider, tapeId);

			ISeriesDigestEngine engine = digestEngine ?? new SeriesDigestEngine(DefaultSeriesDigestAlgorithm);
			Multihash baselineDigest = engine.ComputeBaseline();

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
				engine,
				tape,
				seriesName,
				actualDescription,
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

			IEnumerable<Tape> tapes = librarian.FetchTapesInSeries(seriesId);
			Tape latestTape = SelectLatestTape(tapes);
			TapeLabel label = latestTape.ReadLabel();

			ISeriesDigestEngine engine = digestEngine ?? new SeriesDigestEngine(DefaultSeriesDigestAlgorithm);
			Multihash baselineDigest = engine.ComputeBaseline();

			string seriesName = label.SeriesName;
			string seriesDescription;
			if (!label.TryGetSeriesDescription(out seriesDescription))
				seriesDescription = string.Empty;

			Multihash parentDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesParentDigest, out parentDigest))
				parentDigest = baselineDigest;

			Multihash headDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesHeadDigest, out headDigest))
				headDigest = parentDigest;

			Multihash contentDigest;
			if (!label.TryGetTrait<Multihash>(CoreTrait.ContentDigest, out contentDigest))
				contentDigest = baselineDigest;

			int seriesNumber = label.SeriesNumber;

			return new BackupWizard(
				librarian,
				seriesId,
				engine,
				latestTape,
				seriesName,
				seriesDescription,
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
			Writer.WriteCode(code);
		}

		public Code GetLatestCheckpoint()
		{
			TapeLabel label = _currentTape.ReadLabel();
			if (!label.TryGetClientCheckpoint(out Code checkpoint))
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

			Multihash contentDigest = _isTapeContentDirty
				? _digests.ComputeContentDigest(_currentTape)
				: _currentTapeContentDigest;

			Multihash seriesHeadDigest = _isTapeContentDirty
				? _digests.ComputeSeriesHeadDigest(_seriesParentDigest, contentDigest)
				: _currentSeriesHeadDigest;

			label = label
				.WithContentMultihash(contentDigest)
				.WithSeriesParentMultihash(_seriesParentDigest)
				.WithSeriesHeadMultihash(seriesHeadDigest)
				.WithClientCheckpoint(clientCheckpointCode);
			_currentTape.WriteLabel(label);

			_currentTapeContentDigest = contentDigest;
			_currentSeriesHeadDigest = seriesHeadDigest;
			_isTapeContentDirty = false;
		}
		#endregion

		#region internals
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
			_seriesStream?.Flush();
		}

		private void EnsureWriterDisposed()
		{
			_codeWriter?.Dispose();
		}

		private CodeWriter EnsureCodeWriter()
		{
			if (_codeWriter != null)
				return _codeWriter;

			TapeSeriesStream stream = new TapeSeriesStream(_librarian, _seriesId, TapeSeriesStream.OpenMode.Append);
			// Subscribe to tape changes and reactively finalize/seed labels.
			stream.CurrentTapeChanged += OnCurrentTapeChanged;

			DirtyTrackingStream trackingStream = new DirtyTrackingStream(this, stream);
			WizardCodeWriter writer = new WizardCodeWriter(this, trackingStream);
			_seriesStream = stream;
			_codeWriter = writer;
			return writer;
		}

		private void OnCurrentTapeChanged(object? sender, TapeSeriesStream.RolloverEventArgs e)
		{
			// e.FinishedTape might be null if this is the very first tape creation from ctor,
			// but in practice rollover only fires after we had a current tape.
			if (e.FinishedTape != null)
			{
				Multihash finishedContent = _digests.ComputeContentDigest(e.FinishedTape);
				Multihash finishedHead = _digests.ComputeSeriesHeadDigest(_seriesParentDigest, finishedContent);
				.WithContentMultihash(_digests.ComputeBaseline())

			_currentTapeContentDigest = _digests.ComputeBaseline();
		private void OnWriterDisposed()
		{
			if (_seriesStream != null)
			{
				_seriesStream.CurrentTapeChanged -= OnCurrentTapeChanged;
			}
			_codeWriter = null;
			_seriesStream = null;
		}

		private static Multihash ComputeContentDigest(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape));

			int contentLengthBits = tape.Position;
			if (contentLengthBits <= 0)
				return BaselineSeriesDigest;

			Code content = TapeHelper.ReadContent(tape, 0, contentLengthBits);
			return Multihash.FromCode(content, SeriesDigestAlgorithm);
		}

		private static Multihash ComputeSeriesHeadDigest(Multihash parentDigest, Multihash contentDigest)
		{
			if (parentDigest == null)
				throw new ArgumentNullException(nameof(parentDigest));
			if (contentDigest == null)
				throw new ArgumentNullException(nameof(contentDigest));

			if (parentDigest.Algorithm != contentDigest.Algorithm)
				throw new InvalidOperationException("Digest algorithms must match to compute the series head digest.");

			byte[] parentBytes = parentDigest.Digest;
			byte[] contentBytes = contentDigest.Digest;
			byte[] combined = new byte[parentBytes.Length + contentBytes.Length];
			Array.Copy(parentBytes, 0, combined, 0, parentBytes.Length);
			Array.Copy(contentBytes, 0, combined, parentBytes.Length, contentBytes.Length);
			Code combinedCode = new Code(combined);
			return Multihash.FromCode(combinedCode, contentDigest.Algorithm);
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

		#region private classes
		private sealed class DirtyTrackingStream : Stream, IBitAwareStream
		{
			private readonly BackupWizard _owner;
			private readonly TapeSeriesStream _inner;

			public DirtyTrackingStream(BackupWizard owner, TapeSeriesStream inner)
			{
				_owner = owner ?? throw new ArgumentNullException(nameof(owner));
				_inner = inner ?? throw new ArgumentNullException(nameof(inner));
			}

			public override bool CanRead => _inner.CanRead;
			public override bool CanSeek => _inner.CanSeek;
			public override bool CanWrite => _inner.CanWrite;
			public override long Length => _inner.Length;
			public override long Position
			{
				get => _inner.Position;
				set => _inner.Position = value;
			}

			public override void Flush() => _inner.Flush();
			public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
			public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
			public override void SetLength(long value) => _inner.SetLength(value);

			public override void Write(byte[] buffer, int offset, int count)
			{
				_inner.Write(buffer, offset, count);
				if (count > 0)
					_owner.MarkTapeContentDirty();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_inner.Dispose();
				base.Dispose(disposing);
			}

			public byte CurrentByte
			{
				get => ((IBitAwareStream)_inner).CurrentByte;
				set => ((IBitAwareStream)_inner).CurrentByte = value;
			}

			public int OffsetIntoCurrentByte
			{
				get => ((IBitAwareStream)_inner).OffsetIntoCurrentByte;
				set => ((IBitAwareStream)_inner).OffsetIntoCurrentByte = value;
			}
		}

		private sealed class WizardCodeWriter : CodeWriter
		{
			private readonly BackupWizard _owner;
			private bool _isDisposed;

			public WizardCodeWriter(BackupWizard owner, Stream stream)
				: base(stream)
			{
				_owner = owner ?? throw new ArgumentNullException(nameof(owner));
				_isDisposed = false;
			}

			protected override void Dispose(bool disposing)
			{
				if (_isDisposed)
				{
					base.Dispose(disposing);
					return;
				}

				try
				{
					base.Dispose(disposing);
				}
				finally
				{
					_owner.OnWriterDisposed();
					_isDisposed = true;
				}
			}
		}
		#endregion
	}
}
