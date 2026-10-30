using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class Int32PackRat : PackRat<int>
	{
		#region constructos
		public Int32PackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, int model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(BitConverter.GetBytes(model));
			stream.WriteBits(code);
		}

		public override int UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(32);
			int model = BitConverter.ToInt32(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
