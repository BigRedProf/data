using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Used to control byte alignment when packing codes. In general, byte aligment
	/// is better for larger models where the performance improvement of moving
	/// bits faster outweighs the space cost.
	/// </summary>
	public enum ByteAligned
	{
		/// <summary>
		/// Align the code reader or writer with the next byte boundary.
		/// </summary>
		Yes = 1,

		/// <summary>
		/// Leave the code reader or writer where it is for maximum  space efficiency.
		/// </summary>
		No = 0
	}
}
