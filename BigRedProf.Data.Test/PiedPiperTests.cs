using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace BigRedProf.Data.Test
{
    public class PiedPiperTests
    {
        #region methods
        [Fact]
        [Trait("Region", "methods")]
        public void RegisterPackRat_ShouldThrowIfPackRatIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = Guid.Empty.ToString();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.RegisterPackRat<object>(null, schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void RegisterPackRat_ShouldThrowIfSchemaIdIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            PackRat<bool> packRat = new BooleanPackRat(piedPiper);
            string schemaId = null;

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.RegisterPackRat<bool>(packRat, schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void RegisterPackRat_ShouldThrowIfSchemaIdIsNotAGuid()
        {
            IPiedPiper piedPiper = new PiedPiper();
            PackRat<bool> packRat = new BooleanPackRat(piedPiper);
            string schemaId = "not-a-GUID";

            Assert.Throws<ArgumentException>(
                () =>
                {
                    piedPiper.RegisterPackRat<bool>(packRat, schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void RegisterPackRat_ShouldThrowIfPackRatIsAlreadyRegisterForSchemaId()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = Guid.Empty.ToString();
            PackRat<bool> packRat1 = new BooleanPackRat(piedPiper);
            piedPiper.RegisterPackRat(packRat1, schemaId);
            PackRat<bool> packRat2 = new BooleanPackRat(piedPiper);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    piedPiper.RegisterPackRat<bool>(packRat2, schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void GetPackRat_ShouldThrowWhenSchemaIdIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = null;

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.GetPackRat<object>(schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void GetPackRat_ShouldThrowWhenSchemaIdIsNotAGuid()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = "not-a-GUID";

            Assert.Throws<ArgumentException>(
                () =>
                {
                    piedPiper.GetPackRat<object>(schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void GetPackRat_ShouldThrowWhenSchemaIdWasntRegistered()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = Guid.Empty.ToString();

            Assert.Throws<ArgumentException>(
                () =>
                {
                    piedPiper.GetPackRat<object>(schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void GetPackRat_ShouldThrowWhenSchemaIdWasRegisteredForADifferentType()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = Guid.Empty.ToString();
            PackRat<bool> packRat1 = new BooleanPackRat(piedPiper);
            piedPiper.RegisterPackRat(packRat1, schemaId);

            Assert.Throws<InvalidOperationException>(
                () =>
                {
                    piedPiper.GetPackRat<object>(schemaId);
                }
            );
        }

        [Fact]
        [Trait("Region", "methods")]
        public void GetPackRat_ShouldWork()
        {
            IPiedPiper piedPiper = new PiedPiper();
            string schemaId = Guid.Empty.ToString();
            BooleanPackRat packRat1 = new BooleanPackRat(piedPiper);
            piedPiper.RegisterPackRat(packRat1, schemaId);

            PackRat<bool> packRat2 = piedPiper.GetPackRat<bool>(schemaId);
            Assert.Equal(packRat1, packRat2);
        }

        [Fact]
        [Trait("Region", "methods")]
        public void EncodeModel_ShouldThrowWhenModelIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterDefaultPackRats();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.EncodeModel<string>(null, SchemaId.TextUtf8);
                }
            );
        }

		[Fact]
		[Trait("Region", "methods")]
		public void EncodeModel_ShouldThrowWhenSchemaIdIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.EncodeModel<string>("foo", null);
				}
			);
		}

		[Fact]
        [Trait("Region", "methods")]
        public void EncodeModel_And_DecodeModel_ShouldWork()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterDefaultPackRats();

			TestModelEncodeAndDecode<bool>(piedPiper, true, SchemaId.Boolean);
			TestModelEncodeAndDecode<int>(piedPiper, 43, SchemaId.Int32);
			TestModelEncodeAndDecode<int>(piedPiper, 70719495, SchemaId.EfficientWholeNumber31);
			TestModelEncodeAndDecode<string>(piedPiper, "Go Big Red!", SchemaId.TextUtf8);
        }

		[Fact]
		[Trait("Region", "methods")]
		public void DecodeModel_ShouldThrowWhenCodeIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.DecodeModel<string>(null, SchemaId.TextUtf8);
				}
			);
		}

		[Fact]
        [Trait("Region", "methods")]
        public void DecodeModel_ShouldThrowWhenSchemaIdIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterDefaultPackRats();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.DecodeModel<string>("100", null);
                }
            );
        }

		[Fact]
		[Trait("Region", "methods")]
		public void EncodeModelWithSchema_ShouldThrowWhenModelIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.EncodeModelWithSchema(null, SchemaId.TextUtf8);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void EncodeModelWithSchema_ShouldThrowWhenSchemaIdIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.EncodeModelWithSchema("foo", null);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void EncodeModelWithSchema_And_DecodeModelWithSchema_ShouldWork()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

            TestModelEncodeAndDecodeWithSchema(piedPiper, true, SchemaId.Boolean);
			TestModelEncodeAndDecodeWithSchema(piedPiper, new Guid("A1423247-DF48-42BC-87F1-D57D7045880D"), SchemaId.Guid);
			TestModelEncodeAndDecodeWithSchema(piedPiper, 43, SchemaId.Int32);
			TestModelEncodeAndDecodeWithSchema(piedPiper, 70719495, SchemaId.EfficientWholeNumber31);
			TestModelEncodeAndDecodeWithSchema(piedPiper, "Go Big Red!", SchemaId.TextUtf8);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void DecodeModelWithSchema_ShouldThrowWhenCodeIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.DecodeModelWithSchema(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldThrowWhenCodeWriterIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackNullableModel<int>(null, 43, SchemaId.Int32, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "1" + "00" + fortyThreeCode;	// 00 for byte alignment

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, SchemaId.Int32, ByteAligned.Yes);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "1" + fortyThreeCode;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, SchemaId.Int32, ByteAligned.No);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNullAndByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code expectedCode = "0" + "00";	// 00 for byte alignment

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, SchemaId.Int32, ByteAligned.Yes);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNullAndNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code expectedCode = "0";

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, SchemaId.Int32, ByteAligned.No);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			codeTester.ReadAndVerify(expectedCode);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldThrowWhenCodeReaderIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackNullableModel<int>(null, SchemaId.Int32, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code code = "10000000" + fortyThreeCode;
			int expectedValue = 43;

			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);

			int actualValue = piedPiper.UnpackNullableModel<int>(reader,SchemaId.Int32, ByteAligned.Yes);

			Assert.Equal<int>(expectedValue, actualValue);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code code = "1" + fortyThreeCode;
			int expectedValue = 43;

			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);

			int actualValue = piedPiper.UnpackNullableModel<int>(reader, SchemaId.Int32, ByteAligned.No);

			Assert.Equal<int>(expectedValue, actualValue);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNullAndByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Code code = "00000000";
			int? expectedValue = null;

			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);

			int? actualValue = piedPiper.UnpackNullableModel<int?>(reader, SchemaId.Int32, ByteAligned.Yes);

			Assert.Equal<int?>(expectedValue, actualValue);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNullAndNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);

			Code code = "0";
			int? expectedValue = null;

			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);
			writer.WriteCode(code);
			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);

			int? actualValue = piedPiper.UnpackNullableModel<int?>(reader, SchemaId.Int32, ByteAligned.No);

			Assert.Equal<int?>(expectedValue, actualValue);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void SaveCodeToByteArray_ShouldThrowWhenCodeIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.SaveCodeToByteArray(null);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void SaveCodeTo_And_LoadCodeFrom_ByteArray_ShouldWork()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

            TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "0");
			TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "1");
			TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "101011");
			TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "11011000 10110011 1111");
			TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "01010001 00101110 01100010 00100111 00001101");
			TestSaveCodeToAndLoadCodeFromByteArray(piedPiper, "01101010 11010001 11011001 00101001 01010100 10010100 11");
		}

		[Fact]
		[Trait("Region", "methods")]
		public void LoadCodeFromByteArray_ShouldThrowWhenByteArrayIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterDefaultPackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
                    piedPiper.LoadCodeFromByteArray(null);
				}
			);
		}
		#endregion

		#region private methods
		private void TestModelEncodeAndDecode<M>(IPiedPiper piedPiper, M model, string schemaId)
		{
			Code encodedModel = piedPiper.EncodeModel<M>(model, schemaId);
			M decodedModel = piedPiper.DecodeModel<M>(encodedModel, schemaId);
			Assert.Equal(model, decodedModel);
		}

		private void TestModelEncodeAndDecodeWithSchema(IPiedPiper piedPiper, object model, string schemaId)
		{
			Code encodedModel = piedPiper.EncodeModelWithSchema(model, schemaId);
			ModelWithSchema modelWithSchema = piedPiper.DecodeModelWithSchema(encodedModel);
            Assert.Equal(new Guid(schemaId), new Guid(modelWithSchema.SchemaId));
			Assert.Equal(model, modelWithSchema.Model);
		}

		private void TestSaveCodeToAndLoadCodeFromByteArray(IPiedPiper piedPiper, Code code)
		{
			byte[] byteArray = piedPiper.SaveCodeToByteArray(code);
			Code roundTrippedCode = piedPiper.LoadCodeFromByteArray(byteArray);
			Assert.Equal(code, roundTrippedCode);
		}
		#endregion
	}
}