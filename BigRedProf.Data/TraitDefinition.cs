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

		public TraitDefinition(string traitId, string schemaId)
		{
			TraitId = traitId;
			SchemaId = schemaId;
		}
		#endregion

		#region properties
		/// <summary>
		/// The trait identifier.
		/// </summary>
		public string TraitId
		{
			get;
			set;
		}

		/// <summary>
		/// The schema identifier.
		/// </summary>
		public string SchemaId
		{
			get;
			set;
		}
		#endregion
	}
}
