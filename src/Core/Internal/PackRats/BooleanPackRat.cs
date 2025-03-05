using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class BooleanPackRat : PackRat<bool>
	{
		#region constructors
		public BooleanPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, bool model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			writer.WriteCode(model ? "1" : "0");
		}

		public override bool UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(1);
			bool model = code == "1" ? true : false;
			return model;
		}
		#endregion
	}
}
