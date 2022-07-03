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
			Guid schemaId = Guid.Empty;

			Assert.Throws<ArgumentNullException>(
				() =>
				{
					piedPiper.RegisterPackRat<object>(null, schemaId);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RegisterPackRat_ShouldThrowIfPackRatIsAlreadyRegisterForSchemaId()
		{
			IPiedPiper piedPiper = new PiedPiper();
			Guid schemaId = Guid.Empty;
			PackRat<bool> packRat1 = new BooleanPackRat();
			piedPiper.RegisterPackRat(packRat1, schemaId);
			PackRat<bool> packRat2 = new BooleanPackRat();

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					piedPiper.RegisterPackRat<bool>(packRat2, schemaId);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetPackRat_ShouldThrowWhenSchemaIdWasntRegistered()
		{
			IPiedPiper piedPiper = new PiedPiper();
			Guid schemaId = Guid.Empty;
			
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
			Guid schemaId = Guid.Empty;
			PackRat<bool> packRat1 = new BooleanPackRat();
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
			Guid schemaId = Guid.Empty;
			BooleanPackRat packRat1 = new BooleanPackRat();
			piedPiper.RegisterPackRat(packRat1, schemaId);

			PackRat<bool> packRat2 = piedPiper.GetPackRat<bool>(schemaId);
			Assert.Equal(packRat1, packRat2);
		}
		#endregion
	}
}