using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape
{
	public class BackupWizard
	{
		#region fields
		private Librarian _librarian;
		private Guid _seriesId;
		#endregion

		#region private constructors
		private BackupWizard(Librarian librarian, Guid seriesId)
		{
			_librarian = librarian;
			_seriesId = seriesId;
		}
		#endregion

		#region functions
		public BackupWizard CreateNew(
			Librarian librarian, 
			Guid seriesId, 
			string seriesName, 
			string seriesDescription
		)
		{
			throw new NotImplementedException();
		}

		public BackupWizard OpenExisting(Librarian librarian, Guid seriesId)
		{
			throw new NotImplementedException(); 
		}
		#endregion

		#region methods
		public Code GetLatestCheckpoint()
		{
			throw new NotImplementedException();
		}

		public void SetLatestCheckpoint(Code clientCheckpointCode)
		{
			throw new NotImplementedException();
		}

		public void Record(Code content)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
