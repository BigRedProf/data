using System;
using System.Collections.Generic;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class FlexModelTests
	{
		private IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			DefineTraits(piedPiper);
			return piedPiper;
		}

		private void DefineTraits(IPiedPiper piedPiper)
		{
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000002", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000003", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000005", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000001", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000004", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000006", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000007", CoreSchema.Int32));
			piedPiper.DefineTrait(new TraitDefinition("00000000-0000-0000-0000-000000000008", CoreSchema.Int32));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTraitIds_ShouldReturnEmptyList_WhenNoTraitsAdded()
		{
			FlexModel model = new FlexModel();
			IList<Guid> traitIds = model.GetTraitIds();
			Assert.Empty(traitIds);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void HasTrait_ShouldReturnFalse_WhenTraitNotPresent()
		{
			FlexModel model = new FlexModel();
			bool hasTrait = model.HasTrait("00000000-0000-0000-0000-000000000001");
			Assert.False(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AddTrait_ShouldAddTrait()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			string traitId = "00000000-0000-0000-0000-000000000002";
			Trait<int> trait = new Trait<int>(traitId, 72);
			model.AddTrait(piedPiper, trait);
			bool hasTrait = model.HasTrait(traitId);
			Assert.True(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldReturnTraitValue_WhenTraitExists()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			string traitId = "00000000-0000-0000-0000-000000000003";
			Trait<int> trait = new Trait<int>(traitId, 72);
			model.AddTrait(piedPiper, trait);
			int value = model.GetTrait<int>(piedPiper, traitId);
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldThrow_WhenTraitDoesNotExist()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			Assert.Throws<ArgumentException>(() => model.GetTrait<int>(piedPiper, "00000000-0000-0000-0000-000000000004"));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnTrueAndTraitValue_WhenTraitExists()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			string traitId = "00000000-0000-0000-0000-000000000005";
			Trait<int> trait = new Trait<int>(traitId, 72);
			model.AddTrait(piedPiper, trait);
			bool result = model.TryGetTrait<int>(piedPiper, traitId, out int value);
			Assert.True(result);
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnFalse_WhenTraitDoesNotExist()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			bool result = model.TryGetTrait<int>(piedPiper, "00000000-0000-0000-0000-000000000006", out int value);
			Assert.False(result);
			Assert.Equal(default, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RemoveTrait_ShouldReturnTrue_WhenTraitExists()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			string traitId = "00000000-0000-0000-0000-000000000007";
			Trait<int> trait = new Trait<int>(traitId, 72);
			model.AddTrait(piedPiper, trait);
			bool removed = model.RemoveTrait(traitId);
			Assert.True(removed);
			bool hasTrait = model.HasTrait(traitId);
			Assert.False(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RemoveTrait_ShouldReturnFalse_WhenTraitDoesNotExist()
		{
			FlexModel model = new FlexModel();
			bool removed = model.RemoveTrait("00000000-0000-0000-0000-000000000008");
			Assert.False(removed);
		}
	}
}
