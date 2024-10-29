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
		}

		public override DateTime UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(64);
			long ticks = BitConverter.ToInt64(code.ByteArray, 0);
			DateTime model = new DateTime(ticks);
			return model;
		}
		#endregion
	}
}
