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

			// NOTE: We don't have to worry much about byte alignment here because the
			// EfficientWholeNumber31PackRat yields 8-, 16-, or 32-bit numbers for any values above
			// 4. In other words, codes of length 3 or less will fit in 7 bits and should never need to
			// be byte aligned, while codes of length 4 or more will naturally be byte aligned which
			// is essentially for really long codes.

			PiedPiper.GetPackRat<int>(CoreSchema.EfficientWholeNumber31).PackModel(writer, model.Length);
			writer.WriteCode(model);
		}

		public override Code UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			int length = PiedPiper.GetPackRat<int>(CoreSchema.EfficientWholeNumber31).UnpackModel(reader);
			Code model = reader.Read(length);
			
			return model;
		}
		#endregion
	}
}
