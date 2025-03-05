using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class SinglePackRat : PackRat<float>
	{
		#region constructos
		public SinglePackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, float model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(BitConverter.GetBytes(model));
			writer.WriteCode(code);
		}

		public override float UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(32);
			float model = BitConverter.ToSingle(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
