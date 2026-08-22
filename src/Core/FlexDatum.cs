using System;
using System.Collections.Generic;
using System.Linq;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A datum whose subject is represented by a set of independently identified
	/// <see cref="Trait"/>s.
	/// </summary>
	/// <remarks>
	/// Picture a physical card. Down its face are entries, each one a trait identifier followed by
	/// the answer to that question. A reader works down it, and for each entry either recognises
	/// the identifier -- in which case it knows the question, knows the schema of the answer, and
	/// can read it -- or does not, in which case it skips that entry and continues.
	///
	/// So a writer may record traits a reader does not understand, a reader can use every trait it
	/// recognises without understanding the rest, and unrecognised traits do not prevent
	/// interpretation of the recognised ones. That is a compatibility guarantee obtained from the
	/// structure of the thing itself, not from discipline or negotiation between the parties.
	///
	/// A flex datum holds codes, never decoded models, which is what lets an unrecognised trait be
	/// held, enumerated, compared, and copied forward untouched. Interpretation is something a
	/// reader brings, via <see cref="GetTrait{M}"/>, and only then does a pack rat need to exist.
	///
	/// Instances are immutable. Build one with <see cref="FlexDatumBuilder"/>, or derive one from
	/// an existing flex datum with <see cref="ToBuilder"/>.
	/// </remarks>
	public class FlexDatum : IEquatable<FlexDatum>
	{
		#region fields
		// Canonical order: sorted by the trait identifier's wire bytes. See TraitValues.
		private readonly TraitValue[] _traitValues;
		private readonly Dictionary<Guid, Code> _byTraitId;
		private int _hashCode;
		private bool _hasHashCode;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a flex datum from a set of trait values.
		/// </summary>
		/// <param name="traitValues">
		/// The trait values. Order is irrelevant; the flex datum imposes its own. A trait may
		/// appear only once -- a subject has at most one answer per question.
		/// </param>
		public FlexDatum(IEnumerable<TraitValue> traitValues)
		{
			if (traitValues == null)
				throw new ArgumentNullException(nameof(traitValues));

			_byTraitId = new Dictionary<Guid, Code>();
			foreach (TraitValue traitValue in traitValues)
			{
				if (traitValue == null)
					throw new ArgumentException("A trait value cannot be null.", nameof(traitValues));

				if (_byTraitId.ContainsKey(traitValue.TraitId))
				{
					throw new ArgumentException(
						$"Trait '{traitValue.TraitId}' appears more than once. A subject has at " +
						"most one answer per question.",
						nameof(traitValues)
					);
				}

				_byTraitId.Add(traitValue.TraitId, traitValue.Code);
			}

			_traitValues = _byTraitId
				.Select(kvp => new TraitValue(kvp.Key, kvp.Value))
				.OrderBy(traitValue => traitValue.TraitId, TraitIdComparer.Instance)
				.ToArray();
		}
		#endregion

		#region properties
		/// <summary>
		/// The trait values, in canonical order.
		/// </summary>
		/// <remarks>
		/// Two flex data bearing the same entries are the same record and must be
		/// indistinguishable, so the entries have a canonical order fixed by the trait identifiers
		/// themselves rather than by the order someone happened to write them down. Without that,
		/// the same record has many possible faces, and you can neither compare two records by
		/// their codes nor summarise one by a fingerprint.
		///
		/// The order is by the trait identifier's wire bytes, ascending. That is deliberately not
		/// <see cref="Guid.CompareTo(Guid)"/>, whose field-wise semantics are a .NET detail; the
		/// wire bytes are what every party can agree on.
		///
		/// Includes traits with no registered definition. They are trait values like any other,
		/// holding a code awaiting a schema.
		/// </remarks>
		public IReadOnlyList<TraitValue> TraitValues => _traitValues;

		/// <summary>
		/// The number of traits.
		/// </summary>
		public int Count => _traitValues.Length;
		#endregion

		#region methods
		/// <summary>
		/// Gets the list of trait identifiers, in canonical order.
		/// </summary>
		public IList<Guid> GetTraitIds()
		{
			return _traitValues.Select(traitValue => traitValue.TraitId).ToList();
		}

		/// <summary>
		/// Checks whether a trait is present.
		/// </summary>
		/// <remarks>
		/// A trait is present or absent, and nothing in between. Absence is how a subject says no.
		/// </remarks>
		public bool HasTrait(AttributeFriendlyGuid traitId)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			return _byTraitId.ContainsKey(traitId);
		}

		/// <summary>
		/// Gets the uninterpreted code answering a trait.
		/// </summary>
		/// <remarks>
		/// Works whether or not the trait has a registered definition, which is what makes
		/// round-tripping an unrecognised trait possible.
		/// </remarks>
		public bool TryGetTraitCode(AttributeFriendlyGuid traitId, out Code code)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			return _byTraitId.TryGetValue(traitId, out code);
		}

		/// <summary>
		/// Interprets a trait, unpacking its code under the schema its trait identifier fixes.
		/// </summary>
		/// <typeparam name="M">The model type.</typeparam>
		/// <param name="traitId">The trait identifier.</param>
		/// <param name="piedPiper">The pied piper, which knows the trait and its schema.</param>
		/// <returns>The trait's value.</returns>
		public M GetTrait<M>(AttributeFriendlyGuid traitId, IPiedPiper piedPiper)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			if (!_byTraitId.TryGetValue(traitId, out Code code))
				throw new KeyNotFoundException($"Trait '{traitId}' is not present.");

			return UnpackTrait<M>(traitId, code, piedPiper);
		}

		/// <summary>
		/// Interprets a trait if it is present.
		/// </summary>
		/// <typeparam name="M">The model type.</typeparam>
		/// <param name="traitId">The trait identifier.</param>
		/// <param name="piedPiper">The pied piper, which knows the trait and its schema.</param>
		/// <param name="value">The trait's value.</param>
		/// <returns>True if the trait is present, otherwise false.</returns>
		public bool TryGetTrait<M>(AttributeFriendlyGuid traitId, IPiedPiper piedPiper, out M value)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			value = default;
			if (!_byTraitId.TryGetValue(traitId, out Code code))
				return false;

			value = UnpackTrait<M>(traitId, code, piedPiper);
			return true;
		}

		/// <summary>
		/// Creates a builder seeded with this flex datum's traits.
		/// </summary>
		/// <remarks>
		/// This is how a reader revises one answer and writes out a new record while carrying
		/// every entry it did not understand across unchanged. Without that, the act of updating
		/// destroys whatever a better-informed writer had recorded, and a reader with an older
		/// vocabulary becomes a hazard to everyone with a newer one.
		/// </remarks>
		public FlexDatumBuilder ToBuilder(IPiedPiper piedPiper)
		{
			return new FlexDatumBuilder(piedPiper, _traitValues);
		}
		#endregion

		#region IEquatable<FlexDatum> methods
		public bool Equals(FlexDatum other)
		{
			if (other == null)
				return false;

			if (_traitValues.Length != other._traitValues.Length)
				return false;

			// Canonical order makes this a straight walk rather than a lookup per trait.
			for (int i = 0; i < _traitValues.Length; ++i)
			{
				if (!_traitValues[i].Equals(other._traitValues[i]))
					return false;
			}

			return true;
		}
		#endregion

		#region object methods
		public override bool Equals(object obj)
		{
			return Equals(obj as FlexDatum);
		}

		public override int GetHashCode()
		{
			// Immutable, so worth caching: hashing walks every trait.
			if (!_hasHashCode)
			{
				unchecked
				{
					int hashCode = _traitValues.Length;
					for (int i = 0; i < _traitValues.Length; ++i)
						hashCode = (hashCode * 31) + _traitValues[i].GetHashCode();

					_hashCode = hashCode;
				}
				_hasHashCode = true;
			}

			return _hashCode;
		}

		public static bool operator ==(FlexDatum left, FlexDatum right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			return left.Equals(right);
		}

		public static bool operator !=(FlexDatum left, FlexDatum right)
		{
			return !(left == right);
		}
		#endregion

		#region private functions
		private static M UnpackTrait<M>(Guid traitId, Code code, IPiedPiper piedPiper)
		{
			Guid schemaId = piedPiper.GetTrait(traitId).SchemaId;
			object model = piedPiper.UnpackModel(code, schemaId);

			if (model is M typedModel)
				return typedModel;

			// A trait packed through an integral schema comes back boxed as that integral type,
			// so a boxed int is not `is M` against an enum M even when the enum's underlying type
			// is int.
			Type modelType = typeof(M);
			if (model != null && modelType.IsEnum && model.GetType() == Enum.GetUnderlyingType(modelType))
				return (M) Enum.ToObject(modelType, model);

			throw new InvalidOperationException(
				$"Trait '{traitId}' exists but cannot be cast to type '{typeof(M).Name}'. " +
				$"Actual type: '{(model != null ? model.GetType().Name : "null")}'."
			);
		}
		#endregion

		#region private classes
		/// <summary>
		/// Orders trait identifiers by their wire bytes, which is an agreement every party can
		/// keep, unlike <see cref="Guid.CompareTo(Guid)"/>.
		/// </summary>
		private class TraitIdComparer : IComparer<Guid>
		{
			public static readonly TraitIdComparer Instance = new TraitIdComparer();

			public int Compare(Guid left, Guid right)
			{
				byte[] leftBytes = left.ToByteArray();
				byte[] rightBytes = right.ToByteArray();

				for (int i = 0; i < leftBytes.Length; ++i)
				{
					int difference = leftBytes[i].CompareTo(rightBytes[i]);
					if (difference != 0)
						return difference;
				}

				return 0;
			}
		}
		#endregion
	}
}
