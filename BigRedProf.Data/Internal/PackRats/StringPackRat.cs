using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class StringPackRat : PackRat<string>
	{
		#region constructors
		public StringPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, string model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			byte[] bytes = Encoding.UTF8.GetBytes(model);
			PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31).PackModel(writer, bytes.Length);
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

			string model;
			int byteCount = PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31).UnpackModel(reader);
			if (byteCount == 0)
			{
				model = string.Empty;
			}
			else
			{
				Code code = reader.Read(byteCount);
				model = Encoding.UTF8.GetString(code.ByteArray);
			}

			return model;
		}
		#endregion
	}
}
