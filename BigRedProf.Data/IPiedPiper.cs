using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// A service for work withing models and <see cref="PackRat"/>s.
	/// </summary>
	public interface IPiedPiper
	{
		#region methods
		/// <summary>
		/// Registers a <see cref="PackRat{T}"/> for use with a specific schema identifier.
		/// </summary>
		/// <typeparam name="T">The type of model.</typeparam>
		/// <param name="packRat">The pack rat.</param>
		/// <param name="schemaId">The schema identifier.</param>
		void RegisterPackRat<T>(PackRat<T> packRat, string schemaId);

		/// <summary>
		/// Returns the <see cref="PackRat{T}"/> registered for use with a specific schema identifier.
		/// </summary>
		/// <typeparam name="T">The type of model.</typeparam>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns></returns>
		PackRat<T> GetPackRat<T>(string schemaId);
	}
	#endregion
}
