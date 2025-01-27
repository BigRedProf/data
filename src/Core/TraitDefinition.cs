using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// A trait definition defines a type of trait.
	/// </summary>
	public class TraitDefinition
	{
		#region constructors
		public TraitDefinition()
		{
		}

		public TraitDefinition(AttributeFriendlyGuid traitId, AttributeFriendlyGuid schemaId)
		{
			TraitId = traitId;
			SchemaId = schemaId;
		}
		#endregion

		#region properties
		/// <summary>
		/// The trait identifier.
		/// </summary>
		public Guid TraitId
		{
			get;
			set;
		}

		/// <summary>
		/// The schema identifier.
		/// </summary>
		public Guid SchemaId
		{
			get;
			set;
		}
		#endregion
	}
}
