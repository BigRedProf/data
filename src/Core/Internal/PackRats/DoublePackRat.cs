using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class DoublePackRat : PackRat<double>
	{
		#region constructos
		public DoublePackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, double model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(BitConverter.GetBytes(model));
			stream.WriteBits(code);
		}

		public override double UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(64);
			double model = BitConverter.ToDouble(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
