using System;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A trait identifier together with a code answering that trait. The trait's schema is what
	/// makes that code a datum.
	/// </summary>
	/// <remarks>
	/// This holds a code rather than a datum on purpose. The schema is already fixed by the trait
	/// identifier, so carrying a datum alongside would state it twice and admit ill-formed pairs.
	/// More importantly, it would make an unrecognised entry impossible to describe: you cannot
	/// form a datum without knowing the schema, so a reader holding a flex datum full of
	/// questions it does not recognise would be holding things that were not trait values at all.
	///
	/// As codes, they are perfectly ordinary trait values that simply have not been interpreted.
	/// </remarks>
	public class TraitValue : IEquatable<TraitValue>
	{
		#region fields
		private readonly Guid _traitId;
		private readonly Code _code;
		#endregion

		#region constructors
		public TraitValue(AttributeFriendlyGuid traitId, Code code)
		{
			if (traitId == null)
				throw new ArgumentNullException(nameof(traitId));

			if (code == null)
				throw new ArgumentNullException(nameof(code));

			_traitId = traitId;
			_code = code;
		}
		#endregion

		#region properties
		/// <summary>
		/// The trait identifier.
		/// </summary>
		public Guid TraitId => _traitId;

		/// <summary>
		/// The code answering the trait.
		/// </summary>
		public Code Code => _code;
		#endregion

		#region IEquatable<TraitValue> methods
		public bool Equals(TraitValue? other)
		{
			if (other == null)
				return false;

			return _traitId == other._traitId && _code == other._code;
		}
		#endregion

		#region object methods
		public override bool Equals(object? obj)
		{
			return Equals(obj as TraitValue);
		}

		public override int GetHashCode()
		{
			return _traitId.GetHashCode() ^ _code.GetHashCode();
		}

		public override string ToString()
		{
			return $"{_traitId}={_code}";
		}
		#endregion
	}
}
