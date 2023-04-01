using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal
{
	internal interface IBitAwareStream
	{
		#region properties
		byte CurrentByte { get; set; }
		int OffsetIntoCurrentByte { get; set; }
		#endregion
	}
}
