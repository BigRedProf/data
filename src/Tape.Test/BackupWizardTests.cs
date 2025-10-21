using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using BigRedProf.Data.Tape._TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Tape.Test
{
	public class BackupWizardTests
	{
		#region unit tests
		[Trait("Region", "BackupWizard factories")]
		[Fact]
		public void CreateNew_ShouldInitializeSeriesMetadata()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000001");
			string seriesName = "Unit Test Series";
			string seriesDescription = "This is a test series.";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, seriesDescription);
			Assert.NotNull(wizard);

			IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Single(tapes);
			Tape tape = tapes[0];
			TapeLabel label = tape.ReadLabel();

			Assert.Equal(seriesId, label.SeriesId);
			Assert.Equal(seriesName, label.SeriesName);
			Assert.Equal(1, label.SeriesNumber);
			Assert.Equal(seriesName, label.Name);
			string storedDescription;
			Assert.True(label.TryGetSeriesDescription(out storedDescription));
			Assert.Equal(seriesDescription, storedDescription);
			Assert.Equal(0, tape.Position);
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void GetLatestCheckpoint_ShouldThrowWhenCheckpointMissing()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("bbbbbbbb-1111-0000-0000-000000000000");
			string seriesName = "Checkpoint Missing Series";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, "No checkpoints yet");
			Assert.Throws<InvalidOperationException>(() => wizard.GetLatestCheckpoint());
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void SetLatestCheckpoint_ShouldPersistOnLabel()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("bbbbbbbb-0000-0000-0000-000000000002");
			string seriesName = "Checkpoint Series";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, "Checkpoint description");

			Code checkpoint = new Code("10101010");
			wizard.SetLatestCheckpoint(checkpoint);

			Code retrieved = wizard.GetLatestCheckpoint();
			Assert.Equal(checkpoint, retrieved);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			TapeLabel label = tape.ReadLabel();
			Code stored;
			Assert.True(label.TryGetClientCheckpoint(out stored));
			Assert.Equal(checkpoint, stored);
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void Record_ShouldUpdateTapeContentAndDigests()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("cccccccc-0000-0000-0000-000000000003");
			string seriesName = "Record Series";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, "Record description");

			Code content = new Code("11110000 00001111 10101010");
			wizard.Record(content);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			Assert.Equal(content.Length, tape.Position);

			TapeLabel label = tape.ReadLabel();
			Multihash actualContentDigest = label.ContentMultihash;
			Multihash expectedContentDigest = ComputeContentDigest(tape);
			Assert.Equal(expectedContentDigest, actualContentDigest);

			Multihash expectedParentDigest = ComputeBaselineSeriesDigest();
			Assert.Equal(expectedParentDigest, label.SeriesParentMultihash);

			Multihash expectedHeadDigest = ComputeSeriesHeadDigest(expectedParentDigest, expectedContentDigest);
			Assert.Equal(expectedHeadDigest, label.SeriesHeadMultihash);
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void Record_ShouldAdvanceToNextTapeWhenCapacityExceeded()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("cccccccc-ffff-0000-0000-000000000005");
			string seriesName = "Overflow Series";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, "Overflow description");

			Tape initialTape = librarian.FetchTapesInSeries(seriesId).Single();
			TapeLabel initialLabel = initialTape.ReadLabel();
			int nearCapacityPosition = Tape.MaxContentLength - 8;
			initialLabel.AddTrait(new Trait<int>(TapeTrait.TapePosition, nearCapacityPosition));
			initialTape.WriteLabel(initialLabel);

			BackupWizard reopenedWizard = BackupWizard.OpenExisting(librarian, seriesId);
			Code content = new Code("10101010 11110000");
			reopenedWizard.Record(content);

			IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId)
				.OrderBy(tape => tape.ReadLabel().SeriesNumber)
				.ToList();
			Assert.Equal(2, tapes.Count);

			Tape firstTape = tapes[0];
			Tape secondTape = tapes[1];
			TapeLabel firstLabel = firstTape.ReadLabel();
			TapeLabel secondLabel = secondTape.ReadLabel();

			Assert.Equal(Tape.MaxContentLength, firstTape.Position);
			int overflowBits = Tape.MaxContentLength - nearCapacityPosition;
			int expectedRemainder = content.Length - overflowBits;
			Assert.Equal(expectedRemainder, secondTape.Position);
			Assert.Equal(firstLabel.SeriesNumber + 1, secondLabel.SeriesNumber);
			Assert.Equal(firstLabel.ContentMultihash, secondLabel.SeriesParentMultihash);

			Multihash expectedSecondHead = ComputeSeriesHeadDigest(secondLabel.SeriesParentMultihash, secondLabel.ContentMultihash);
			Assert.Equal(expectedSecondHead, secondLabel.SeriesHeadMultihash);
		}

		[Trait("Region", "BackupWizard factories")]
		[Fact]
		public void OpenExisting_ShouldResumeLatestTape()
		{
			TapeProvider tapeProvider = TapeProviderHelper.CreateMemoryTapeProvider();
			Librarian librarian = new Librarian(tapeProvider.PiedPiper, tapeProvider);
			Guid seriesId = new Guid("dddddddd-0000-0000-0000-000000000004");
			string seriesName = "Resume Series";

			BackupWizard wizard = BackupWizard.CreateNew(librarian, seriesId, seriesName, "Resume description");
			Code initialContent = new Code("10101010 01010101");
			wizard.Record(initialContent);
			Code checkpoint = new Code("0011");
			wizard.SetLatestCheckpoint(checkpoint);

			BackupWizard reopened = BackupWizard.OpenExisting(librarian, seriesId);
			Code reopenedCheckpoint = reopened.GetLatestCheckpoint();
			Assert.Equal(checkpoint, reopenedCheckpoint);

			Code additionalContent = new Code("11110000");
			reopened.Record(additionalContent);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			int expectedPosition = initialContent.Length + additionalContent.Length;
			Assert.Equal(expectedPosition, tape.Position);

			TapeLabel label = tape.ReadLabel();
			Multihash expectedContentDigest = ComputeContentDigest(tape);
			Assert.Equal(expectedContentDigest, label.ContentMultihash);
			Multihash expectedParentDigest = ComputeBaselineSeriesDigest();
			Assert.Equal(expectedParentDigest, label.SeriesParentMultihash);
			Multihash expectedHeadDigest = ComputeSeriesHeadDigest(expectedParentDigest, expectedContentDigest);
			Assert.Equal(expectedHeadDigest, label.SeriesHeadMultihash);
		}
		#endregion

		#region private static methods
		private static Multihash ComputeContentDigest(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape));

			int contentLength = tape.Position;
			if (contentLength == 0)
				return ComputeBaselineSeriesDigest();

			TapePlayer player = new TapePlayer();
			player.InsertTape(tape);
			player.RewindOrFastForwardTo(0);
			Code content = player.Play(contentLength);
			return Multihash.FromCode(content, MultihashAlgorithm.SHA2_256);
		}

		private static Multihash ComputeBaselineSeriesDigest()
		{
			byte[] zeroBytes = new byte[1];
			Code zeroCode = new Code(zeroBytes, 8);
			return Multihash.FromCode(zeroCode, MultihashAlgorithm.SHA2_256);
		}

		private static Multihash ComputeSeriesHeadDigest(Multihash parentDigest, Multihash contentDigest)
		{
			if (parentDigest == null)
				throw new ArgumentNullException(nameof(parentDigest));
			if (contentDigest == null)
				throw new ArgumentNullException(nameof(contentDigest));

			byte[] parentBytes = parentDigest.Digest;
			byte[] contentBytes = contentDigest.Digest;
			byte[] combined = new byte[parentBytes.Length + contentBytes.Length];
			Array.Copy(parentBytes, 0, combined, 0, parentBytes.Length);
			Array.Copy(contentBytes, 0, combined, parentBytes.Length, contentBytes.Length);
			Code combinedCode = new Code(combined);
			return Multihash.FromCode(combinedCode, MultihashAlgorithm.SHA2_256);
		}
		#endregion
	}
}
