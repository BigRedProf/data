using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeLibrary
	{
		#region fields
		private readonly Librarian _librarian;
		#endregion

		#region constructors
		protected TapeLibrary(Librarian librarian)
		{
			if (librarian == null)
				throw new ArgumentNullException(nameof(librarian));

			_librarian = librarian;
		}
		#endregion

		#region properties
		public Librarian Librarian
		{
			get { return _librarian; }
		}
		#endregion

		#region protected methods
		protected static IPiedPiper PreparePiedPiper(IPiedPiper? piedPiper)
		{
			IPiedPiper actualPiedPiper = piedPiper ?? new PiedPiper();
			TapeProvider.EnsureTapePiedPiper(actualPiedPiper);
			return actualPiedPiper;
		}
		#endregion

		#region internal properties
		internal TapeProvider TapeProvider
		{
			get { return _librarian.TapeProvider; }
		}
		#endregion
	}
}
