using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// Orchestrates reading from a tape series to restore a data source
	/// from its tape backups.
	/// </summary>
	public class RestorationWizard
	{
		#region fields
		#endregion

		#region constructors
		private RestorationWizard(Librarian librarian, Guid seriesId, string seriesName, string seriesDescription)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region properties
		/// <summary>
		/// Gets the <see cref="CodeReader"/> instance used for reading the codes or models from tape.
		/// </summary>
		public CodeReader CodeReader { get; private set; }

		/// <summary>
		/// The total offset, in bits, within the tape series where this restoration wizard is currently positioned.
		/// </summary>
		public long Bookmark { get; private set; }
		#endregion

		#region functions
		/// <summary>
		/// Opens an existing tape series for restoration.
		/// </summary>
		/// <param name="library">The tape library.</param>
		/// <param name="seriesId">The tape series identifier.</param>
		/// <param name="offset">
		/// The offset, in bits, the <see cref="CodeReader"/> will start at. Use this for incremental restores.
		/// </param>
		/// <returns>A <see cref="RestorationWizard"/> instance.</returns>
		public static RestorationWizard OpenExistingTapeSeries(TapeLibrary library, Guid seriesId, long offset)
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
