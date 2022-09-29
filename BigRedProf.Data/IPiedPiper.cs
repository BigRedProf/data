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

		/// <summary>
		/// Packs a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model.</param>
		/// <param name="packRat">The pack rat.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		void PackNullableModel<M>(
			CodeWriter writer,
			M model,
			PackRat<M> packRat,
			ByteAligned byteAligned
		)
		where M : new();

		/// <summary>
		/// Unpacks a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="packRat">The pack rat.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		/// <returns>The model.</returns>
		M UnpackNullableModel<M>(CodeReader reader, PackRat<M> packRat, ByteAligned byteAligned)
			where M : new();

		/// <summary>
		/// Packs a list using the specified list element pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="list">The list.</param>
		/// <param name="elementSchemaId">The element schema identifier.</param>
		/// <param name="allowNullLists">Whether or not null lists are allowed.</param>
		/// <param name="allowNullElements">Whether or not null elements are allowed in the list.</param>
		/// <param name="byteAligned">Controls the packing size of the null markers.</param>
		void PackList<M>(
				CodeWriter writer,
				IList<M> list,
				string elementSchemaId,
				bool allowNullLists,
				bool allowNullElements,
				ByteAligned byteAligned
			);

		/// <summary>
		/// Unpacks a list using the specified list element pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="elementSchemaId">The element schema identifier.</param>
		/// <param name="allowNullLists">Whether or not null lists are allowed.</param>
		/// <param name="allowNullElements">Whether or not null elements are allowed in the list.</param>
		/// <param name="byteAligned">Controls the packing size of the null markers.</param>
		/// <returns>The list.</returns>
		IList<M> UnpackList<M>(
			CodeReader reader,
			string elementSchemaId,
			bool allowNullLists,
			bool allowNullElements,
			ByteAligned byteAligned
		);
	}
	#endregion
}
