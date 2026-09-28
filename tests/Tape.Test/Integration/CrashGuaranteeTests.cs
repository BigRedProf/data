using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;
using BigRedProf.Data.Tape.Providers.Memory;

namespace BigRedProf.Data.Tape.Test.Integration
{
	/// <summary>
	/// EXPLORATORY, for BigRedProf/stories#21 and BigRedProf/digihouse#427: what the Tape layer
	/// actually guarantees after a crash, as it stands. These tests pin CURRENT behaviour -- some
	/// of which is the absence of a guarantee -- so a design can rest on facts. Not for merging.
	/// </summary>
	public class CrashGuaranteeTests : IDisposable
	{
		#region fields
		private readonly string _diskPath = Path.Combine(Path.GetTempPath(), "CrashGuaranteeTests");
		private readonly IPiedPiper _piedPiper;
		private readonly PackRat<Code> _codePackRat;
		private static readonly Code[] Things = new Code[] { "0", "00", "10101", "1100110011", "1" };
		#endregion

		#region constructors
		public CrashGuaranteeTests()
		{
			if (Directory.Exists(_diskPath))
				Directory.Delete(_diskPath, true);
			Directory.CreateDirectory(_diskPath);

			_piedPiper = new PiedPiper();
			_piedPiper.RegisterCorePackRats();
			_codePackRat = _piedPiper.GetPackRat<Code>(CoreSchema.Code);
		}

		public void Dispose()
		{
			if (Directory.Exists(_diskPath))
				Directory.Delete(_diskPath, true);
		}
		#endregion

		#region tests
		[Fact]
		public void G1_ASeriesThatNeverReachedItsCheckpointHasNone()
		{
			// Crash = the wizard is abandoned after writing frames, before SetLatestCheckpoint.
			MemoryLibrary library = new MemoryLibrary();
			Guid seriesId = Id(1);
			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, "story", "d");
			Append(wizard, Things);

			BackupWizard reopened = BackupWizard.OpenExisting(library, seriesId);

			// GUARANTEED: an unfinished new series is recognisable -- it has no checkpoint.
			Assert.Throws<InvalidOperationException>(() => reopened.GetLatestCheckpoint());
		}

		[Fact]
		public void G2_AnUnfinishedSeriesEndsPartWayThroughAFrame()
		{
			// Position is persisted into the label on every content write, but the CodeWriter
			// holds the last partial byte until it is disposed -- which a crash never does. So an
			// unfinished series is truncated at an arbitrary BIT, usually mid-frame.
			MemoryLibrary library = new MemoryLibrary();
			Guid seriesId = Id(2);
			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, "story", "d");
			Append(wizard, Things);

			// NOT GUARANTEED: its content is neither complete nor even frame-aligned. Only the
			// checkpoint says what a series holds; without one it holds nothing usable.
			Assert.Throws<InvalidOperationException>(() => ReadFrames(library, seriesId, Things.Length));
		}

		[Fact]
		public void G3_AppendingToAFinishedSeriesAfterACrashLeavesBitsBeyondItsCheckpoint()
		{
			// Finish a series, reopen it, append two more frames, and crash before the next
			// checkpoint. The checkpoint still describes three frames, but the tape has grown.
			MemoryLibrary library = new MemoryLibrary();
			Guid seriesId = Id(3);
			BackupWizard first = BackupWizard.CreateNew(library, seriesId, "story", "d");
			Append(first, Things[0], Things[1], Things[2]);
			first.SetLatestCheckpoint(_piedPiper.PackModel(3L, CoreSchema.Int64));
			int positionAtCheckpoint = library.Librarian.FetchTapesInSeries(seriesId).Single().Position;
			BackupWizard second = BackupWizard.OpenExisting(library, seriesId);
			Append(second, Things[3], Things[4], Things[0], Things[1]);

			Tape tape = library.Librarian.FetchTapesInSeries(seriesId).Single();
			long checkpointed = _piedPiper.UnpackModel<long>(
				BackupWizard.OpenExisting(library, seriesId).GetLatestCheckpoint(), CoreSchema.Int64);

			// The checkpoint is unchanged; the tape's position is not. The next append would land
			// after these orphan bits, and a reader told "read to the end" would read garbage. So:
			// a reader must be told HOW MANY frames, and a finished series must never be appended to.
			Assert.Equal(3L, checkpointed);
			Assert.True(tape.Position > positionAtCheckpoint);
			Assert.Equal(new Code[] { Things[0], Things[1], Things[2] }, ReadFrames(library, seriesId, 3));
		}

		[Fact]
		public void G4_ACorruptedFrameIsReadBackWithoutComplaint()
		{
			MemoryLibrary library = new MemoryLibrary();
			Guid seriesId = Id(4);
			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, "story", "d");
			Append(wizard, new Code("11111111 11111111"));
			wizard.SetLatestCheckpoint("1");
			Tape tape = library.Librarian.FetchTapesInSeries(seriesId).Single();
			Multihash digestWritten = tape.ReadLabel().ContentDigest;

			// Flip content bits behind the wizard's back: bit rot, or a bad copy.
			tape.TapeProvider.WriteTapeInternal(tape.Id, new byte[] { 0x00 }, 1, 1);

			// NOT GUARANTEED: restoration reads the damaged frame and says nothing ...
			Code readBack = ReadFrames(library, seriesId, 1).Single();
			Assert.NotEqual(new Code("11111111 11111111"), readBack);

			// ... although the label's digest would have caught it, had anybody recomputed it.
			Multihash digestNow = new SeriesDigestEngine().ComputeContentDigest(tape);
			Assert.NotEqual(digestWritten, digestNow);
		}

		[Fact]
		public void G5_EveryWriteAlsoRewritesTheLabel()
		{
			// Relevant to an S3 provider (data#87): count provider calls for five small frames.
			CountingTapeProvider provider = new CountingTapeProvider();
			Librarian librarian = new Librarian(provider);
			BackupWizard wizard = BackupWizard.CreateNew(librarian, Id(5), "story", "d");
			Append(wizard, Things);
			wizard.SetLatestCheckpoint("1");

			// Every content write is followed by a label write; nothing is buffered.
			Assert.True(provider.ContentWrites >= 1);
			Assert.True(provider.LabelWrites > provider.ContentWrites);
		}

		[Fact]
		public void G6_ATornLabelOnDiskMakesTheSeriesUnreadable()
		{
			// DiskTapeProvider writes a label with File.WriteAllBytes: not atomic. Simulate a crash
			// part-way through the checkpoint's label write by truncating the label file.
			DiskLibrary library = new DiskLibrary(_diskPath);
			Guid seriesId = Id(6);
			BackupWizard wizard = BackupWizard.CreateNew(library, seriesId, "story", "d");
			Append(wizard, Things);
			wizard.SetLatestCheckpoint("1");
			string labelFile = Directory.GetFiles(_diskPath, "*.label", SearchOption.AllDirectories).Single();
			byte[] label = File.ReadAllBytes(labelFile);
			File.WriteAllBytes(labelFile, label.Take(label.Length / 2).ToArray());

			// It fails LOUDLY rather than reading as some other series -- but the series, and
			// with it every tape's worth of content, is lost. So a disk label write is not a
			// commit point anybody should rely on for a whole run.
			Assert.ThrowsAny<Exception>(() => BackupWizard.OpenExisting(library, seriesId).GetLatestCheckpoint());
		}

		[Fact]
		public void G7_FindingOneSeriesReadsEveryLabelInTheLibrary()
		{
			CountingTapeProvider provider = new CountingTapeProvider();
			Librarian librarian = new Librarian(provider);
			for (int i = 0; i < 20; ++i)
			{
				BackupWizard wizard = BackupWizard.CreateNew(librarian, Id(100 + i), "story", "d");
				Append(wizard, Things[0]);
				wizard.SetLatestCheckpoint("1");
			}
			provider.ResetCounts();

			librarian.FetchTapesInSeries(Id(100));

			// One lookup reads all twenty labels.
			Assert.True(provider.LabelReads >= 20);
		}
		#endregion

		#region private methods
		private void Append(BackupWizard wizard, params Code[] things)
		{
			foreach (Code thing in things)
				wizard.Append(_piedPiper.PackModel(thing, CoreSchema.Code));
		}

		private IList<Code> ReadFrames(TapeLibrary library, Guid seriesId, int count)
		{
			List<Code> frames = new List<Code>();
			using (RestorationWizard restoration = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 0))
			{
				for (int i = 0; i < count; ++i)
					frames.Add(_codePackRat.UnpackModel(restoration.CodeReader));
			}

			return frames;
		}

		private static Guid Id(int n)
		{
			return new Guid(n, 0, 0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 7 });
		}
		#endregion

		#region nested types
		/// <summary>A memory provider that counts what it is asked to do.</summary>
		private sealed class CountingTapeProvider : MemoryTapeProvider
		{
			public int ContentWrites { get; private set; }
			public int LabelWrites { get; private set; }
			public int LabelReads { get; private set; }

			public void ResetCounts()
			{
				ContentWrites = 0;
				LabelWrites = 0;
				LabelReads = 0;
			}

			public override byte[] ReadLabelInternal(Guid tapeId)
			{
				++LabelReads;
				return base.ReadLabelInternal(tapeId);
			}

			public override void WriteTapeInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength)
			{
				++ContentWrites;
				base.WriteTapeInternal(tapeId, data, byteOffset, byteLength);
			}

			public override void WriteLabelInternal(Guid tapeId, byte[] data)
			{
				++LabelWrites;
				base.WriteLabelInternal(tapeId, data);
			}
		}
		#endregion
	}
}
