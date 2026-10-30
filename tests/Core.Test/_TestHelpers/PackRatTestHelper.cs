using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal static class PackRatTestHelper
	{
		#region functions
		public static IPiedPiper GetPiedPiper()
		{
			PiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			return piedPiper;
		}

		public static void TestPackModel<M>(PackRat<M> packRat, M model, Code expectedCode)
		{
			CodeTester codeTester = new CodeTester();

			// Use these "alignments messes" to help ensure we account for:
			// 1. Byte alignment
			// 2. Correct length ("010" != "01011111")
			Code expectedAlignmentMess1 = "10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 10";

			codeTester.Write(expectedAlignmentMess1);

			// Pack into an exact-length code so the surrounding alignment is undisturbed.
			Code actualCode;
			using (CodeBitStream stream = new CodeBitStream())
			{
				packRat.PackModel(stream, model);
				actualCode = stream.ToCode();
			}
			codeTester.Writer.WriteBits(actualCode);
			
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		public static void TestUnpackModel<M>(PackRat<M> packRat, Code code, M expectedModel)
		{
			CodeTester codeTester = new CodeTester();

			// Unlike in TestPackModel, the first alignment mess must be byte aligned.
			Code expectedAlignmentMess1 = "10101010 10101010";
			Code expectedAlignmentMess2 = "11011011 01101101 10110";

			codeTester.Write(expectedAlignmentMess1);
			codeTester.Write(code);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			M actualModel = packRat.UnpackModel(codeTester.Reader);
			Assert.Equal<M>(expectedModel, actualModel);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		public static void TestUnpackModelCodeOnlyNoEquals<M>(PackRat<M> packRat, Code code)
		{
			CodeTester codeTester = new CodeTester();

			// Unlike in TestPackModel, the first alignment mess must be byte aligned.
			Code expectedAlignmentMess1 = "10101010 10101010";
			Code expectedAlignmentMess2 = "11011011 01101101 10110";

			codeTester.Write(expectedAlignmentMess1);
			codeTester.Write(code);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			M actualModel = packRat.UnpackModel(codeTester.Reader);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		public static void TestPackUnpackModel<M>(PackRat<M> packRat, M originalModel)
		{
			CodeTester codeTester = new CodeTester();

			// Byte-aligned so we're safe on read boundaries
			Code expectedAlignmentMess1 = "10101010 10101010";
			Code expectedAlignmentMess2 = "11011011 01101101 10110";

			codeTester.Write(expectedAlignmentMess1);

			Code packedCode;
			using (CodeBitStream stream = new CodeBitStream())
			{
				packRat.PackModel(stream, originalModel);
				packedCode = stream.ToCode();
			}

			codeTester.Writer.WriteBits(packedCode);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			M unpackedModel = packRat.UnpackModel(codeTester.Reader);
			Assert.Equal(originalModel, unpackedModel);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		public static BitStream CreateBitStream(Code code)
		{
			MemoryStream writerStream = new MemoryStream();
			BitStream writer = new StreamBitWriter(writerStream);
			writer.WriteBits(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			BitStream reader = new StreamBitReader(readerStream);
			return reader;
		}

		public static IEnumerable<string> RepeatString(string s, int count)
		{
			for (int i = 0; i < count; i++)
				yield return s;
		}

		public static string ReverseBytes(string stringOfBytes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] byteStrings = stringOfBytes.Split(' ');

			bool firstByte = true;
			foreach (string byteString in byteStrings)
			{
				if(byteString.Length != 8)
					throw new ArgumentException($"Byte '{byteString}' is not 8 bits long.", nameof(byteString));

				if (firstByte)
					firstByte = false;
				else
					stringBuilder.Append(' ');

				string reversedByteString = ReverseString(byteString);
				stringBuilder.Append(reversedByteString);
			}

			return stringBuilder.ToString();
		}
		#endregion

		#region private functions
		private static string ReverseString(string @string)
		{
			StringBuilder stringBuilder = new StringBuilder(@string.Length);
			for (int i = @string.Length - 1; i >= 0; --i)
				stringBuilder.Append(@string[i]);

			return stringBuilder.ToString();
		}
		#endregion
	}
}
