using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class PackRatTests : PackRat<object>
	{
		#region constructors
		public PackRatTests()
			: base(PackRatTestHelper.GetPiedPiper())
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
					PiedPiper.PackNullableModel<object>(null, "foo", packRatTests, ByteAligned.No);
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
					PiedPiper.PackNullableModel<object>(writer, null, null, ByteAligned.No);
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

			PiedPiper.PackNullableModel(writer, null, packRatTests, ByteAligned.No);
			Code expectedCode = "0";
			// null bit -> 0

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(1);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNullByteAlignedModels()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackNullableModel(writer, null, packRatTests, ByteAligned.Yes);
			Code expectedCode = "0";
			// null bit -> 0

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(1);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNonNullModels()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackNullableModel(writer, "foo", packRatTests, ByteAligned.No);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(2);
			Assert.Equal("11", actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNonNullByteAlignedModels()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackNullableModel(writer, "foo", packRatTests, ByteAligned.Yes);
			Code expectedCode = "10000000 1";
			// null bit -> 1
			// byte alignment
			// our dummy model -> 1

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(2, readerStream.Length);
			Code actualCode = reader.Read(9);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldThrowWhenReaderIsNull()
		{
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					PiedPiper.UnpackNullableModel<object>(null, packRatTests, ByteAligned.No);
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
					PiedPiper.UnpackNullableModel<object>(reader, null, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNullModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = PiedPiper.UnpackNullableModel(reader, packRatTests, ByteAligned.No);

			Assert.Null(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNullByteAlignedModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = PiedPiper.UnpackNullableModel(reader, packRatTests, ByteAligned.Yes);

			Assert.Null(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNonNullModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("11");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = PiedPiper.UnpackNullableModel(reader, packRatTests, ByteAligned.No);

			Assert.Equal("foo", actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNonNullByteAlignedModels()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("10000000 1");
			PackRatTests packRatTests = new PackRatTests();

			object actualModel = PiedPiper.UnpackNullableModel(reader, packRatTests, ByteAligned.Yes);

			Assert.Equal("foo", actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldThrowWhenWriterIsNull()
		{
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					PiedPiper.PackList<string>(
						null, 
						new string[] { "foo" }, 
						SchemaId.StringUtf8, 
						false, 
						false, 
						ByteAligned.No
					);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldThrowWhenListIsNullAndNullListsAreNotAllowed()
		{
			PackRatTests packRatTests = new PackRatTests();
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					PiedPiper.PackList<string>(
						codeWriter,
						null,
						SchemaId.StringUtf8,
						false,
						true,
						ByteAligned.No
					);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldThrowWhenListHasNullElementsAndNullElementsAreNotAllowed()
		{
			PackRatTests packRatTests = new PackRatTests();
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentException>(
				() =>
				{
					PiedPiper.PackList<string>(
						codeWriter,
						new string[] { "foo", null, "bar" },
						SchemaId.StringUtf8,
						true,
						false,
						ByteAligned.No
					);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNullLists()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				null,
				SchemaId.StringUtf8,
				true,
				false,
				ByteAligned.No
			);
			Code expectedCode = "0";
			// null bit -> 0

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(1, readerStream.Length);
			Code actualCode = reader.Read(1);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNonNullableListsWithoutNullElements()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				new string[] { "foo", "bar" },
				SchemaId.StringUtf8,
				false,
				false,
				ByteAligned.No
			);
			Code expectedCode = "1001 1011 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110";
			// 2 -> 1001
			// "foo" -> 1011 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000 .01100010 .01100001 .01110010

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(8, readerStream.Length);
			Code actualCode = reader.Read(64);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNonNullableListsWithoutNullElementsByteAligned()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<bool>(
				writer,
				new bool[] { true, false, true, true, false },
				SchemaId.Boolean,
				false,
				false,
				ByteAligned.Yes
			);
			Code expectedCode = "11010000 1 0000000 0 0000000 1 0000000 1 0000000 0";
			// 5 -> 11010000
			// true -> 1
			// byte alignment -> 0000000
			// false -> 0
			// byte alignment -> 0000000
			// true -> 1
			// byte alignment -> 0000000
			// true -> 1
			// byte alignment -> 0000000
			// false -> 0

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(6, readerStream.Length);
			Code actualCode = reader.Read(41);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNonNullableListsWithNullElements()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.StringUtf8,
				false,
				true,
				ByteAligned.No
			);
			Code expectedCode = "1011 101 1011 00000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110";
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba00000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(9, readerStream.Length);
			Code actualCode = reader.Read(72);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNonNullableListsWithNullElementsByteAligned()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.StringUtf8,
				false,
				true,
				ByteAligned.Yes
			);
			Code expectedCode = "1011 101 0 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110";
			// 3 -> 1011
			// null element array -> 101
			// byte alignment -> 0
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(9, readerStream.Length);
			Code actualCode = reader.Read(72);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNullableListsWithNullElements()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.StringUtf8,
				true,
				true,
				ByteAligned.No
			);
			Code expectedCode = "1 1011 101 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110";
			// null bit -> 1
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(9, readerStream.Length);
			Code actualCode = reader.Read(72);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldWorkForNullableListsWithNullElementsByteAligned()
		{
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			PackRatTests packRatTests = new PackRatTests();

			PiedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.StringUtf8,
				true,
				true,
				ByteAligned.Yes
			);
			Code expectedCode = "1 1011 101 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110";
			// null bit -> 1
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			Assert.Equal(9, readerStream.Length);
			Code actualCode = reader.Read(72);
			Assert.Equal(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldThrowWhenReaderIsNull()
		{
			PackRatTests packRatTests = new PackRatTests();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					PiedPiper.UnpackList<string>(
						null,
						SchemaId.StringUtf8,
						false,
						false,
						ByteAligned.No
					);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNullLists()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				true,
				false,
				ByteAligned.No
			);
			IList<string> expectedList = null;
			// null bit -> 0

			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNonNullableListsWithoutNullElements()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1001 1011 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				false,
				false,
				ByteAligned.No
			);
			IList<string> expectedList = new string[] { "foo", "bar" };
			// 2 -> 1001
			// "foo" -> 1011 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000 .01100010 .01100001 .01110010

			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNonNullableListsWithoutNullElementsByteAligned()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"11010000 1 0000000 0 0000000 1 0000000 1 0000000 0"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<bool> actualList = PiedPiper.UnpackList<bool>(
				reader,
				SchemaId.Boolean,
				false,
				false,
				ByteAligned.Yes
			);
			IList<bool> expectedList = new bool[] { true, false, true, true, false };
			// 5 -> 11010000
			// true -> 1
			// byte alignment -> 0000000
			// false -> 0
			// byte alignment -> 0000000
			// true -> 1
			// byte alignment -> 0000000
			// true -> 1
			// byte alignment -> 0000000
			// false -> 0
			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNonNullableListsWithNullElements()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1011 101 1011 00000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				false,
				true,
				ByteAligned.No
			);
			IList<string> expectedList = new string[] { "foo", null, "bar" };
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba00000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNonNullableListsWithNullElementsByteAligned()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1011 101 0 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				false,
				true,
				ByteAligned.Yes
			);
			IList<string> expectedList = new string[] { "foo", null, "bar" };
			// 3 -> 1011
			// null element array -> 101
			// byte alignment -> 0
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNullableListsWithNullElements()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1 1011 101 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				true,
				true,
				ByteAligned.No
			);
			IList<string> expectedList = new string[] { "foo", null, "bar" };
			// null bit -> 1
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			Assert.Equal(expectedList, actualList);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackList_ShouldWorkForNullableListsWithNullElementsByteAligned()
		{
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1 1011 101 1011 0000 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);
			PackRatTests packRatTests = new PackRatTests();

			IList<string> actualList = PiedPiper.UnpackList<string>(
				reader,
				SchemaId.StringUtf8,
				true,
				true,
				ByteAligned.Yes
			);
			IList<string> expectedList = new string[] { "foo", null, "bar" };
			// null bit -> 1
			// 3 -> 1011
			// null element array -> 101
			// "foo" -> 1011 ba0000 .01100110 .01101111 .01101111 
			// "bar" -> 1011 ba0000000 .01100010 .01100001 .01110010

			Assert.Equal(expectedList, actualList);
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
