using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class ModelWithSchemaPackRat : PackRat<ModelWithSchema>
	{
		#region constructors
		public ModelWithSchemaPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, ModelWithSchema model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Guid schemaId;
			if(!Guid.TryParse(model.SchemaId, out schemaId))
				throw new ArgumentException("The Schema Identifier is not a valid GUID.", nameof(model));

			PiedPiper.PackModel<Guid>(writer, schemaId, SchemaId.Guid);
			PiedPiper.PackModel(writer, model.Model, model.SchemaId);
		}

		public override ModelWithSchema UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Guid schemaIdAsGuid = PiedPiper.UnpackModel<Guid>(reader, SchemaId.Guid);
			string schemaId = schemaIdAsGuid.ToString();
			object model = PiedPiper.UnpackModel(reader, schemaId);

			return new ModelWithSchema()
			{
				SchemaId = schemaId,
				Model = model
			};
		}
		#endregion
	}
}
