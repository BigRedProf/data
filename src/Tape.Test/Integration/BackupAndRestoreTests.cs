using BigRedProf.Data.Tape.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		#endregion
	}
}
