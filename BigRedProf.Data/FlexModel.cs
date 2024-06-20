using System;
using System.Collections.Generic;
using System.Linq;
using BigRedProf.Data.Internal;

namespace BigRedProf.Data
{
	/// <summary>
	/// A flexible model that allows for a strong decoupling between producers of models
	/// and consumers of models. Clients interact only with specific <see cref="Trait{M}"/>s,
	/// so a consumer client may be able to understand some traits of a <see cref="FlexModel"/>
	/// even if it doesn't understand all of its traits.
	/// </summary>
	public class FlexModel
	{
		#region fields
		private readonly Dictionary<string, EncodedTrait> _encodedTraits;
		#endregion

		#region constructors
		public FlexModel()
		{
			_encodedTraits = new Dictionary<string, EncodedTrait>();
		}

		public FlexModel(int capacity)
		{
			_encodedTraits = new Dictionary<string, EncodedTrait>(capacity);
		}
		#endregion

		#region properties
		internal Dictionary<string, EncodedTrait> InternalEncodedTraits => _encodedTraits;
		#endregion

		#region methods
		/// <summary>
		/// Gets the list of trait identifiers.
		/// </summary>
		/// <returns>A list of trait identifiers.</returns>
		public IList<string> GetTraitIds()
		{
			return _encodedTraits.Keys.ToList();
		}

		/// <summary>
		/// Checks if a trait exists.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool HasTrait(string traitIdentifier)
		{
			return _encodedTraits.ContainsKey(traitIdentifier);
		}

		/// <summary>
		/// Gets a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="piedPiper">The pied piper.</param>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>The trait value.</returns>
		public M GetTrait<M>(IPiedPiper piedPiper, string traitIdentifier)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			if (!_encodedTraits.TryGetValue(traitIdentifier, out var encodedTrait))
				throw new ArgumentException($"Trait '{traitIdentifier}' does not exist.");

			string schemaId = piedPiper.GetTraitDefinition(traitIdentifier).SchemaId;
			return piedPiper.DecodeModel<M>(encodedTrait.EncodedModel, schemaId);
		}

		/// <summary>
		/// Tries to get a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="piedPiper">The pied piper.</param>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <param name="trait">The trait value.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool TryGetTrait<M>(IPiedPiper piedPiper, string traitIdentifier, out M trait)
		{
			trait = default;
			if (!_encodedTraits.TryGetValue(traitIdentifier, out var encodedTrait))
			{
				return false;
			}

			try
			{
				string schemaId = piedPiper.GetTraitDefinition(traitIdentifier).SchemaId;
				trait = piedPiper.DecodeModel<M>(encodedTrait.EncodedModel, schemaId);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Adds a trait.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="piedPiper">The pied piper.</param>
		/// <param name="trait">The trait to add.</param>
		public void AddTrait<M>(IPiedPiper piedPiper, Trait<M> trait)
		{
			if (trait == null)
				throw new ArgumentNullException(nameof(trait));

			string schemaId = piedPiper.GetTraitDefinition(trait.TraitId).SchemaId;
			var encodedTrait = new EncodedTrait
			{
				TraitId = trait.TraitId,
				EncodedModel = piedPiper.EncodeModel(trait.Model, schemaId)
			};

			_encodedTraits[trait.TraitId] = encodedTrait;
		}

		/// <summary>
		/// Removes a trait by its identifier.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait was removed, otherwise false.</returns>
		public bool RemoveTrait(string traitIdentifier)
		{
			return _encodedTraits.Remove(traitIdentifier);
		}
		#endregion
	}
}
