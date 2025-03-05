using System;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Represents a model with its schema and its length. This can be useful when some clients
	/// won't understand the schema of all models as it allows them to skip over such models.
	/// </summary>
	public class ModelWithSchemaAndLength : IEquatable<ModelWithSchemaAndLength>
	{
		#region properties
		/// <summary>
		/// The schema identifier.
		/// </summary>
		public AttributeFriendlyGuid SchemaId { get; set; }

		/// <summary>
		/// The length of the encoded model, not including the schema identifier, in bits.
		/// </summary>
		public int Length { get; set; }

		/// <summary>
		/// The model.
		/// </summary>
		public object Model { get; set; }
		#endregion

		#region IEquatable<ModelWithSchemaAndLength> methods
		public bool Equals(ModelWithSchemaAndLength other)
		{
			if (other == null)
				return false;

			if (SchemaId != other.SchemaId)
				return false;

			if (Length != other.Length)
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
			int hashLength = Length.GetHashCode();
			int hashModel = Model == null ? 0 : Model.GetHashCode();
			return hashSchemaId ^ hashLength ^ hashModel;
		}

		public static bool operator ==(ModelWithSchemaAndLength left, ModelWithSchemaAndLength right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null) || object.ReferenceEquals(right, null))
				return false;

			return left.Equals(right);
		}

		public static bool operator !=(ModelWithSchemaAndLength left, ModelWithSchemaAndLength right)
		{
			return !(left == right);
		}
		#endregion
	}
}
