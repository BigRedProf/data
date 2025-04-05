using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class BytePackRat : PackRat<byte>
	{
		#region constructors
		public BytePackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, byte model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(new byte[] { model });
			writer.WriteCode(code);
		}

		public override byte UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(8);
			byte model = code.ByteArray[0];
			return model;
		}
		#endregion
	}
}
