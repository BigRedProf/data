using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal
{
	// TODO: Revisit whether this should be public or internal. It was internal
	// until we made it public for TapeSeriesWriter. If public, move to Core namespace.
	public interface IBitAwareStream
	{
		#region properties
		byte CurrentByte { get; set; }
		int OffsetIntoCurrentByte { get; set; }
		#endregion
	}
}
