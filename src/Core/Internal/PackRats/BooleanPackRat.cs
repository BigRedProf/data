using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class BooleanPackRat : PackRat<bool>
	{
		#region constructors
		public BooleanPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, bool model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			stream.WriteBits(model ? "1" : "0");
		}

		public override bool UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			Code code = stream.ReadBits(1);
			bool model = code == "1" ? true : false;
			return model;
		}
		#endregion
	}
}
