using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// Represents a model with its schema. This can be useful when you store multiple models
	/// together and won't otherwise know what schema each is.
	/// </summary>
	public class ModelWithSchema : IEquatable<ModelWithSchema>
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

		#region IEquatable<ModelWithSchema> methods
		public bool Equals(ModelWithSchema other)
		{
			if (other == null)
				return false;

			if (SchemaId != other.SchemaId)
				return false;

			if (Model == null && other.Model != null)
				return false;

			if (Model != null && other.Model == null)
				return false;

			if (Model != null && !Model.Equals(other.Model))
				return false;

			return true;
		}
		#endregion

		#region object methods
		public override bool Equals(object obj)
		{
			return Equals(obj as ModelWithSchema);
		}

		public override int GetHashCode()
		{
			int hashSchemaId = SchemaId == null ? 0 : SchemaId.GetHashCode();
			int hashModel = Model == null ? 0 : Model.GetHashCode();
			return hashSchemaId ^ hashModel;
		}

		public static bool operator ==(ModelWithSchema left, ModelWithSchema right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			return left.Equals(right);
		}

		public static bool operator !=(ModelWithSchema left, ModelWithSchema right)
		{
			return !(left == right);
		}
		#endregion
	}
}
