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
		private readonly Dictionary<Guid, UntypedTrait> _untypedTraits;
		#endregion

		#region constructors
		public FlexModel()
		{
			_untypedTraits = new Dictionary<Guid, UntypedTrait>();
		}

		public FlexModel(int capacity)
		{
			_untypedTraits = new Dictionary<Guid, UntypedTrait>(capacity);
		}

		public FlexModel(params object[] identifierModelPairs)
		{
			if(identifierModelPairs.Length % 2 != 0)
				throw new ArgumentException("The number of arguments must be even.");

			_untypedTraits = new Dictionary<Guid, UntypedTrait>(identifierModelPairs.Length / 2);
			for(int i = 0; i < identifierModelPairs.Length; i += 2)
			{
				Guid traitIdentifier = (Guid)identifierModelPairs[i];
				object model = identifierModelPairs[i + 1];
				_untypedTraits[traitIdentifier] = new UntypedTrait
				{
					TraitId = traitIdentifier,
					Model = model
				};
			}
		}
		#endregion

		#region properties
		internal Dictionary<Guid, UntypedTrait> InternalUntypedTraits => _untypedTraits;
		#endregion

		#region methods
		/// <summary>
		/// Gets the list of trait identifiers.
		/// </summary>
		/// <returns>A list of trait identifiers.</returns>
		public IList<Guid> GetTraitIds()
		{
			return _untypedTraits.Keys.ToList();
		}

		/// <summary>
		/// Checks if a trait exists.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool HasTrait(AttributeFriendlyGuid traitIdentifier)
		{
			return _untypedTraits.ContainsKey(traitIdentifier);
		}

		/// <summary>
		/// Gets a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>The trait value.</returns>
		public M GetTrait<M>(AttributeFriendlyGuid traitIdentifier)
		{
			if (!_untypedTraits.TryGetValue(traitIdentifier, out UntypedTrait untypedTrait))
				throw new ArgumentException($"Trait '{traitIdentifier}' does not exist.");

			return (M)untypedTrait.Model;
		}

		/// <summary>
		/// Tries to get a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <param name="trait">The trait value.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool TryGetTrait<M>(AttributeFriendlyGuid traitIdentifier, out M trait)
		{
			trait = default;
			if (!_untypedTraits.TryGetValue(traitIdentifier, out UntypedTrait untypedTrait))
				return false;

			trait = (M)untypedTrait.Model;
			return true;
		}

		/// <summary>
		/// Adds a trait.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="trait">The trait to add.</param>
		public void AddTrait<M>(Trait<M> trait)
		{
			if (trait == null)
				throw new ArgumentNullException(nameof(trait));

			UntypedTrait untypedTrait = new UntypedTrait
			{
				TraitId = trait.TraitId,
				Model = trait.Model
			};
			_untypedTraits[trait.TraitId] = untypedTrait;
		}

		/// <summary>
		/// Removes a trait by its identifier.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait was removed, otherwise false.</returns>
		public bool RemoveTrait(AttributeFriendlyGuid traitIdentifier)
		{
			return _untypedTraits.Remove(traitIdentifier);
		}
		#endregion
	}
}
