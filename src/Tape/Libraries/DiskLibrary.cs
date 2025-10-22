using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Disk;
using System;

namespace BigRedProf.Data.Tape.Libraries
{
	public sealed class DiskLibrary : TapeLibrary
	{
		#region constructors
		public DiskLibrary(string directoryPath)
			: this(null, directoryPath)
		{
		}

		public DiskLibrary(IPiedPiper? piedPiper, string directoryPath)
			: base(CreateLibrarian(piedPiper, directoryPath))
		{
		}
		#endregion

		#region private static methods
		private static Librarian CreateLibrarian(IPiedPiper? piedPiper, string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
				throw new ArgumentNullException(nameof(directoryPath));

			IPiedPiper actualPiedPiper = PreparePiedPiper(piedPiper);
			DiskTapeProvider provider = new DiskTapeProvider(actualPiedPiper, directoryPath);
			return new Librarian(provider);
		}
		#endregion
	}
}
