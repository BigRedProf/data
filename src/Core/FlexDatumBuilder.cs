using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Builds a <see cref="FlexDatum"/>.
	/// </summary>
	/// <remarks>
	/// A flex datum is a durable record and so is immutable. This is where the mutation lives:
	/// add the traits you understand, carry forward the trait values you do not, and build.
	/// </remarks>
	public class FlexDatumBuilder
	{
		#region fields
		private readonly IPiedPiper _piedPiper;
		private readonly Dictionary<Guid, Code> _traitCodes;
		#endregion

		#region constructors
		public FlexDatumBuilder(IPiedPiper piedPiper)
			: this(piedPiper, Array.Empty<TraitValue>())
		{
		}

		public FlexDatumBuilder(IPiedPiper piedPiper, IEnumerable<TraitValue> traitValues)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			if (traitValues == null)
				throw new ArgumentNullException(nameof(traitValues));

			_piedPiper = piedPiper;
			_traitCodes = new Dictionary<Guid, Code>();
			foreach (TraitValue traitValue in traitValues)
				_traitCodes[traitValue.TraitId] = traitValue.Code;
		}
		#endregion

		#region methods
		/// <summary>
		/// Answers a trait, packing the answer under the schema its trait identifier fixes.
		/// </summary>
		/// <typeparam name="M">The model type.</typeparam>
		/// <param name="traitId">The trait identifier.</param>
		/// <param name="value">The trait's value.</param>
		public FlexDatumBuilder AddTrait<M>(AttributeFriendlyGuid traitId, M value)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			Guid schemaId = _piedPiper.GetTrait(traitId).SchemaId;

			// An enum answered through an integral schema has no pack rat of its own, so pack it
			// as its underlying value. GetTrait does the same in reverse.
			if (typeof(M).IsEnum)
			{
				object underlyingValue = Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(M)));
				_traitCodes[traitId] = _piedPiper.PackModel(underlyingValue, schemaId);
			}
			else
			{
				_traitCodes[traitId] = _piedPiper.PackModel(value, schemaId);
			}

			return this;
		}

		/// <summary>
		/// Adds an already-packed trait value.
		/// </summary>
		/// <remarks>
		/// This is what carries an unrecognised trait forward untouched, and it deliberately does
		/// not require the trait to be defined.
		/// </remarks>
		public FlexDatumBuilder AddTraitValue(TraitValue traitValue)
		{
			if (traitValue == null)
				throw new ArgumentNullException(nameof(traitValue));

			_traitCodes[traitValue.TraitId] = traitValue.Code;

			return this;
		}

		/// <summary>
		/// Removes a trait.
		/// </summary>
		/// <returns>True if the trait was present, otherwise false.</returns>
		public bool RemoveTrait(AttributeFriendlyGuid traitId)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			return _traitCodes.Remove(traitId);
		}

		/// <summary>
		/// Builds the flex datum.
		/// </summary>
		public FlexDatum Build()
		{
			List<TraitValue> traitValues = new List<TraitValue>(_traitCodes.Count);
			foreach (KeyValuePair<Guid, Code> traitCode in _traitCodes)
				traitValues.Add(new TraitValue(traitCode.Key, traitCode.Value));

			return new FlexDatum(traitValues);
		}
		#endregion
	}
}
