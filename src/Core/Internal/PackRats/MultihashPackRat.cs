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
		public override void PackModel(BitStream stream, Multihash model)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			_varIntPackRat.PackModel(stream, (int)model.Algorithm);
			_varIntPackRat.PackModel(stream, model.Digest.Length);

			stream.AlignToNextByteBoundary();
			foreach (var b in model.Digest)
			{
				_bytePackRat.PackModel(stream, b);
			}
		}

		public override Multihash UnpackModel(BitStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			var algorithmCode = _varIntPackRat.UnpackModel(stream);
			var length = _varIntPackRat.UnpackModel(stream);

			stream.AlignToNextByteBoundary();
			var digest = new byte[length];
			for (int i = 0; i < length; i++)
			{
				digest[i] = _bytePackRat.UnpackModel(stream);
			}

			var algorithm = (MultihashAlgorithm)algorithmCode;
			return new Multihash(digest, algorithm);
		}
		#endregion
	}
}
