using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal static class PackRatTestHelper
	{
		public static void TestPackModel<M>(PackRat<M> packRat, M model, Code expectedCode)
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			packRat.PackModel(writer, model);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(expectedCode.Length % 8, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
		}

		public static void TestUnpackModel<M>(PackRat<M> packRat, Code code, M expectedModel)
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);

			M actualModel = packRat.UnpackModel(reader);

			Assert.Equal<M>(expectedModel, actualModel);
		}
	}
}
