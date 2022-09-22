using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class PackRatTests : PackRat<object>
	{
		#region constructors
		public PackRatTests()
			: base(new PiedPiper())
		{
		}
		#endregion

		#region protected methods
		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldThrowWhenWriterIsNull()
		{
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRatTests.PackNullableModel<object>(null, "foo", packRatTests, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldThrowWhenPackRatIsNull()
		{
			CodeWriter writer = new CodeWriter(new MemoryStream());
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRatTests.PackNullableModel<object>(writer, null, null, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNullModels()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			packRatTests.PackNullableModel(writer, null, packRatTests, ByteAligned.No);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(1);
			Assert.Equal("0", actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNonNullModels()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			packRatTests.PackNullableModel(writer, "foo", packRatTests, ByteAligned.No);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(2);
			Assert.Equal("11", actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldThrowWhenReaderIsNull()
		{
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRatTests.UnpackNullableModel<object>(null, packRatTests, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldThrowWhenPackRatIsNull()
		{
			CodeReader reader = new CodeReader(new MemoryStream());
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					packRatTests.UnpackNullableModel<object>(reader, null, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNullModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = packRatTests.UnpackNullableModel(reader, packRatTests, ByteAligned.No);

			Assert.Null(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNonNullModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("11");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = packRatTests.UnpackNullableModel(reader, packRatTests, ByteAligned.No);

			Assert.Equal("foo", actualModel);
		}
		#endregion

		#region abstract PackRat methods
		public override void PackModel(CodeWriter writer, object model)
		{
			writer.WriteCode("1");
		}

		public override object UnpackModel(CodeReader reader)
		{
			bool isNull = reader.Read(1) == "0";
			return isNull ? null : "foo";
		}
		#endregion
	}
}
