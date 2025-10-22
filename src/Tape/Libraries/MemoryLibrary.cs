using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Memory;
using System;

namespace BigRedProf.Data.Tape.Libraries
{
	public sealed class MemoryLibrary : TapeLibrary
	{
		#region constructors
		public MemoryLibrary()
			: this(null)
		{
		}

		public MemoryLibrary(IPiedPiper? piedPiper)
			: base(CreateLibrarian(piedPiper))
		{
		}
		#endregion

		#region private static methods
		private static Librarian CreateLibrarian(IPiedPiper? piedPiper)
		{
			IPiedPiper actualPiedPiper = PreparePiedPiper(piedPiper);

			MemoryTapeProvider provider = new MemoryTapeProvider(actualPiedPiper);
			return new Librarian(provider);
		}
		#endregion
	}
}
