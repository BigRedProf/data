using BigRedProf.Data.Core.PackRats;
using System;
using System.Text;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class TextPackRat : PackRat<string>
	{
		#region constructors
		public TextPackRat(IPiedPiper piedPiper, Encoding encoding)
			: base(piedPiper)
		{
			Encoding = encoding;
		}
		#endregion

		#region properties
		public Encoding Encoding
		{
			get;
			private set;
		}
		#endregion

		#region methods
		public void PackModelWithoutLength(BitStream stream, string model)
		{
			byte[] bytes = Encoding.GetBytes(model);
			TextPackRatHelper.PackTextBytes(stream, bytes);
		}

		public string UnpackModelWithoutLength(BitStream stream, int lengthOfEncodedBytes)
		{
			byte[] bytes = TextPackRatHelper.UnpackTextBytes(stream, lengthOfEncodedBytes);
			return Encoding.GetString(bytes);
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, string model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			byte[] bytes = Encoding.GetBytes(model);
			PiedPiper.PackModel(stream, bytes.Length, CoreSchema.EfficientWholeNumber31);
			TextPackRatHelper.PackTextBytes(stream, bytes);
		}

		public override string UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			string model;
			int byteCount = PiedPiper.GetPackRat<int>(CoreSchema.EfficientWholeNumber31).UnpackModel(stream);
			if (byteCount == 0)
			{
				model = string.Empty;
			}
			else
			{
				byte[] textBytes = TextPackRatHelper.UnpackTextBytes(stream, byteCount);
				model = Encoding.GetString(textBytes);
			}

			return model;
		}
		#endregion
	}
}
