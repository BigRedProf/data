using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class GuidPackRat : PackRat<Guid>
	{
		#region constructos
		public GuidPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, Guid model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(model.ToByteArray());
			stream.WriteBits(code);
		}

		public override Guid UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(128);
			Guid model = new Guid(code.ByteArray);
			return model;
		}
		#endregion
	}
}
