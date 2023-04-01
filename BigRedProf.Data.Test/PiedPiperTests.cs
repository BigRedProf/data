using BigRedProf.Data.Internal.PackRats;
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
                    piedPiper.EncodeModel<string>(null, SchemaId.StringUtf8);
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
			TestModelEncodeAndDecode<string>(piedPiper, "Go Big Red!", SchemaId.StringUtf8);
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
					piedPiper.DecodeModel<string>(null, SchemaId.StringUtf8);
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
		#endregion

		#region private methods
		private void TestModelEncodeAndDecode<M>(IPiedPiper piedPiper, M model, string schemaId)
		{
			Code encodedModel = piedPiper.EncodeModel<M>(model, schemaId);
			M decodedModel = piedPiper.DecodeModel<M>(encodedModel, schemaId);
			Assert.Equal(model, decodedModel);
		}
		#endregion
	}
}