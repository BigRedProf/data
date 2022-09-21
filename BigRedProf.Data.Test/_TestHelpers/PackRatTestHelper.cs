using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal static class PackRatTestHelper
	{
		public static IPiedPiper GetPiedPiper()
		{
			PiedPiper piedPiper = new PiedPiper();
			
			EfficientWholeNumber31PackRat efficientWholeNumber31PackRat = new EfficientWholeNumber31PackRat(piedPiper);
			piedPiper.RegisterPackRat(efficientWholeNumber31PackRat, SchemaId.EfficientWholeNumber31);

			return piedPiper;
		}

		public static void TestPackModel<M>(PackRat<M> packRat, M model, Code expectedCode)
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			packRat.PackModel(writer, model);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			int expectedStreamLength = (expectedCode.Length / 8) + ((expectedCode.Length % 8) > 0 ? 1 : 0);
			Assert.Equal(expectedStreamLength, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
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
