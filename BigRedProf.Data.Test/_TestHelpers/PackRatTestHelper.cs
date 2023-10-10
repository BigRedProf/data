using System.IO;
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
			Code expectedAlignmentMess1 = "10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 10";

			codeTester.Write(expectedAlignmentMess1);
			packRat.PackModel(codeTester.Writer, model);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		public static void TestUnpackModel<M>(PackRat<M> packRat, Code code, M expectedModel)
		{
			CodeReader reader = CreateCodeReader(code);

			M actualModel = packRat.UnpackModel(reader);

			Assert.Equal<M>(expectedModel, actualModel);
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
