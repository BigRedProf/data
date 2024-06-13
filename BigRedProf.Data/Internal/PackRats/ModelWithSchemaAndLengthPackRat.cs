using System;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class ModelWithSchemaAndLengthPackRat : PackRat<ModelWithSchemaAndLength>
	{
		#region constructors
		public ModelWithSchemaAndLengthPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, ModelWithSchemaAndLength model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Guid schemaId;
			if(!Guid.TryParse(model.SchemaId, out schemaId))
				throw new ArgumentException("The Schema Identifier is not a valid GUID.", nameof(model));

			PiedPiper.PackModel<Guid>(writer, schemaId, SchemaId.Guid);
			PiedPiper.PackModel(writer, model.Length, SchemaId.EfficientWholeNumber31);
			PiedPiper.PackModel(writer, model.Model, model.SchemaId);
		}

		public override ModelWithSchemaAndLength UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Guid schemaIdAsGuid = PiedPiper.UnpackModel<Guid>(reader, SchemaId.Guid);
			string schemaId = schemaIdAsGuid.ToString();
			int length = PiedPiper.UnpackModel<int>(reader, SchemaId.EfficientWholeNumber31);
			object model = PiedPiper.UnpackModel(reader, schemaId);

			return new ModelWithSchemaAndLength()
			{
				SchemaId = schemaId,
				Length = length,
				Model = model
			};
		}
		#endregion
	}
}
