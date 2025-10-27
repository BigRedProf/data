using BigRedProf.Data.Tape.Libraries;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Diagnostics;

namespace BigRedProf.Data.Tape.Test
{
	public class BackupWizardTests
	{
		#region unit tests
		[Trait("Region", "BackupWizard factories")]
		[Fact]
		public void CreateNew_ShouldInitializeSeriesMetadata()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000001");
			string seriesName = "Unit Test Series";
			string seriesDescription = "This is a test series.";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, seriesDescription);
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
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("bbbbbbbb-1111-0000-0000-000000000000");
			string seriesName = "Checkpoint Missing Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "No checkpoints yet");
			Assert.Throws<InvalidOperationException>(() => wizard.GetLatestCheckpoint());
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void SetLatestCheckpoint_ShouldPersistOnLabel()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("bbbbbbbb-0000-0000-0000-000000000002");
			string seriesName = "Checkpoint Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "Checkpoint description");

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
		public void Record_ShouldUpdateTapeContentWithoutComputingDigests()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("cccccccc-0000-0000-0000-000000000003");
			string seriesName = "Record Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "Record description");

			Code content = new Code("11110000 00001111 10101010");
			wizard.Record(content);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			Assert.Equal(content.Length, tape.Position);

			TapeLabel label = tape.ReadLabel();
			Multihash baselineDigest = ComputeBaselineSeriesDigest();
			Assert.Equal(baselineDigest, label.ContentDigest);
			Assert.Equal(baselineDigest, label.SeriesParentDigest);
			Assert.Equal(baselineDigest, label.SeriesHeadDigest);
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void SetLatestCheckpoint_ShouldComputeDigestsAfterRecording()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("cccccccc-0000-0000-0000-000000000006");
			string seriesName = "Checkpoint Digest Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "Checkpoint digest description");

			Code content = new Code("11110000 00001111");
			wizard.Record(content);

			Code checkpoint = new Code("0101");
			wizard.SetLatestCheckpoint(checkpoint);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			TapeLabel label = tape.ReadLabel();

			Code storedCheckpoint;
			Assert.True(label.TryGetClientCheckpoint(out storedCheckpoint));
			Assert.Equal(checkpoint, storedCheckpoint);

			Multihash expectedParentDigest = ComputeBaselineSeriesDigest();
			Multihash expectedContentDigest = ComputeContentDigest(tape);
			Multihash expectedHeadDigest = ComputeSeriesHeadDigest(expectedParentDigest, expectedContentDigest);

			Assert.Equal(expectedContentDigest, label.ContentDigest);
			Assert.Equal(expectedParentDigest, label.SeriesParentDigest);
			Assert.Equal(expectedHeadDigest, label.SeriesHeadDigest);
		}

		[Trait("Region", "BackupWizard methods")]
		[Fact]
		public void Record_ShouldAdvanceToNextTapeWhenCapacityExceeded()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("cccccccc-ffff-0000-0000-000000000005");
			string seriesName = "Overflow Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "Overflow description");

			Tape initialTape = librarian.FetchTapesInSeries(seriesId).Single();
			TapeLabel initialLabel = initialTape.ReadLabel();
			int nearCapacityPosition = Tape.MaxContentLength - 8;
			initialLabel.AddTrait(new Trait<int>(TapeTrait.TapePosition, nearCapacityPosition));
			initialTape.WriteLabel(initialLabel);

			BackupWizard reopenedWizard = BackupWizard.OpenExisting(library, seriesId);
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
			Assert.Equal(firstLabel.ContentDigest, secondLabel.SeriesParentDigest);

			Multihash baselineDigest = ComputeBaselineSeriesDigest();
			Assert.Equal(baselineDigest, firstLabel.ContentDigest);
			Assert.Equal(baselineDigest, secondLabel.ContentDigest);
			Assert.Equal(firstLabel.SeriesHeadDigest, secondLabel.SeriesHeadDigest);
		}

		[Trait("Region", "BackupWizard factories")]
		[Fact]
		public void OpenExisting_ShouldResumeLatestTape()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("dddddddd-0000-0000-0000-000000000004");
			string seriesName = "Resume Series";

			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, seriesName, "Resume description");
			Code initialContent = new Code("10101010 01010101");
			wizard.Record(initialContent);
			Code checkpoint = new Code("0011");
			wizard.SetLatestCheckpoint(checkpoint);

			BackupWizard reopened = BackupWizard.OpenExisting(library, seriesId);
			Code reopenedCheckpoint = reopened.GetLatestCheckpoint();
			Assert.Equal(checkpoint, reopenedCheckpoint);

			Code additionalContent = new Code("11110000");
			reopened.Record(additionalContent);
			Code newCheckpoint = new Code("1100");
			reopened.SetLatestCheckpoint(newCheckpoint);

			Tape tape = librarian.FetchTapesInSeries(seriesId).Single();
			int expectedPosition = initialContent.Length + additionalContent.Length;
			Assert.Equal(expectedPosition, tape.Position);

			TapeLabel label = tape.ReadLabel();
			Code storedCheckpoint;
			Assert.True(label.TryGetClientCheckpoint(out storedCheckpoint));
			Assert.Equal(newCheckpoint, storedCheckpoint);
			Multihash expectedContentDigest = ComputeContentDigest(tape);
			Assert.Equal(expectedContentDigest, label.ContentDigest);
			Multihash expectedParentDigest = ComputeBaselineSeriesDigest();
			Assert.Equal(expectedParentDigest, label.SeriesParentDigest);
			Multihash expectedHeadDigest = ComputeSeriesHeadDigest(expectedParentDigest, expectedContentDigest);
			Assert.Equal(expectedHeadDigest, label.SeriesHeadDigest);
			Code refreshedCheckpoint = reopened.GetLatestCheckpoint();
			Assert.Equal(newCheckpoint, refreshedCheckpoint);
		}

		[Fact]
		[Trait("Region", "BackupWizard factories")]
		public void SuperTest()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("eeeeeeee-0000-4343-0000-000000000001");
			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, "Test Series", "Test Description");

			Code oneHundredM0s = new Code(new string('0', 100_000_000));
			Code oneHundredM1s = new Code(new string('1', 100_000_000));

			wizard.Record(oneHundredM0s);
			wizard.SetLatestCheckpoint(new Code("0001"));

			IList<Tape> tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Single(tapes);
			Tape tape = tapes[0];
			TapeLabel label = tape.ReadLabel();
			Assert.Equal(seriesId, label.SeriesId);
			Assert.Equal(100_000_000, tape.Position);
			Assert.Equal("bciqluhce65z353d537ghv22buiruovi3omln4b4suj2qpmr5orehrji", label.ContentDigest.ToMultibaseString());
			Assert.Equal("bciqj334znsy6vb7fs23mvxgmuobzunjothm44b7ggxg3eoptrsrjj6a", label.SeriesParentDigest.ToMultibaseString());
			Code latestCheckpoint = wizard.GetLatestCheckpoint();
			Assert.Equal(new Code("0001"), latestCheckpoint);

			// write another 400M bits to reach half of tape capacity
			wizard.Record(oneHundredM1s);
			wizard.Record(oneHundredM0s);
			wizard.Record(oneHundredM1s);
			wizard.Record(oneHundredM0s);

			// assert state after writing 500M bits
			tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Single(tapes);
			tape = tapes[0];
			label = tape.ReadLabel();
			Assert.Equal(seriesId, label.SeriesId);
			Assert.Equal(500_000_000, tape.Position);
			// the digests should be unchanged since we haven't updated the checkpoint
			Assert.Equal("bciqluhce65z353d537ghv22buiruovi3omln4b4suj2qpmr5orehrji", label.ContentDigest.ToMultibaseString());
			Assert.Equal("bciqj334znsy6vb7fs23mvxgmuobzunjothm44b7ggxg3eoptrsrjj6a", label.SeriesParentDigest.ToMultibaseString());
			latestCheckpoint = wizard.GetLatestCheckpoint();
			Assert.Equal(new Code("0001"), latestCheckpoint);

			// update checkpoint
			wizard.SetLatestCheckpoint(new Code("0010"));
			tapes = librarian.FetchTapesInSeries(seriesId).ToList();

			// assert state after writing 500M bits *and* updating the checkpoint
			tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Single(tapes);
			tape = tapes[0];
			label = tape.ReadLabel();
			Assert.Equal(seriesId, label.SeriesId);
			Assert.Equal(500_000_000, tape.Position);
			Assert.Equal("bciqcxqslpnvijoietip6m3ppy3wc7cxtzinxnxpgq57eatvxidtigfi", label.ContentDigest.ToMultibaseString());
			Assert.Equal("bciqj334znsy6vb7fs23mvxgmuobzunjothm44b7ggxg3eoptrsrjj6a", label.SeriesParentDigest.ToMultibaseString());
			latestCheckpoint = wizard.GetLatestCheckpoint();
			Assert.Equal(new Code("0010"), latestCheckpoint);

			// write another 500M bits to reach full tape capacity
			wizard.Record(oneHundredM1s);
			wizard.Record(oneHundredM0s);
			wizard.Record(oneHundredM1s);
			wizard.Record(oneHundredM0s);
			wizard.Record(oneHundredM1s);
			wizard.SetLatestCheckpoint(new Code("0011"));

			// assert state after writing 1B bits
			tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Single(tapes);
			tape = tapes[0];
			label = tape.ReadLabel();
			Assert.Equal(seriesId, label.SeriesId);
			Assert.Equal(1_000_000_000, tape.Position);
			Assert.Equal("bciqdcnjjjav5mjvkwssyczvz3azdlr2sb5rshlbyu4375k567gvgtoa", label.ContentDigest.ToMultibaseString());
			Assert.Equal("bciqcxqslpnvijoietip6m3ppy3wc7cxtzinxnxpgq57eatvxidtigfi", label.SeriesParentDigest.ToMultibaseString());
			latestCheckpoint = wizard.GetLatestCheckpoint();
			Assert.Equal(new Code("0011"), latestCheckpoint);

			// write another 1 bit to overflow to a new tape
			wizard.Record(new Code("1"));
			wizard.SetLatestCheckpoint(new Code("0100"));

			// assert state after writing 1B + 1 bits
			tapes = librarian.FetchTapesInSeries(seriesId).ToList();
			Assert.Equal(2, tapes.Count);
			Tape tape1 = tapes[0];
			TapeLabel label1 = tape1.ReadLabel();
			Assert.Equal(seriesId, label1.SeriesId);
			Assert.Equal(1_000_000_000, tape1.Position);
			Assert.Equal("bciqdcnjjjav5mjvkwssyczvz3azdlr2sb5rshlbyu4375k567gvgtoa", label1.ContentDigest.ToMultibaseString());
			Assert.Equal("bciqcxqslpnvijoietip6m3ppy3wc7cxtzinxnxpgq57eatvxidtigfi", label1.SeriesParentDigest.ToMultibaseString());
			Tape tape2 = tapes[1];
			TapeLabel label2 = tape2.ReadLabel();
			Assert.Equal(seriesId, label2.SeriesId);
			Assert.Equal(1, tape2.Position);
			var cd = label2.ContentDigest.ToMultibaseString();
			Debug.WriteLine($"***** cd = {cd}");
			Assert.Equal("bciqc6d6r5cny3yovokjhilwdqdveobtogb5nmrpvxq5nvwfan72ymca", label2.ContentDigest.ToMultibaseString());
			var pd = label2.SeriesParentDigest.ToMultibaseString();
			Debug.WriteLine($"***** pd = {pd}");
			Assert.Equal("bciqdcnjjjav5mjvkwssyczvz3azdlr2sb5rshlbyu4375k567gvgtoa", label2.SeriesParentDigest.ToMultibaseString());
			latestCheckpoint = wizard.GetLatestCheckpoint();
			Assert.Equal(new Code("0100"), latestCheckpoint);
		}
		#endregion

		#region private functions
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
