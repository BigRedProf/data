using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class VarIntPackRat : PackRat<int>
	{
		#region constructors
		public VarIntPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, int model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			uint value = (uint)model;

			while (value >= 0x80)
			{
				byte nextByte = (byte)((value & 0x7F) | 0x80); // set MSB for continuation
				PiedPiper.PackModel<byte>(writer, ReverseByte(nextByte), CoreSchema.Byte);
				value >>= 7;
			}

			PiedPiper.PackModel<byte>(writer, ReverseByte((byte)value), CoreSchema.Byte); // final byte, MSB = 0
		}

		public override int UnpackModel(CodeReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			int result = 0;
			int shift = 0;

			while (true)
			{
				if (shift > 28) // guard against overflow (5 full groups of 7 bits = 35 bits)
					throw new InvalidOperationException("LEB128 varint is too long for Int32.");

				byte b = ReverseByte(PiedPiper.UnpackModel<byte>(reader, CoreSchema.Byte));
				result |= (b & 0x7F) << shift;

				if ((b & 0x80) == 0)
					break;

				shift += 7;
			}

			return result;
		}
		#endregion

		#region functions
		private static byte ReverseByte(byte b)
		{
			b = (byte)(((b * 0x0802 & 0x22110) | (b * 0x8020 & 0x88440)) * 0x10101 >> 16);
			return b;
		}
		#endregion
	}
}
