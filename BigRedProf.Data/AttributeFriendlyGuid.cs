using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// A wrapper class that effectively allows <see cref="Guid"/> objects to be used
	/// as custom attribute parameters.
	/// </summary>
	public struct AttributeFriendlyGuid
	{
		#region fields
		private Guid _guid;
		#endregion

		#region constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeFriendlyGuid"/> struct from a string representation of a GUID.
		/// </summary>
		/// <param name="guidString">The string representation of the GUID.</param>
		public AttributeFriendlyGuid(string guidString)
		{
			_guid = new Guid(guidString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeFriendlyGuid"/> struct from a <see cref="Guid"/> object.
		/// </summary>
		/// <param name="guid">The GUID object.</param>
		public AttributeFriendlyGuid(Guid guid)
		{
			_guid = guid;
		}
		#endregion

		#region operator overloads
		/// <summary>
		/// Converts an <see cref="AttributeFriendlyGuid"/> to a <see cref="Guid"/>.
		/// </summary>
		/// <param name="afGuid">The <see cref="AttributeFriendlyGuid"/> to convert.</param>
		public static implicit operator Guid(AttributeFriendlyGuid afGuid)
		{
			return afGuid._guid;
		}

		/// <summary>
		/// Converts a <see cref="Guid"/> to an <see cref="AttributeFriendlyGuid"/>.
		/// </summary>
		/// <param name="guid">The <see cref="Guid"/> to convert.</param>
		public static implicit operator AttributeFriendlyGuid(Guid guid)
		{
			return new AttributeFriendlyGuid(guid);
		}

		/// <summary>
		/// Converts an <see cref="AttributeFriendlyGuid"/> to a string.
		/// </summary>
		/// <param name="afGuid">The <see cref="AttributeFriendlyGuid"/> to convert.</param>
		public static implicit operator string(AttributeFriendlyGuid afGuid)
		{
			return afGuid._guid.ToString();
		}

		/// <summary>
		/// Converts a string to an <see cref="AttributeFriendlyGuid"/>.
		/// </summary>
		/// <param name="guidString">The string representation of the GUID.</param>
		public static implicit operator AttributeFriendlyGuid(string guidString)
		{
			return new AttributeFriendlyGuid(guidString);
		}
		#endregion

		#region object methods
		/// <summary>
		/// Returns the string representation of the GUID.
		/// </summary>
		/// <returns>A string that represents the GUID.</returns>
		public override string ToString()
		{
			return _guid.ToString();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
		public override bool Equals(object obj)
		{
			if (obj is AttributeFriendlyGuid afGuid)
			{
				return _guid.Equals(afGuid._guid);
			}
			return false;
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return _guid.GetHashCode();
		}
		#endregion
	}
}
