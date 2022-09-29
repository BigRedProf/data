using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// This class allows models of a given type to be packed and unpacked
	/// to and from codes.
	/// </summary>
	/// <typeparam name="T">The type of model to pack and unpack.</typeparam>
	public abstract class PackRat<T>
	{
		#region protected constructors
		protected PackRat(IPiedPiper piedPiper)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));
				
			PiedPiper = piedPiper;
		}
		#endregion

		#region protected properties
		protected IPiedPiper PiedPiper
		{
			get;
			private set;
		}
		#endregion

		#region abstract methods
		/// <summary>
		/// Packs a model into a <see cref="Code"/>.
		/// </summary>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model to pack.</param>
		public abstract void PackModel(CodeWriter writer, T model);

		/// <summary>
		/// Unpacks a model from a <see cref="Code"/>.
		/// </summary>
		/// <param name="reader">The code reader.</param>
		/// <returns>The unpacked model.</returns>
		public abstract T UnpackModel(CodeReader reader);
		#endregion
	}
}
