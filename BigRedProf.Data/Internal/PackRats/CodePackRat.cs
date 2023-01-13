using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class CodePackRat : PackRat<Code>
	{
		#region constructors
		public CodePackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, Code model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31).PackModel(writer, model.Length);
			writer.AlignToNextByteBoundary();

			writer.WriteCode(model);
		}

		public override Code UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			int length = PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31).UnpackModel(reader);
			reader.AlignToNextByteBoundary();

			Code model = reader.Read(length);
			
			return model;
		}
		#endregion
	}
}
