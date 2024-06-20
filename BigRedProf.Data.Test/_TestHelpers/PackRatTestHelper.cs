using System.IO;
using System.Reflection;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal static class PackRatTestHelper
	{
		public static IPiedPiper GetPiedPiper()
		{
			PiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

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

			// Pack the model with a new CodeWriter so the byte alignment doesn't get messed up
			//packRat.PackModel(codeTester.Writer, model);
			Code actualCode = null;
			using (MemoryStream stream = new MemoryStream())
			{
				CodeWriter writer = new CodeWriter(stream);
				packRat.PackModel(writer, model);
				actualCode = writer.ToDebugCode();
			}
			codeTester.Writer.WriteCode(actualCode);
			
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

		public static CodeReader CreateCodeReader(Code code)
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			return reader;
		}
	}
}
