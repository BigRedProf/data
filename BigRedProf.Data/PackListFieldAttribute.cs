using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Instructs the pack rat compiler to pack this list field in the specified order using the specified
	/// schema for each element.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class PackListFieldAttribute : Attribute
	{
		#region constructors
		/// <summary>
		/// Creates a new <see cref="PackListFieldAttribute"/>.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="elementSchemaId">The schema identified for each element.</param>
		/// <param name="byteAligned">
		/// Determines the order in which fields are packed. Use 1 for the first field.
		/// </param>
		public PackListFieldAttribute(int position, string elementSchemaId, ByteAligned byteAligned)
		{
			ByteAligned = byteAligned;
		}
		#endregion

		#region properties
		/// <summary>
		/// Determines the order in which fields are packed. Use 1 for the first field.
		/// </summary>
		public int Position
		{
			get;
		}

		/// <summary>
		/// The schema identifier for each element.
		/// </summary>
		public string ElementSchemaId
		{
			get;
		}       
		
		/// <summary>
				/// Whether or not to align the code writer to a byte boundary
				/// before packing each element in the list.
				/// </summary>
		public ByteAligned ByteAligned
		{
			get;
		}

		/// <summary>
		/// Whether or not null values are allowed (and thus an extra bit is encoded 
		/// when packing).
		/// </summary>
		/// <remarks>
		/// If not specified, the <see cref="Nullable"/> attribute (C# question mark
		/// operator) will determine whether or not the object is nullable. But, in
		/// practice, it is often necessary to explicitly set this property because the
		/// C# compiler doesn't allow default constructors with non-nullable
		/// properties.
		/// </remarks>
		public bool? IsNullable
		{
			get;
		}
		#endregion
	}
}
