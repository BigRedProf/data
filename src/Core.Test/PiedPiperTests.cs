using BigRedProf.Data.Core;
using BigRedProf.Data.Core.Internal.PackRats;
using BigRedProf.Data.Core.PackRats;
using BigRedProf.Data.Test._TestHelpers;
using System;
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

            Assert.Throws<FormatException>(
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
		public void RegisterTokenizer_ShouldThrowIfTokenizerIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			string tokenizerId = Guid.Empty.ToString();
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.RegisterTokenizer<string>(null, tokenizerId);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RegisterTokenizer_ShouldRegisterTokenizer()
		{
			IPiedPiper piedPiper = new PiedPiper();
			string tokenizerId = Guid.Empty.ToString();
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			piedPiper.RegisterTokenizer<string>(tokenizer, tokenizerId);

			Tokenizer<string> tokenizer2 = piedPiper.GetTokenizer<string>(tokenizerId);
			Assert.NotNull(tokenizer2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RegisterTokenizer_ShouldRegisterCorrespondingPackRat()
		{
			IPiedPiper piedPiper = new PiedPiper();
			string tokenizerId = Guid.Empty.ToString();
			Tokenizer<string> tokenizer = new Tokenizer<string>();

			piedPiper.RegisterTokenizer<string>(tokenizer, tokenizerId);

			PackRat<string> packRat = piedPiper.GetPackRat<string>(tokenizerId);
			TokenizedModelPackRat<string> tokenizedModelPackRat = packRat as TokenizedModelPackRat<string>;
			Assert.NotNull(packRat);
			Assert.NotNull(tokenizedModelPackRat);
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

            Assert.Throws<FormatException>(
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
        public void PackModelToCode_ShouldThrowWhenModelIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterCorePackRats();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.PackModel<string>(null, CoreSchema.TextUtf8);
                }
            );
        }

		[Fact]
		[Trait("Region", "methods")]
		public void PackModelToCode_ShouldThrowWhenSchemaIdIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackModel<string>("foo", null);
				}
			);
		}

		[Fact]
        [Trait("Region", "methods")]
        public void PackModelToCode_And_UnpackModelFromCode_ShouldWork()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterCorePackRats();

			TestModelEncodeAndDecode<bool>(piedPiper, true, CoreSchema.Boolean);
			TestModelEncodeAndDecode<int>(piedPiper, 43, CoreSchema.Int32);
			TestModelEncodeAndDecode<int>(piedPiper, 70719495, CoreSchema.EfficientWholeNumber31);
			TestModelEncodeAndDecode<string>(piedPiper, "Go Big Red!", CoreSchema.TextUtf8);
        }

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackModelFromCode_ShouldThrowWhenCodeIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackModel<string>((Code)null, CoreSchema.TextUtf8);
				}
			);
		}

		[Fact]
        [Trait("Region", "methods")]
        public void UnpackModelFromCode_ShouldThrowWhenSchemaIdIsNull()
        {
            IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterCorePackRats();

            Assert.Throws<ArgumentNullException>(
                () =>
                {
                    piedPiper.UnpackModel<string>("100", null);
                }
            );
        }

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldThrowWhenCodeWriterIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
            piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.PackNullableModel<int>(null, 43, CoreSchema.Int32, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void PackNullableModel_ShouldWorkWhenByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "1" + "00" + fortyThreeCode;	// 00 for byte alignment

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, CoreSchema.Int32, ByteAligned.Yes);
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
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code expectedCode = "1" + fortyThreeCode;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, CoreSchema.Int32, ByteAligned.No);
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
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code expectedCode = "0" + "00";	// 00 for byte alignment

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, CoreSchema.Int32, ByteAligned.Yes);
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
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code expectedCode = "0";

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, CoreSchema.Int32, ByteAligned.No);
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
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.UnpackNullableModel<int>(null, CoreSchema.Int32, ByteAligned.No);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code code = "1" + "00" + fortyThreeCode;    // 00 for byte alignment
			int expectedValue = 43;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, CoreSchema.Int32, ByteAligned.Yes);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			int actualValue = piedPiper.UnpackNullableModel<int>(codeTester.Reader, CoreSchema.Int32, ByteAligned.Yes);
			Assert.Equal<int>(expectedValue, actualValue);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code fortyThreeCode = "11010100 00000000 00000000 00000000";
			Code code = "1" + "00" + fortyThreeCode;    // 00 for byte alignment
			int expectedValue = 43;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int>(codeTester.Writer, 43, CoreSchema.Int32, ByteAligned.No);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			int actualValue = piedPiper.UnpackNullableModel<int>(codeTester.Reader, CoreSchema.Int32, ByteAligned.No);
			Assert.Equal<int>(expectedValue, actualValue);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNullAndByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code code = "0" + "00";    // 00 for byte alignment
			int? expectedValue = null;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, CoreSchema.Int32, ByteAligned.Yes);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			int? actualValue = piedPiper.UnpackNullableModel<int?>(codeTester.Reader, CoreSchema.Int32, ByteAligned.Yes);
			Assert.Equal<int?>(expectedValue, actualValue);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void UnpackNullableModel_ShouldWorkWhenNullAndNotByteAligned()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterPackRat<int>(new Int32PackRat(piedPiper), CoreSchema.Int32);
			CodeTester codeTester = new CodeTester();
			Code expectedAlignmentMess1 = "10101010 10101010 10101010 10101";
			Code expectedAlignmentMess2 = "11011011 01101101 01101101 1";
			Code code = "0";
			int? expectedValue = null;

			codeTester.Write(expectedAlignmentMess1);
			piedPiper.PackNullableModel<int?>(codeTester.Writer, null, CoreSchema.Int32, ByteAligned.Yes);
			codeTester.Write(expectedAlignmentMess2);

			codeTester.StopWritingAndStartReading();

			codeTester.ReadAndVerify(expectedAlignmentMess1);
			int? actualValue = piedPiper.UnpackNullableModel<int?>(codeTester.Reader, CoreSchema.Int32, ByteAligned.Yes);
			Assert.Equal<int?>(expectedValue, actualValue);
			codeTester.ReadAndVerify(expectedAlignmentMess2);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void SaveCodeToByteArray_ShouldThrowWhenCodeIsNull()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();

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
			piedPiper.RegisterCorePackRats();

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
			piedPiper.RegisterCorePackRats();

			Assert.Throws<ArgumentNullException>(
				() =>
				{
                    piedPiper.LoadCodeFromByteArray(null);
				}
			);
		}
		#endregion

		#region core trait tests
		[Fact]
		[Trait("Region", "methods")]
		public void DefineCoreTraits_ShouldDefineKindAsAGuid()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.DefineCoreTraits();

			BigRedProf.Data.Core.Trait kind = piedPiper.GetTrait(CoreTrait.Kind);

			Assert.Equal(new Guid(CoreSchema.Guid), kind.SchemaId);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Kind_ShouldBeAnOrdinaryTrait()
		{
			// Kind carries no machinery: it is written, read, and absent exactly like any other
			// answer. If this test ever needs special handling, something has made kind
			// structural, which is the one thing it must never become.
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineCoreTraits();
			Guid chair = new Guid("2a1e0f52-6d9c-4a3b-9a0e-1c4f6b7d8e90");

			FlexDatum withKind = new FlexDatumBuilder(piedPiper)
				.AddTrait(CoreTrait.Kind, chair)
				.Build();
			FlexDatum withoutKind = new FlexDatumBuilder(piedPiper).Build();

			Assert.Equal(chair, withKind.GetTrait<Guid>(CoreTrait.Kind, piedPiper));
			Assert.False(withoutKind.TryGetTrait<Guid>(CoreTrait.Kind, piedPiper, out _));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void CoreTraitIdentifiers_ShouldNeverChange()
		{
			// A trait identifier is bound to its schema forever, so a core identifier is not a
			// value anyone may edit: changing one silently reinterprets every record ever
			// written. Pinning the literals makes an accidental edit a failing test rather than
			// a discovery made years later. To ask a different question, mint a new identifier.
			Assert.Equal("7759e69c-15cd-44ee-a02e-3f29759fbe35", CoreTrait.Id);
			Assert.Equal("0bc66d67-3976-436d-90a3-c4faa811ab34", CoreTrait.Name);
			Assert.Equal("80f6f851-2f07-48de-9950-b21d8e1ef734", CoreTrait.Kind);
			Assert.Equal("ce22d178-02ec-470c-b8de-60c71961dec2", CoreTrait.Content);
			Assert.Equal("93a2dbed-065e-4f64-8ab0-8448a82a30ea", CoreTrait.ContentDigest);
			Assert.Equal("6f182156-5ac4-4670-a1da-0d5339f64509", CoreTrait.ContentLength);
			Assert.Equal("9080538a-aafc-4ab9-a90f-e1c0d2d3f814", CoreTrait.SeriesId);
			Assert.Equal("cbeabd91-8580-45ed-97d4-c797c36d0611", CoreTrait.SeriesName);
			Assert.Equal("9866367b-f1ae-4699-b123-32691149488b", CoreTrait.SeriesNumber);
			Assert.Equal("8c110105-4569-4a7c-a0e9-9e417ac252d2", CoreTrait.SeriesHeadDigest);
			Assert.Equal("35c4fbf0-d0e9-4e5c-822e-22bd4a64eb30", CoreTrait.SeriesParentDigest);
		}
		#endregion

		#region private methods
		private void TestModelEncodeAndDecode<M>(IPiedPiper piedPiper, M model, string schemaId)
		{
			Code encodedModel = piedPiper.PackModel<M>(model, schemaId);
			M decodedModel = piedPiper.UnpackModel<M>(encodedModel, schemaId);
			Assert.Equal(model, decodedModel);
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