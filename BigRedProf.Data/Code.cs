using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// A <see cref="Code"/> is an ordered set of bits. The caller is responsible for encoding models into codes, decoding codes into models
	/// and defining the meaning of codes it uses.
	/// </summary>
	public class Code
	{
		#region constructors
		public Code(params Bit[] bits)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region properties
		/// <summary>
		/// The length, in bits, of the code.
		/// </summary>
		public int Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Gets or sets the value of a specific <see cref="Bit"/> within the code.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public Bit this[int offset]
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}
