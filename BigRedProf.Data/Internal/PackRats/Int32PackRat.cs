using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class Int32PackRat : PackRat<int>
	{
		#region PackRat methods
		public override void PackModel(CodeWriter writer, int model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(BitConverter.GetBytes(model));
			writer.WriteCode(code);
		}

		public override int UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(32);
			int model = BitConverter.ToInt32(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
