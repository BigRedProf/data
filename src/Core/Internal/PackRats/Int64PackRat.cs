using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class Int64PackRat : PackRat<long>
	{
		#region constructos
		public Int64PackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, long model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(BitConverter.GetBytes(model));
			writer.WriteCode(code);
		}

		public override long UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(64);
			long model = BitConverter.ToInt64(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
