using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Libraries;

namespace BigRedProf.Data.Tape.Test.Integration
{
	public class BackupAndRestoreTests
	{
		#region integration tests
		[Trait("Region", "Backup And Restore")]
		[Fact]
		public void BackupAndRestore_ShouldWork_WithByteAlignedWrites()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000001");
			string seriesName = "byte aligned";
			string seriesDescription = "These reads and writes are byte-aligned.";

			BackupWizard backupWizard = BackupWizard.CreateNew(library, seriesId, seriesName, seriesDescription);
			backupWizard.Writer.WriteCode("00000000");
			backupWizard.Writer.WriteCode("11111111");
			backupWizard.Writer.WriteCode("00000000");
			backupWizard.Writer.WriteCode("11111111");
			backupWizard.Writer.WriteCode("00000000");
			backupWizard.Writer.WriteCode("11111111");
			backupWizard.Writer.WriteCode("00000000");
			backupWizard.Writer.WriteCode("11111111");

			RestorationWizard restorationWizard = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 0);
			Assert.Equal("00000000", restorationWizard.CodeReader.Read(8));
			Assert.Equal("11111111", restorationWizard.CodeReader.Read(8));
			Assert.Equal("00000000", restorationWizard.CodeReader.Read(8));
			Assert.Equal("11111111", restorationWizard.CodeReader.Read(8));
			Assert.Equal("00000000", restorationWizard.CodeReader.Read(8));
			Assert.Equal("11111111", restorationWizard.CodeReader.Read(8));
			Assert.Equal("00000000", restorationWizard.CodeReader.Read(8));
			Assert.Equal("11111111", restorationWizard.CodeReader.Read(8));
		}

		[Trait("Region", "Backup And Restore")]
		[Fact]
		public void BackupAndRestore_ShouldWork_WithNonByteAlignedWrites()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000002");
			string seriesName = "not byte aligned";
			string seriesDescription = "These reads and writes are NOT byte-aligned.";

			BackupWizard backupWizard = BackupWizard.CreateNew(library, seriesId, seriesName, seriesDescription);
			backupWizard.Writer.WriteCode("00000");
			backupWizard.Writer.WriteCode("11111");
			backupWizard.Writer.WriteCode("00000");
			backupWizard.Writer.WriteCode("11111");
			backupWizard.Writer.WriteCode("00000");
			backupWizard.Writer.WriteCode("11111");
			backupWizard.Writer.WriteCode("00000");
			backupWizard.Writer.WriteCode("11111");

			RestorationWizard restorationWizard = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 0);
			Assert.Equal("00000", restorationWizard.CodeReader.Read(5));
			Assert.Equal("11111", restorationWizard.CodeReader.Read(5));
			Assert.Equal("00000", restorationWizard.CodeReader.Read(5));
			Assert.Equal("11111", restorationWizard.CodeReader.Read(5));
			Assert.Equal("00000", restorationWizard.CodeReader.Read(5));
			Assert.Equal("11111", restorationWizard.CodeReader.Read(5));
			Assert.Equal("00000", restorationWizard.CodeReader.Read(5));
			Assert.Equal("11111", restorationWizard.CodeReader.Read(5));
		}

		[Trait("Region", "Backup And Restore")]
		[Fact]
		public void BackupAndRestore_ShouldWork_WithNonByteAlignedWrites2()
		{
			MemoryLibrary library = new MemoryLibrary();
			Librarian librarian = library.Librarian;
			Guid seriesId = new Guid("aaaaaaaa-0000-0000-0000-000000000003");
			string seriesName = "not byte aligned II";
			string seriesDescription = "These reads and writes are also NOT byte-aligned.";
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			PackRat<Code> codePackRat = piedPiper.GetPackRat<Code>(CoreSchema.Code);

			BackupWizard backupWizard = BackupWizard.CreateNew(library, seriesId, seriesName, seriesDescription);
			backupWizard.Writer.WriteCode(piedPiper.EncodeModel<Code>("0", CoreSchema.Code));
			backupWizard.Writer.WriteCode(piedPiper.EncodeModel<Code>("00", CoreSchema.Code));
			backupWizard.Writer.WriteCode(piedPiper.EncodeModel<Code>("10101", CoreSchema.Code));

			RestorationWizard restorationWizard = RestorationWizard.OpenExistingTapeSeries(library, seriesId, 0);
			Code code1 = codePackRat.UnpackModel(restorationWizard.CodeReader);
			Assert.Equal("0", code1.ToString());
			Code code2 = codePackRat.UnpackModel(restorationWizard.CodeReader);
			Assert.Equal("00", code2.ToString());
			Code code3 = codePackRat.UnpackModel(restorationWizard.CodeReader);
			Assert.Equal("10101", code3.ToString());
		}
		#endregion
	}
}
