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

			packRat.PackModel(codeTester.Writer, model);

			codeTester.StopWritingAndStartReading();

			Code actualCode = codeTester.Reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
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
