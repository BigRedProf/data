using BigRedProf.Data.PackRats;
using System.Collections.Generic;
using System.Reflection;

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
		void RegisterPackRat<T>(PackRat<T> packRat, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Returns the <see cref="PackRat{T}"/> registered for use with a specific schema identifier.
		/// </summary>
		/// <typeparam name="T">The type of model.</typeparam>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns>The pack rat.</returns>

		PackRat<T> GetPackRat<T>(AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Registers all the core pack rats such as <see cref="BooleanPackRat"/>, <see cref="StringPackRat"/>,
		/// and <see cref="Int32PackRat"/>.
		/// </summary>
		void RegisterCorePackRats();

		/// <summary>
		/// Registers pack rats for all the models in a given assembly decorated with
		/// <see cref="GeneratePackRatAttribute"/>.
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
		/// Registers a <see cref="Tokenizer{TModel}"/> for use with a specified tokenizer identifier.
		/// </summary>
		/// <typeparam name="TModel">The type of model.</typeparam>
		/// <param name="tokenizer">The tokenizer.</param>
		/// <param name="tokenizerId">The tokenizer identifier.</param>
		void RegisterTokenizer<TModel>(Tokenizer<TModel> tokenizer, AttributeFriendlyGuid tokenizerId);

		/// <summary>
		/// Returns the <see cref="Tokenizer{TModel}"/> registered for use with a specific tokenizer
		/// identifier.
		/// </summary>
		/// <typeparam name="TModel">The type of model.</typeparam>
		/// <param name="tokenizerId">The tokenizer identifier.</param>
		/// <returns>The tokenizer.</returns>
		Tokenizer<TModel> GetTokenizer<TModel>(AttributeFriendlyGuid tokenizerId);

		/// <summary>
		/// Registers all the tokenizers in a given assembly decorated with
		/// <see cref="AssemblyTokenizerAttribute"/> and creates their corresponding
		/// pack rats.
		/// </summary>
		/// <param name="assembly">The assembly to reflect.</param>
		void RegisterTokenizers(Assembly assembly);

		/// <summary>
		/// Defines a trait by associating a <see cref="TraitDefinition"/> with a given trait 
		/// identifier.
		/// </summary>
		/// <param name="traitDefintion">The trait definition.</param>
		void DefineTrait(TraitDefinition traitDefintion);

		/// <summary>
		/// Returns the <see cref="TraitDefinition"/> for a given trait identifier.
		/// </summary>
		/// <param name="traitId">The trait identifier.</param>
		/// <returns>The trait definition.</returns>
		TraitDefinition GetTraitDefinition(AttributeFriendlyGuid traitId);

		/// <summary>
		/// Packs a model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model.</param>
		/// <param name="schemaId">The schema identifier.</param>
		void PackModel<M>(CodeWriter writer, M model, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Unpacks a model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns>The model.</returns>
		M UnpackModel<M>(CodeReader reader, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Packs a model using the specified pack rat.
		/// </summary>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model.</param>
		/// <param name="schemaId">The schema identifier.</param>
		void PackModel(CodeWriter writer, object model, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Unpacks a model using the specified pack rat.
		/// </summary>
		/// <param name="reader">The code reader.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <returns>The model.</returns>
		object UnpackModel(CodeReader reader, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Packs a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		void PackNullableModel<M>(
			CodeWriter writer,
			M model,
			AttributeFriendlyGuid schemaId,
			ByteAligned byteAligned
		);

		/// <summary>
		/// Unpacks a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="schemaId">The schema identifier.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		/// <returns>The model.</returns>
		M UnpackNullableModel<M>(CodeReader reader, AttributeFriendlyGuid schemaId, ByteAligned byteAligned);			

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
				AttributeFriendlyGuid elementSchemaId,
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
			AttributeFriendlyGuid elementSchemaId,
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
		Code EncodeModel<M>(M model, AttributeFriendlyGuid schemaId);

        /// <summary>
        /// Decodes a model using the <see cref="PackRat{M}"/> registered for the provided schema.
        /// </summary>
        /// <typeparam name="M"></typeparam>
        /// <param name="code">The encoded model.</param>
        /// <param name="schemaId">The schema identifier.</param>
        /// <returns>The model.</returns>
        M DecodeModel<M>(Code code, AttributeFriendlyGuid schemaId);

		/// <summary>
		/// Stores a <see cref="Code"/> to an array of bytes. The resulting byte array will include
		/// a prefix that encodes the length of the code so that this method can be used in tandem
		/// with <see cref="LoadCodeFromByteArray(IPiedPiper, byte[])"/> to roundtrip codes.
		/// </summary>
		/// <param name="code">The codes to save.</param>
		/// <returns>The byte array.</returns>
		byte[] SaveCodeToByteArray(Code code);

		/// <summary>
		/// Load a <see cref="Code"/> from an array of bytes. This method can be used to
		/// in tandem with <see cref="SaveCodeToByteArray(IPiedPiper, Code)"/> to roundtrip codes.
		/// </summary>
		/// <param name="byteArray">The byte array to load.</param>
		/// <returns>The code.</returns>
		Code LoadCodeFromByteArray(byte[] byteArray);
	}
	#endregion
}
