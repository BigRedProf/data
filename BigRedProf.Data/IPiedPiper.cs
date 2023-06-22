using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Schema;

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
		/// Registers all the default pack rats such as <see cref="BooleanPackRat"/>, <see cref="StringPackRat"/>,
		/// and <see cref="Int32PackRat"/>.
		/// </summary>
		void RegisterDefaultPackRats();

		/// <summary>
		/// Registers pack rats for all the models in a given assembly decorated with
		/// <see cref="RegisterPackRatAttribute"/>.
		/// </summary>
		/// <param name="assembly">The assembly to reflect.</param>
		/// <remarks>
		/// This method is used in conjection with the pack rat compiler.
		/// 1. Decorate the models in a given assembly with pack rat attributes.
		/// 2. Run the pack rat compiler as part of the regular build.
		/// 3. In apps consuming the assembly, call this method at start-up
		/// to register the pack rats.
		/// </remarks>
		void RegisterPackRats(Assembly assembly);

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

		/// <summary>
		/// Encodes a model using the <see cref="PackRat{M}"/> registered for the provided schema.
		/// </summary>
		/// <typeparam name="M">The model type.</typeparam>
		/// <param name="model">The model.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns>The code.</returns>
		Code EncodeModel<M>(M model, string schemaId);

        /// <summary>
        /// Decodes a model using the <see cref="PackRat{M}"/> registered for the provided schema.
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="code">The encoded model.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns>The model.</returns>
        M DecodeModel<M>(Code code, string schemaId);

		/// <summary>
		/// Encodes a model with its schema using the <see cref="PackRat{M}"/> registered for the
		/// provided schema.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns>The code.</returns>
		Code EncodeModelWithSchema(object model, string schemaId);

		/// <summary>
		/// Decodes a model with its schema using the <see cref="PackRat{M}"/> registered for the
		/// provided schema.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="code">The encoded model.</param>
		/// <returns>The model.</returns>
		ModelWithSchema DecodeModelWithSchema(Code code);
	}
	#endregion
}
