using BigRedProf.Data.Internal.PackRats;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class BooleanPackRatTests
	{
		#region PackRat methods
		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldThrowWhenWriterIsNull()
		{
			BooleanPackRat packRat = new BooleanPackRat();
			bool model = false;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.PackModel(null, model);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForTrue()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			BooleanPackRat packRat = new BooleanPackRat();

			packRat.PackModel(writer, true);
			
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Code code = reader.Read(1);
			Assert.Equal(1, readerStream.Length);
			Assert.Equal<Code>("1", code);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void PackModel_ShouldWorkForFalse()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			BooleanPackRat packRat = new BooleanPackRat();

			packRat.PackModel(writer, false);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Code code = reader.Read(1);
			Assert.Equal(1, readerStream.Length);
			Assert.Equal<Code>("0", code);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldThrowWhenReaderIsNull()
		{
			BooleanPackRat packRat = new BooleanPackRat();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRat.UnpackModel(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForTrue()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode("1");
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			BooleanPackRat packRat = new BooleanPackRat();

			bool model = packRat.UnpackModel(reader);

			Assert.True(model);
		}

		[Fact]
		[Trait("Region", "PackRat methods")]
		public void UnpackModel_ShouldWorkForFalse()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode("0");
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			BooleanPackRat packRat = new BooleanPackRat();

			bool model = packRat.UnpackModel(reader);

			Assert.False(model);
		}
		#endregion
	}
}
