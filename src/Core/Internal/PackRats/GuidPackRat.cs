using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class GuidPackRat : PackRat<Guid>
	{
		#region constructos
		public GuidPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, Guid model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(model.ToByteArray());
			writer.WriteCode(code);
		}

		public override Guid UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(128);
			Guid model = new Guid(code.ByteArray);
			return model;
		}
		#endregion
	}
}
