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
		public override void PackModel(BitStream stream, byte model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(new byte[] { model });
			stream.WriteBits(code);
		}

		public override byte UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(8);
			byte model = code.ByteArray[0];
			return model;
		}
		#endregion
	}
}
