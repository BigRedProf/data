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
		public override void PackModel(CodeWriter writer, int model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (model < 0)
				throw new ArgumentOutOfRangeException(nameof(model), "Varint must be non-negative.");

			while (model >= 0x80)
			{
				byte b = (byte)((model & 0x7F) | 0x80);
				PiedPiper.PackModel<byte>(writer, b, CoreSchema.Byte);
				model >>= 7;
			}

			PiedPiper.PackModel<byte>(writer, (byte)model, CoreSchema.Byte);
		}

		public override int UnpackModel(CodeReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			int value = 0;
			int shift = 0;

			while (true)
			{
				byte b = PiedPiper.UnpackModel<byte>(reader, CoreSchema.Byte);

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
