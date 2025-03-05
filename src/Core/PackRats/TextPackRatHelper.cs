using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.PackRats
{
	public class TextPackRatHelper
	{
		#region functions
		public static void PackTextBytes(CodeWriter writer, byte[] bytes)
		{
			if (bytes.LongLength != 0)
			{
				Code code = new Code(bytes);
				writer.AlignToNextByteBoundary();
				writer.WriteCode(code);
			}
		}

		public static byte[] UnpackTextBytes(CodeReader reader, int length)
		{
			reader.AlignToNextByteBoundary();
			Code code = reader.Read(length * 8);
			return code.ByteArray;
		}

		public static void PackTextWithoutLength(CodeWriter writer, string text, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(text);
			PackTextBytes(writer, bytes);
		}

		public static string UnpackTextWithoutLength(CodeReader reader, int lengthInBytes, Encoding encoding)
		{
			byte[] bytes = UnpackTextBytes(reader, lengthInBytes);
			return encoding.GetString(bytes);
		}
		#endregion
	}
}
