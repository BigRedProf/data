using System;

namespace BigRedProf.Data.Core.Internal
{
	internal class UnsignedVarIntPackRat : PackRat<int>
	{
		#region constructors
		public UnsignedVarIntPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, int model)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (model < 0)
				throw new ArgumentOutOfRangeException(nameof(model), "Varint must be non-negative.");

			while (model >= 0x80)
			{
				byte b = (byte)((model & 0x7F) | 0x80);
				PiedPiper.PackModel<byte>(stream, b, CoreSchema.Byte);
				model >>= 7;
			}

			PiedPiper.PackModel<byte>(stream, (byte)model, CoreSchema.Byte);
		}

		public override int UnpackModel(BitStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			int value = 0;
			int shift = 0;

			while (true)
			{
				byte b = PiedPiper.UnpackModel<byte>(stream, CoreSchema.Byte);

				value |= (b & 0x7F) << shift;

				if ((b & 0x80) == 0)
					break;

				shift += 7;

				if (shift >= 35)
					throw new InvalidOperationException("Varint too long.");
			}

			return value;
		}
		#endregion
	}
}
