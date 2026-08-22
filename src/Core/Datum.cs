using System;
using BigRedProf.Data.Core.Internal;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A <see cref="Code"/> together with the schema under which that code is to be read.
	/// </summary>
	/// <remarks>
	/// A datum says how its code is *to be* read. It does not promise that you are able to read
	/// it. Someone who has never heard of the schema still holds the datum entire: they can carry
	/// it, copy it, count it, take a fingerprint of it, and hand it to someone who knows the
	/// agreement. What they cannot do is interpret it.
	///
	/// So interpretation is something a reader brings to a datum, never something stored inside
	/// one. A datum holds a code, never a decoded model; call <see cref="Unpack{M}(IPiedPiper)"/>
	/// when you want the model, and only then does a pack rat need to exist for the schema.
	/// </remarks>
	public class Datum : IEquatable<Datum>
	{
		#region fields
		private readonly Guid _schemaId;
		private readonly Code _code;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a datum.
		/// </summary>
		/// <param name="schemaId">The schema identifier.</param>
		/// <param name="code">The code.</param>
		public Datum(AttributeFriendlyGuid schemaId, Code code)
		{
			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (code == null)
				throw new ArgumentNullException(nameof(code));

			_schemaId = schemaId;
			_code = code;
		}
		#endregion

		#region properties
		/// <summary>
		/// The schema identifier.
		/// </summary>
		public Guid SchemaId => _schemaId;

		/// <summary>
		/// The code.
		/// </summary>
		public Code Code => _code;
		#endregion

		#region methods
		/// <summary>
		/// Interprets this datum, unpacking its code under its schema.
		/// </summary>
		/// <remarks>
		/// The unpack itself is weakly typed, because the model type is the caller's question
		/// rather than the datum's: a reader that does not know what a datum represents can ask
		/// for <see cref="object"/> and find out afterwards, which is the ordinary case when a
		/// datum arrives from somewhere. Asking for a specific type still works, and so does
		/// asking for an enum whose schema is integral.
		/// </remarks>
		/// <typeparam name="M">The type the caller wants the model as.</typeparam>
		/// <param name="piedPiper">The pied piper.</param>
		/// <returns>The model this datum represents.</returns>
		public M Unpack<M>(IPiedPiper piedPiper)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			object model = piedPiper.UnpackModel(_code, _schemaId);
			if (ModelCoercion.TryCoerce<M>(model, out M coerced))
				return coerced;

			throw new InvalidOperationException(
				$"This datum's model cannot be presented as type '{typeof(M).Name}'. " +
				$"Actual type: '{ModelCoercion.DescribeType(model)}'."
			);
		}
		#endregion

		#region IEquatable<Datum> methods
		public bool Equals(Datum? other)
		{
			if (other == null)
				return false;

			return _schemaId == other._schemaId && _code == other._code;
		}
		#endregion

		#region object methods
		public override bool Equals(object? obj)
		{
			return Equals(obj as Datum);
		}

		public override int GetHashCode()
		{
			return _schemaId.GetHashCode() ^ _code.GetHashCode();
		}

		public override string ToString()
		{
			return $"{_schemaId}:{_code}";
		}

		public static bool operator ==(Datum? left, Datum? right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			return left.Equals(right);
		}

		public static bool operator !=(Datum? left, Datum? right)
		{
			return !(left == right);
		}
		#endregion
	}
}
