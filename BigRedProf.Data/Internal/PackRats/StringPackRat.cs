using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class StringPackRat : PackRat<string>
	{
		#region fields
		private readonly IPiedPiper _piedPier;
		#endregion

		#region constructors
		public StringPackRat(IPiedPiper piedPiper)
		{
			if(piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			_piedPier = piedPiper;
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, string model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			byte[] bytes = Encoding.UTF8.GetBytes(model);
			_piedPier.GetPackRat<int>(SchemaId.EfficientWholeNumber31).PackModel(writer, bytes.Length);
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

			int byteCount = _piedPier.GetPackRat<int>(SchemaId.EfficientWholeNumber31).UnpackModel(reader);
			Code code = reader.Read(byteCount);
			string model = Encoding.UTF8.GetString(code.ByteArray);
			return model;
		}
		#endregion
	}
}
