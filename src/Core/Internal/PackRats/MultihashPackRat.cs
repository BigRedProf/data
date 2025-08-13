using System;

namespace BigRedProf.Data.Core.Internal
{
	internal class MultihashPackRat : PackRat<Multihash>
	{
		#region fields
		private readonly PackRat<int> _varIntPackRat;
		private readonly PackRat<byte> _bytePackRat;
		#endregion

		#region constructors
		public MultihashPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			_varIntPackRat = new UnsignedVarIntPackRat(piedPiper);
			_bytePackRat = piedPiper.GetPackRat<byte>(CoreSchema.Byte);
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, Multihash model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			_varIntPackRat.PackModel(writer, (int)model.Algorithm);
			_varIntPackRat.PackModel(writer, model.Digest.Length);

			writer.AlignToNextByteBoundary();
			foreach (var b in model.Digest)
			{
				_bytePackRat.PackModel(writer, b);
			}
		}

		public override Multihash UnpackModel(CodeReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			var algorithmCode = _varIntPackRat.UnpackModel(reader);
			var length = _varIntPackRat.UnpackModel(reader);

			reader.AlignToNextByteBoundary();
			var digest = new byte[length];
			for (int i = 0; i < length; i++)
			{
				digest[i] = _bytePackRat.UnpackModel(reader);
			}

			var algorithm = (MultihashAlgorithm)algorithmCode;
			return new Multihash(digest, algorithm);
		}
		#endregion
	}
}
