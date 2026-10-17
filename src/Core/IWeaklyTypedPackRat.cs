using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// This interface encapsulates the non-generic packing and unpacking methods. You can cast a
	/// <see cref="PackRat{T}"/> to this interface if you need to call them.
	/// </summary>
	internal interface IWeaklyTypedPackRat
	{
		#region abstract methods
		/// <summary>
		/// Packs a model into a <see cref="Code"/>.
		/// </summary>
		/// <param name="stream">The bit stream.</param>
		/// <param name="model">The model to pack.</param>
		void PackModel(BitStream stream, object model);

		/// <summary>
		/// Unpacks a model from a <see cref="Code"/>.
		/// </summary>
		/// <param name="stream">The bit stream.</param>
		/// <returns>The unpacked model.</returns>
		object UnpackModel(BitStream stream);
		#endregion
	}
}
