using System;

namespace BigRedProf.Data.Internal.PackRats
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
		public override void PackModel(CodeWriter writer, double model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code code = new Code(BitConverter.GetBytes(model));
			writer.WriteCode(code);
		}

		public override double UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			Code code = reader.Read(64);
			double model = BitConverter.ToDouble(code.ByteArray, 0);
			return model;
		}
		#endregion
	}
}
