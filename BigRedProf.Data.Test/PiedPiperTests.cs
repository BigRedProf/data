using BigRedProf.Data.Internal.PackRats;
using System;
using System.IO;
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
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "10000000" + fortyThreeCode;

			piedPiper.PackNullableModel<int>(writer, 43, SchemaId.Int32, ByteAligned.Yes);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			int expectedStreamLength = (expectedCode.Length / 8) + ((expectedCode.Length % 8) > 0 ? 1 : 0);
			Assert.Equal(expectedStreamLength, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "1" + fortyThreeCode;

			piedPiper.PackNullableModel<int>(writer, 43, SchemaId.Int32, ByteAligned.No);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			int expectedStreamLength = (expectedCode.Length / 8) + ((expectedCode.Length % 8) > 0 ? 1 : 0);
			Assert.Equal(expectedStreamLength, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNullAndByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			Code expectedCode = "00000000";

			piedPiper.PackNullableModel<int?>(writer, null, SchemaId.Int32, ByteAligned.Yes);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			int expectedStreamLength = (expectedCode.Length / 8) + ((expectedCode.Length % 8) > 0 ? 1 : 0);
			Assert.Equal(expectedStreamLength, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenNullAndNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), SchemaId.Int32);
			MemoryStream writerStream = new MemoryStream();
			CodeWriter writer = new CodeWriter(writerStream);

			Code expectedCode = "0";

			piedPiper.PackNullableModel<int?>(writer, null, SchemaId.Int32, ByteAligned.No);

			writer.Dispose();
			Stream readerStream = new MemoryStream(writerStream.ToArray());
			CodeReader reader = new CodeReader(readerStream);
			int expectedStreamLength = (expectedCode.Length / 8) + ((expectedCode.Length % 8) > 0 ? 1 : 0);
			Assert.Equal(expectedStreamLength, readerStream.Length);
			Code actualCode = reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
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