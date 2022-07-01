using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class StringPackRat : PackRat<string>
	{
		#region PackRat methods
		public override void PackModel(CodeWriter writer, string model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			byte[] bytes = Encoding.UTF8.GetBytes(model);
			// TODO: write the length of bytes
			if(bytes.LongLength != 0)
			{
				Code code = new Code(bytes);
				writer.AlignToNextByteBoundary();
				writer.WriteCode(code);
			}	
		}

		public override string UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			// TODO: read the length of bytes
			Code code = reader.Read(1);
			string model = Encoding.UTF8.GetString(code.ByteArray);
			return model;
		}
		#endregion
	}
}
