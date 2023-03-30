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
			Assert.False(true);
        }

        [Fact]
        [Trait("Region", "methods")]
        public void EncodeModel_ShouldWork()
        {
            Assert.False(true);
        }

        [Fact]
        [Trait("Region", "methods")]
        public void DecodeModel_ShouldThrowWhenCodeIsNull()
        {
            Assert.False(true);
        }

        [Fact]
        [Trait("Region", "methods")]
        public void DecodeModel_ShouldWork()
        {
            Assert.False(true);
        }
        #endregion
    }
}