using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// Represents a model with its schema. This can be useful when you store multiple models
	/// together and won't otherwise know what schema each is.
	/// </summary>
	public class ModelWithSchema
	{
		#region properties
		/// <summary>
		/// The schema identifier.
		/// </summary>
		public string SchemaId { get; set; }

		/// <summary>
		/// The model.
		/// </summary>
		public object Model { get; set; }
		#endregion
	}
}
