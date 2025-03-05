using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Instructs the pack rat compiler to pack this field in the specified order using the specified
	/// schema.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PackFieldAttribute : Attribute
	{
		#region constructors
		public PackFieldAttribute(int position, string schemaId)
			: this(position, schemaId, ByteAligned.No)
		{
			IsNullable = false;
		}

		public PackFieldAttribute(int position, string schemaId, ByteAligned byteAligned)			
		{
			Position = position;
			SchemaId = schemaId;
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
			private set;
		}

		/// <summary>
		/// The schema identifier.
		/// </summary>
		public string SchemaId
		{
			get; 
			private set;
		}

		/// <summary>
		/// Whether or not to align the code writer to a byte boundary
		/// before packing this field.
		/// </summary>
		public ByteAligned ByteAligned
		{
			get; 
			private set;
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
		public bool IsNullable
		{
			get;
			set;
		}
		#endregion
	}
}
