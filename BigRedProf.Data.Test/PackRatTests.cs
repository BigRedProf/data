using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class PackRatTests
	{
		#region protected methods
		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldThrowWhenSchemaIdIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackNullableModel<object>(null, "foo", string.Empty, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldThrowWhenPackRatIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper(); 
			CodeWriter writer = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackNullableModel<object>(writer, null, null, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackNullableModel_ShouldWorkForNullModels()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackNullableModel<string>(writer, null, SchemaId.TextUtf8, ByteAligned.No);
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackNullableModel<string>(writer, null, SchemaId.TextUtf8, ByteAligned.Yes);
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackNullableModel<bool>(writer, true, SchemaId.Boolean, ByteAligned.No);

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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackNullableModel<bool>(writer, true, SchemaId.Boolean, ByteAligned.Yes);
			Code expectedCode = "10000000 1";
			// null bit -> 1
			// byte alignment
			// our model -> 1

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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackNullableModel<bool?>(null, SchemaId.Boolean, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldThrowWhenPackRatIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = new CodeReader(new MemoryStream());
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackNullableModel<object>(reader, null, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNullModels()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");

			bool? actualModel = piedPiper.UnpackNullableModel<bool?>(reader, SchemaId.Boolean, ByteAligned.No);

			Assert.Null(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNullByteAlignedModels()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");

			bool? actualModel = piedPiper.UnpackNullableModel<bool?>(reader, SchemaId.Boolean, ByteAligned.Yes);

			Assert.Null(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNonNullModels()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader("11");

			bool actualModel = piedPiper.UnpackNullableModel<bool>(reader, SchemaId.Boolean, ByteAligned.No);

			Assert.True(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void UnpackNullableModel_ShouldWorkForNonNullByteAlignedModels()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader("10000000 1");

			bool actualModel = piedPiper.UnpackNullableModel<bool>(reader, SchemaId.Boolean, ByteAligned.Yes);

			Assert.True(actualModel);
		}

		[Fact]
		[Trait("Region", "protected methods")]
		public void PackList_ShouldThrowWhenWriterIsNull()
		{
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackList<string>(
						null, 
						new string[] { "foo" }, 
						SchemaId.TextUtf8, 
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackList<string>(
						codeWriter,
						null,
						SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentException>(
				() =>
				{
					piedPiper.PackList<string>(
						codeWriter,
						new string[] { "foo", null, "bar" },
						SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				null,
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				new string[] { "foo", "bar" },
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<bool>(
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			piedPiper.PackList<string>(
				writer,
				new string[] { "foo", null, "bar" },
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackList<string>(
						null,
						SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader("0");

			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"1001 1011 01100110 11110110 11110110 1011 0000 01000110 10000110 01001110"
			);

			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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
			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			CodeReader reader = PackRatTestHelper.CreateCodeReader(
				"11010000 1 0000000 0 0000000 1 0000000 1 0000000 0"
			);

			IList<bool> actualList = piedPiper.UnpackList<bool>(
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

			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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

			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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

			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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

			IPiedPiper piedPiper = PackRatTestHelper.GetPiedPiper();
			IList<string> actualList = piedPiper.UnpackList<string>(
				reader,
				SchemaId.TextUtf8,
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
	}
}
