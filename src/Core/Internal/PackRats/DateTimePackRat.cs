using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class DateTimePackRat : PackRat<DateTime>
	{
		#region constructos
		public DateTimePackRat(IPiedPiper piedPiper)
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

			if (model.Kind == DateTimeKind.Unspecified)
				stream.WriteBits("00");
			else if (model.Kind == DateTimeKind.Utc)
				stream.WriteBits("01");
			else if (model.Kind == DateTimeKind.Local)
				stream.WriteBits("10");
			else
				throw new InvalidOperationException("Invalid DateTimeKind.");
		}

		public override DateTime UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(64);
			long ticks = BitConverter.ToInt64(code.ByteArray, 0);

			DateTimeKind kind;
			code = stream.ReadBits(2);
			if(code == "00")
				kind = DateTimeKind.Unspecified;
			else if(code == "01")
				kind = DateTimeKind.Utc;
			else if (code == "10")
				kind = DateTimeKind.Local;
			else
				throw new InvalidOperationException("Invalid DateTimeKind code.");

			DateTime model = new DateTime(ticks, kind);
			return model;
		}
		#endregion
	}
}
