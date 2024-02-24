using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Instructs the pack rat compiler to generate a <see cref="PackRat"/> for this
	/// model.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class GeneratePackRatAttribute : Attribute
	{
		#region constructors
		public GeneratePackRatAttribute(string schemaId)
		{
			SchemaId = schemaId;
		}
		#endregion

		#region properties
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
