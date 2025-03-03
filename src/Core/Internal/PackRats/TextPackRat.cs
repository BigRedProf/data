using System;
using System.Text;

namespace BigRedProf.Data.Internal.PackRats
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
		public void PackModelWithoutLength(CodeWriter writer, string model)
		{
			byte[] bytes = Encoding.GetBytes(model);
			PackTextBytes(writer, bytes);
		}

		public string UnpackModelWithoutLength(CodeReader reader, int lengthOfEncodedBytes)
		{
			byte[] bytes = UnpackTextBytes(reader, lengthOfEncodedBytes);
			return Encoding.GetString(bytes);
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, string model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			byte[] bytes = Encoding.GetBytes(model);
			PiedPiper.PackModel(writer, bytes.Length, CoreSchema.EfficientWholeNumber31);
			PackTextBytes(writer, bytes);
		}

		public override string UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			string model;
			int byteCount = PiedPiper.GetPackRat<int>(CoreSchema.EfficientWholeNumber31).UnpackModel(reader);
			if (byteCount == 0)
			{
				model = string.Empty;
			}
			else
			{
				byte[] textBytes = UnpackTextBytes(reader, byteCount);
				model = Encoding.GetString(textBytes);
			}

			return model;
		}
		#endregion

		#region private methods
		private void PackTextBytes(CodeWriter writer, byte[] bytes)
		{
			if (bytes.LongLength != 0)
			{
				Code code = new Code(bytes);
				writer.AlignToNextByteBoundary();
				writer.WriteCode(code);
			}
		}

		private byte[] UnpackTextBytes(CodeReader reader, int length)
		{
			reader.AlignToNextByteBoundary();
			Code code = reader.Read(length * 8);
			return code.ByteArray;
		}
		#endregion
	}
}
