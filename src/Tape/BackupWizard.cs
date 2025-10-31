using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using BigRedProf.Data.Core.Internal;
using System.Diagnostics;
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
			private const MultihashAlgorithm SeriesDigestAlgorithm = MultihashAlgorithm.SHA2_256;
			private static readonly Multihash BaselineSeriesDigest = Multihash.FromCode(new Code(new byte[1], 8), SeriesDigestAlgorithm);
		#endregion

		#region fields
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
			private Guid _latestTapeId;
		#endregion

		#region constructors
			private BackupWizard(
			Librarian librarian,
			Guid seriesId,
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
				if (currentTape == null)
					throw new ArgumentNullException(nameof(currentTape));
				if (seriesParentDigest == null)
					throw new ArgumentNullException(nameof(seriesParentDigest));
				if (currentSeriesHeadDigest == null)
					throw new ArgumentNullException(nameof(currentSeriesHeadDigest));
				if (currentTapeContentDigest == null)
					throw new ArgumentNullException(nameof(currentTapeContentDigest));

				_seriesId = seriesId;
				_currentTape = currentTape;
				_seriesName = seriesName ?? throw new ArgumentNullException(nameof(seriesName));
				_seriesDescription = seriesDescription ?? string.Empty;
				_currentSeriesNumber = currentSeriesNumber;
				_seriesParentDigest = seriesParentDigest;
				_currentSeriesHeadDigest = currentSeriesHeadDigest;
				_currentTapeContentDigest = currentTapeContentDigest;
				_isTapeContentDirty = false;
				_latestTapeId = currentTape.Id;
			}
		#endregion

		#region properties
			/// <summary>
			/// Gets a code writer for appending data to the series.
			/// </summary>
			/// <remarks>
			/// Calling SetLatestCheckpoint() disposes the current writer and persists any partial byte state.
			/// </remarks>
			public CodeWriter Writer
			{
				get
				{
					return EnsureCodeWriter();
				}
			}
		#endregion

		#region functions
			public static BackupWizard CreateNew(
			TapeLibrary library,
			Guid seriesId,
			string seriesName,
			string seriesDescription)
			{
				if (library == null)
					throw new ArgumentNullException(nameof(library));

				return CreateNew(library.Librarian, seriesId, seriesName, seriesDescription);
			}

			public static BackupWizard CreateNew(
			Librarian librarian,
			Guid seriesId,
			string seriesName,
			string seriesDescription)
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

				TapeLabel label = tape.ReadLabel();
				label = label.WithSeriesInfo(seriesId, seriesName, 1);
				label = label.WithName(seriesName);
				label = label.WithSeriesDescription(actualDescription);
				label = label.WithContentMultihash(BaselineSeriesDigest);
				label = label.WithSeriesParentMultihash(BaselineSeriesDigest);
				label = label.WithSeriesHeadMultihash(BaselineSeriesDigest);
				tape.WriteLabel(label);

				BackupWizard wizard = new BackupWizard(
				librarian,
				seriesId,
				tape,
				seriesName,
				actualDescription,
				1,
				BaselineSeriesDigest,
				BaselineSeriesDigest,
				BaselineSeriesDigest);
				return wizard;
			}

			public static BackupWizard OpenExisting(TapeLibrary library, Guid seriesId)
			{
				if (library == null)
					throw new ArgumentNullException(nameof(library));

				return OpenExisting(library.Librarian, seriesId);
			}

			public static BackupWizard OpenExisting(Librarian librarian, Guid seriesId)
			{
				if (librarian == null)
					throw new ArgumentNullException(nameof(librarian));
				if (seriesId == Guid.Empty)
					throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));

				IEnumerable<Tape> tapes = librarian.FetchTapesInSeries(seriesId);
				// Policy: operate on the highest-numbered (latest) tape in the series.
				Tape latestTape = SelectLatestTape(tapes);
				TapeLabel label = latestTape.ReadLabel();

				string seriesName = label.SeriesName;
				string seriesDescription;
				if (!label.TryGetSeriesDescription(out seriesDescription))
					seriesDescription = string.Empty;

				Multihash parentDigest;
				if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesParentDigest, out parentDigest))
					parentDigest = BaselineSeriesDigest;

				Multihash headDigest;
				if (!label.TryGetTrait<Multihash>(CoreTrait.SeriesHeadDigest, out headDigest))
					headDigest = parentDigest;

				Multihash contentDigest;
				if (!label.TryGetTrait<Multihash>(CoreTrait.ContentDigest, out contentDigest))
					contentDigest = BaselineSeriesDigest;

				int seriesNumber = label.SeriesNumber;

				BackupWizard wizard = new BackupWizard(
				librarian,
				seriesId,
				latestTape,
				seriesName,
				seriesDescription,
				seriesNumber,
				parentDigest,
				headDigest,
				contentDigest);
				return wizard;
			}
		#endregion


		#region methods
			/// <summary>
			/// Appends code to the current tape series.
			/// </summary>
			/// <param name="code">The code to append.</param>
			public void Append(Code code)
			{
				if (code == null)
					throw new ArgumentNullException(nameof(code));

				Writer.WriteCode(code);
			}

			public Code GetLatestCheckpoint()
			{
				// The latest checkpoint is stored on the current (latest) tape's label.
				TapeLabel label = _currentTape.ReadLabel();
				Code checkpoint;
				if (!label.TryGetClientCheckpoint(out checkpoint))
					throw new InvalidOperationException("No checkpoint has been recorded for the current tape.");

				return checkpoint;
			}

			/// <summary>
			/// Sets the latest client checkpoint for the series.
			/// </summary>
			/// <param name="clientCheckpointCode">The checkpoint code to record.</param>
			/// <remarks>
			/// Calling SetLatestCheckpoint() disposes the current writer and persists any partial byte state.
			/// </remarks>
			public void SetLatestCheckpoint(Code clientCheckpointCode)
			{
				if (clientCheckpointCode == null)
					throw new ArgumentNullException(nameof(clientCheckpointCode));

				FlushWriterStream();
				EnsureWriterFlushed();
				RefreshCurrentTapeState();

				TapeLabel label = _currentTape.ReadLabel();
				label = ApplySeriesMetadata(label);
				Multihash contentDigest;
				if (_isTapeContentDirty)
					contentDigest = ComputeContentDigest(_currentTape);
				else
					contentDigest = _currentTapeContentDigest;

				Multihash seriesHeadDigest;
				if (_isTapeContentDirty)
					seriesHeadDigest = ComputeSeriesHeadDigest(_seriesParentDigest, contentDigest);
				else
					seriesHeadDigest = _currentSeriesHeadDigest;

				label = label.WithContentMultihash(contentDigest);
				label = label.WithSeriesParentMultihash(_seriesParentDigest);
				label = label.WithSeriesHeadMultihash(seriesHeadDigest);
				label = label.WithClientCheckpoint(clientCheckpointCode);
				_currentTape.WriteLabel(label);

				_currentTapeContentDigest = contentDigest;
				_currentSeriesHeadDigest = seriesHeadDigest;
				_isTapeContentDirty = false;
			}


				private TapeLabel ApplySeriesMetadata(TapeLabel label)
			{
				if (label == null)
					throw new ArgumentNullException(nameof(label));

				TapeLabel updatedLabel = label.WithSeriesInfo(_seriesId, _seriesName, _currentSeriesNumber);
				updatedLabel = updatedLabel.WithName(_seriesName);
				updatedLabel = updatedLabel.WithSeriesDescription(_seriesDescription);
				return updatedLabel;
			}

			private void FlushWriterStream()
			{
				if (_seriesStream != null)
					_seriesStream.Flush();
			}

			private void EnsureWriterFlushed()
			{
				if (_codeWriter != null)
					_codeWriter.Dispose();
			}

		private void EnsureLatestTapeMetadata()
		{
			bool hasRollover = false;
			if (_seriesStream != null)
				hasRollover = _seriesStream.TryConsumeRolloverFlag();

			if (_seriesStream != null && !hasRollover && _currentTape.Id == _latestTapeId)
				return;

			IEnumerable<Tape> tapes = _librarian.FetchTapesInSeries(_seriesId);
			Debug.Assert(tapes != null, "Fetched tapes collection must not be null.");
			Tape latestTape = SelectLatestTape(tapes);
			if (!hasRollover && _currentTape.Id == latestTape.Id)
			{
				_latestTapeId = latestTape.Id;
				return;
			}


			// We are moving off the previous (now-finished) tape.
			// Ensure _currentTapeContentDigest is accurate before using it as the parent.
			if (_isTapeContentDirty)
			{
				_currentTapeContentDigest = ComputeContentDigest(_currentTape);
				_isTapeContentDirty = false;
			}

			_seriesParentDigest = _currentTapeContentDigest;
			_currentTape = latestTape;
			_latestTapeId = latestTape.Id;

			TapeLabel label = latestTape.ReadLabel();
			_currentSeriesNumber = label.SeriesNumber;

			TapeLabel updatedLabel = ApplySeriesMetadata(label)
				.WithContentMultihash(BaselineSeriesDigest)
				.WithSeriesParentMultihash(_seriesParentDigest)
				.WithSeriesHeadMultihash(_currentSeriesHeadDigest);
			_currentTape.WriteLabel(updatedLabel);

			_currentTapeContentDigest = BaselineSeriesDigest;
			_isTapeContentDirty = latestTape.Position > 0;
		}

		private void RefreshCurrentTapeState()
			{
				EnsureLatestTapeMetadata();
			}

			private CodeWriter EnsureCodeWriter()
			{
				if (_codeWriter != null)
					return _codeWriter;

				TapeSeriesStream stream = new TapeSeriesStream(_librarian, _seriesId, TapeSeriesStream.OpenMode.Append);
				DirtyTrackingStream trackingStream = new DirtyTrackingStream(this, stream);
				WizardCodeWriter writer = new WizardCodeWriter(this, trackingStream);
				_seriesStream = stream;
				_codeWriter = writer;
				return writer;
			}

			private void MarkTapeContentDirty()
			{
				_isTapeContentDirty = true;
				EnsureLatestTapeMetadata();
			}

			private void OnWriterDisposed()
			{
				_codeWriter = null;
				_seriesStream = null;
			}




			private static Multihash ComputeContentDigest(Tape tape)
			{
				if (tape == null)
					throw new ArgumentNullException(nameof(tape));

				int contentLength = tape.Position;
				if (contentLength <= 0)
					return BaselineSeriesDigest;

				Code content = TapeHelper.ReadContent(tape, 0, contentLength);
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
					int seriesNumber;
					if (!label.TryGetTrait<int>(CoreTrait.SeriesNumber, out seriesNumber))
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

				public override bool CanRead
				{
					get
					{
						return _inner.CanRead;
					}
				}

				public override bool CanSeek
				{
					get
					{
						return _inner.CanSeek;
					}
				}

				public override bool CanWrite
				{
					get
					{
						return _inner.CanWrite;
					}
				}

				public override long Length
				{
					get
					{
						return _inner.Length;
					}
				}

				public override long Position
				{
					get
					{
						return _inner.Position;
					}
					set
					{
						_inner.Position = value;
					}
				}

				public override void Flush()
				{
					_inner.Flush();
				}

				public override int Read(byte[] buffer, int offset, int count)
				{
					return _inner.Read(buffer, offset, count);
				}

				public override long Seek(long offset, SeekOrigin origin)
				{
					return _inner.Seek(offset, origin);
				}

				public override void SetLength(long value)
				{
					_inner.SetLength(value);
				}

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
					get
					{
						return ((IBitAwareStream)_inner).CurrentByte;
					}
					set
					{
						((IBitAwareStream)_inner).CurrentByte = value;
					}
				}

				public int OffsetIntoCurrentByte
				{
					get
					{
						return ((IBitAwareStream)_inner).OffsetIntoCurrentByte;
					}
					set
					{
						((IBitAwareStream)_inner).OffsetIntoCurrentByte = value;
					}
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

		#endregion
	}
}
