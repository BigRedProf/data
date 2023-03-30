﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
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
		}

		/// <summary>
		/// The schema identifier.
		/// </summary>
		public string SchemaId
		{
			get; 
		}

		/// <summary>
		/// Whether or not to align the code writer to a byte boundary
		/// before packing this field.
		/// </summary>
		public ByteAligned ByteAligned
		{
			get;
		}
		#endregion
	}
}
