using System;

namespace BigRedProf.Data.Internal.PackRats
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
		public override void PackModel(CodeWriter writer, DateTime model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(BitConverter.GetBytes(model.Ticks));
			writer.WriteCode(code);

			if (model.Kind == DateTimeKind.Unspecified)
				writer.WriteCode("00");
			else if (model.Kind == DateTimeKind.Utc)
				writer.WriteCode("01");
			else if (model.Kind == DateTimeKind.Local)
				writer.WriteCode("10");
			else
				throw new InvalidOperationException("Invalid DateTimeKind.");
		}

		public override DateTime UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(64);
			long ticks = BitConverter.ToInt64(code.ByteArray, 0);

			DateTimeKind kind;
			code = reader.Read(2);
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
