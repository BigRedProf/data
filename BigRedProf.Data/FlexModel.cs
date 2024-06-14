using System;
using System.Collections.Generic;
using System.Linq;

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
		private readonly Dictionary<string, object> _traits;
		#endregion

		#region constructors
		public FlexModel()
		{
			_traits = new Dictionary<string, object>();
		}
		#endregion

		#region methods
		/// <summary>
		/// Gets the list of trait identifiers.
		/// </summary>
		/// <returns>A list of trait identifiers.</returns>
		public IList<string> GetTraitIds()
		{
			return _traits.Keys.ToList();
		}

		/// <summary>
		/// Checks if a trait exists.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool HasTrait(string traitIdentifier)
		{
			return _traits.ContainsKey(traitIdentifier);
		}

		/// <summary>
		/// Gets a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>The trait value.</returns>
		public M GetTrait<M>(string traitIdentifier)
		{
			if (!_traits.TryGetValue(traitIdentifier, out var trait))
				throw new ArgumentException($"Trait '{traitIdentifier}' does not exist.");

			if (trait is Trait<M> typedTrait)
			{
				return typedTrait.Model;
			}
			else
			{
				throw new InvalidCastException($"Trait '{traitIdentifier}' is not of type {typeof(M)}.");
			}
		}

		/// <summary>
		/// Tries to get a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <param name="trait">The trait value.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool TryGetTrait<M>(string traitIdentifier, out M trait)
		{
			trait = default;
			if (!_traits.TryGetValue(traitIdentifier, out var value))
				return false;

			if (value is Trait<M> typedTrait)
			{
				trait = typedTrait.Model;
				return true;
			}
			else
			{
				return false;
			}
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

			_traits[trait.TraitId] = trait;
		}

		/// <summary>
		/// Removes a trait by its identifier.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait was removed, otherwise false.</returns>
		public bool RemoveTrait(string traitIdentifier)
		{
			return _traits.Remove(traitIdentifier);
		}
		#endregion
	}
}
