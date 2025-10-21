using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Tape
{
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
		#endregion

		#region private constructors
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
		}
		#endregion

		#region functions
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

		public static BackupWizard OpenExisting(Librarian librarian, Guid seriesId)
		{
			if (librarian == null)
				throw new ArgumentNullException(nameof(librarian));
			if (seriesId == Guid.Empty)
				throw new ArgumentException("Series identifier cannot be empty.", nameof(seriesId));

			IEnumerable<Tape> tapes = librarian.FetchTapesInSeries(seriesId);
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
		public Code GetLatestCheckpoint()
		{
			TapeLabel label = _currentTape.ReadLabel();
			Code checkpoint;
			if (!label.TryGetClientCheckpoint(out checkpoint))
				throw new InvalidOperationException("No checkpoint has been recorded for the current tape.");

			return checkpoint;
		}

		public void SetLatestCheckpoint(Code clientCheckpointCode)
		{
			if (clientCheckpointCode == null)
				throw new ArgumentNullException(nameof(clientCheckpointCode));

			TapeLabel label = _currentTape.ReadLabel();
			label = label.WithClientCheckpoint(clientCheckpointCode);
			_currentTape.WriteLabel(label);
		}

		public void Record(Code content)
		{
			if (content == null)
				throw new ArgumentNullException(nameof(content));

			if (content.Length == 0)
				return;

			int remainingBits = content.Length;
			int contentOffset = 0;

			while (remainingBits > 0)
			{
				int tapePosition = _currentTape.Position;
				int availableBits = Tape.MaxContentLength - tapePosition;

				if (availableBits <= 0)
				{
					FinalizeCurrentTape();
					AdvanceToNextTape();
					continue;
				}

				int bitsToWrite = remainingBits < availableBits ? remainingBits : availableBits;
				Code segment = content[contentOffset, bitsToWrite];
				TapeHelper.WriteContent(_currentTape, segment, tapePosition);
				_currentTape.Position = tapePosition + bitsToWrite;

				Multihash contentDigest = ComputeContentDigest(_currentTape);
				Multihash seriesHeadDigest = ComputeSeriesHeadDigest(_seriesParentDigest, contentDigest);

				TapeLabel label = _currentTape.ReadLabel();
				label = ApplySeriesMetadata(label);
				label = label.WithContentMultihash(contentDigest);
				label = label.WithSeriesParentMultihash(_seriesParentDigest);
				label = label.WithSeriesHeadMultihash(seriesHeadDigest);
				_currentTape.WriteLabel(label);

				_currentTapeContentDigest = contentDigest;
				_currentSeriesHeadDigest = seriesHeadDigest;

				contentOffset += bitsToWrite;
				remainingBits -= bitsToWrite;

				if (_currentTape.Position >= Tape.MaxContentLength)
				{
					FinalizeCurrentTape();
					if (remainingBits > 0)
						AdvanceToNextTape();
				}
			}
		}
		#endregion

		#region private methods
		private TapeLabel ApplySeriesMetadata(TapeLabel label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label));

			TapeLabel updatedLabel = label.WithSeriesInfo(_seriesId, _seriesName, _currentSeriesNumber);
			updatedLabel = updatedLabel.WithName(_seriesName);
			updatedLabel = updatedLabel.WithSeriesDescription(_seriesDescription);
			return updatedLabel;
		}

		private void FinalizeCurrentTape()
		{
			_seriesParentDigest = _currentTapeContentDigest;
		}

		private void AdvanceToNextTape()
		{
			Guid tapeId = Guid.NewGuid();
			Tape tape = Tape.CreateNew(_librarian.TapeProvider, tapeId);

			_currentSeriesNumber++;
			_currentTape = tape;
			_currentTapeContentDigest = BaselineSeriesDigest;

			TapeLabel label = tape.ReadLabel();
			label = ApplySeriesMetadata(label);
			label = label.WithContentMultihash(BaselineSeriesDigest);
			label = label.WithSeriesParentMultihash(_seriesParentDigest);
			label = label.WithSeriesHeadMultihash(_currentSeriesHeadDigest);
			_currentTape.WriteLabel(label);
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
		#endregion
	}
}
