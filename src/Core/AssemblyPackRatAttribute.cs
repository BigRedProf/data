using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Identifies a <see cref="PackRat"/> as needing to be registered when 
	/// <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/> is called.
	/// </summary>
	/// <remarks>
	/// For pack rats created using the pack rat compiler, this attribute gets applied automatically
	/// to any pack rats generated from models decorated with <see cref="GeneratePackRatAttribute"/>.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class)]
	public class AssemblyPackRatAttribute : Attribute
	{
		#region constructors
		public AssemblyPackRatAttribute(string schemaId)
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
