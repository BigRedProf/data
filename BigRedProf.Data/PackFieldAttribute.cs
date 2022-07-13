using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Instructs the pack rat compiler to pack this field in the specified order using the specified
	/// schema.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PackFieldAttribute : Attribute
	{
		#region constructors
		public PackFieldAttribute(int position, string schemaId)
		{
			Position = position;
			SchemaId = schemaId;
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
		#endregion
	}
}
