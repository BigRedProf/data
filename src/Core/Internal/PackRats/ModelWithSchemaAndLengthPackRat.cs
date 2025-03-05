using System;

namespace BigRedProf.Data.Core.Internal.PackRats
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

			PiedPiper.PackModel<Guid>(writer, model.SchemaId, CoreSchema.Guid);
			PiedPiper.PackModel(writer, model.Length, CoreSchema.EfficientWholeNumber31);
			PiedPiper.PackModel(writer, model.Model, model.SchemaId);
		}

		public override ModelWithSchemaAndLength UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Guid schemaId = PiedPiper.UnpackModel<Guid>(reader, CoreSchema.Guid);
			int length = PiedPiper.UnpackModel<int>(reader, CoreSchema.EfficientWholeNumber31);
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
