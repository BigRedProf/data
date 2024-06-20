﻿namespace BigRedProf.Data
{
	/// <summary>
	/// A trait represents a model with a specific purpose or intent.
	/// </summary>
	/// <typeparam name="M">The type of model.</typeparam>
	public class Trait<M>
	{
		#region constructors
		public Trait()
		{
		}

		public Trait(string traitId, M model)
		{
			TraitId = traitId;
			Model = model;
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
		/// The model.
		/// </summary>
		public M Model
		{
			get;
			set;
		}
		#endregion
	}
}