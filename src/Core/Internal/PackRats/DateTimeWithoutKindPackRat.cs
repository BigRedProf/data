using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class DateTimeWithoutKindPackRat : PackRat<DateTime>
	{
		#region constructos
		public DateTimeWithoutKindPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, DateTime model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = new Code(BitConverter.GetBytes(model.Ticks));
			stream.WriteBits(code);
		}

		public override DateTime UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(64);
			long ticks = BitConverter.ToInt64(code.ByteArray, 0);
			DateTime model = new DateTime(ticks);
			return model;
		}
		#endregion
	}
}
