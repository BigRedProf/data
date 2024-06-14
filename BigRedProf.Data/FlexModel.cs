using System;
using System.Collections.Generic;

namespace BigRedProf.Data
{
	/// <summary>
	/// A flexible model that allows for a strong decoupling between producers or models
	/// and consumers of models. Clients interact only with specific <see cref="Trait{M}"/>s,
	/// so a consumer client may be able to understand some traits of a <see cref="FlexModel"/>
	/// even if it doesn't understand all of its traits.
	/// </summary>
	public class FlexModel
	{
		#region methods

		/// <summary>
		/// Gets the list of trait identifiers.
		/// </summary>
		/// <returns>A list of trait identifiers.</returns>
		public IList<string> GetTraitIds()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Checks if a trait exists.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait exists, otherwise false.</returns>
		public bool HasTrait(string traitIdentifier)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets a trait by its identifier.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>The trait value.</returns>
		public M GetTrait<M>(string traitIdentifier)
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds a trait.
		/// </summary>
		/// <typeparam name="M">The type of the trait.</typeparam>
		/// <param name="trait">The trait to add.</param>
		public void AddTrait<M>(Trait<M> trait)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes a trait by its identifier.
		/// </summary>
		/// <param name="traitIdentifier">The trait identifier.</param>
		/// <returns>True if the trait was removed, otherwise false.</returns>
		public bool RemoveTrait(string traitIdentifier)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
