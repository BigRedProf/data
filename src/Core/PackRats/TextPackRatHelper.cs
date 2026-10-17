using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core.PackRats
{
	public class TextPackRatHelper
	{
		#region functions
		public static void PackTextBytes(BitStream stream, byte[] bytes)
		{
			if (bytes.LongLength != 0)
			{
				Code code = new Code(bytes);
				stream.AlignToNextByteBoundary();
				stream.WriteBits(code);
			}
		}

		public static byte[] UnpackTextBytes(BitStream stream, int length)
		{
			stream.AlignToNextByteBoundary();
			Code code = stream.ReadBits(length * 8);
			return code.ByteArray;
		}

		public static void PackTextWithoutLength(BitStream stream, string text, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(text);
			PackTextBytes(stream, bytes);
		}

		public static string UnpackTextWithoutLength(BitStream stream, int lengthInBytes, Encoding encoding)
		{
			byte[] bytes = UnpackTextBytes(stream, lengthInBytes);
			return encoding.GetString(bytes);
		}
		#endregion
	}
}
