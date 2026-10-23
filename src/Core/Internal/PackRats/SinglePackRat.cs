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
		public override void PackModel(BitStream stream, float model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(BitConverter.GetBytes(model));
			stream.WriteBits(code);
		}

		public override float UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(32);
			float model = BitConverter.ToSingle(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
