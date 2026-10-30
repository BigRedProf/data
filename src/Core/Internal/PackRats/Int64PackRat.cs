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
		public override void PackModel(BitStream stream, long model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(BitConverter.GetBytes(model));
			stream.WriteBits(code);
		}

		public override long UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(64);
			long model = BitConverter.ToInt64(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
