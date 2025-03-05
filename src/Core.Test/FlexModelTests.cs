using BigRedProf.Data.Core;
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
		[Trait("Region", "constructors")]
		public void DefaultConstructor_ShouldWork()
		{
			FlexModel model = new FlexModel();
			IList<Guid> traitIds = model.GetTraitIds();
			Assert.Empty(traitIds);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void CapacityConstructor_ShouldWork()
		{
			FlexModel model = new FlexModel(43);
			IList<Guid> traitIds = model.GetTraitIds();
			Assert.Empty(traitIds);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithNoArguments_ShouldWork()
		{
			FlexModel model = new FlexModel(new object[] { });
			IList<Guid> traitIds = model.GetTraitIds();
			Assert.Empty(traitIds);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithOneArgument_ShouldThrow()
		{
			Assert.Throws<ArgumentException>(
				() =>
				{
					new FlexModel("00000000-0000-0000-0000-000000000002");
				}
			);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithTwoArguments_ShouldWork()
		{
			string traitId = "00000000-0000-0000-0000-000000000001";
			Trait<int> trait = new Trait<int>(traitId, 43);

			FlexModel model = new FlexModel(traitId, CoreSchema.Int32);

			int traitCount = model.GetTraitIds().Count;
			Assert.Equal(1, traitCount);
			bool hasTrait = model.HasTrait(traitId);
			Assert.True(hasTrait);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithFourArguments_ShouldWork()
		{
			string trait1Id = "00000000-0000-0000-0000-000000000001";
			Trait<int> trait1 = new Trait<int>(trait1Id, 43);
			string trait2Id = "00000000-0000-0000-0000-000000000002";
			Trait<int> trait2 = new Trait<int>(trait1Id, 95);

			FlexModel model = new FlexModel(
				trait1Id, CoreSchema.Int32,
				trait2Id, CoreSchema.Int32
			);

			int traitCount = model.GetTraitIds().Count;
			Assert.Equal(2, traitCount);
			bool hasTrait1 = model.HasTrait(trait1Id);
			Assert.True(hasTrait1);
			bool hasTrait2 = model.HasTrait(trait2Id);
			Assert.True(hasTrait2);
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithInvalidTypeIdentifier_ShouldThrow()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new FlexModel(123, CoreSchema.Int32); // Invalid type: int
			});
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithNullIdentifier_ShouldThrow()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new FlexModel(null, CoreSchema.Int32); // Invalid type: null
			});
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithMixedValidAndInvalidIdentifiers_ShouldThrow()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				new FlexModel(
					"00000000-0000-0000-0000-000000000001", CoreSchema.Int32,
					123, CoreSchema.Int32 // Invalid type: int
				);
			});
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithValidStringIdentifier_ShouldWork()
		{
			string traitId = "00000000-0000-0000-0000-000000000001";
			FlexModel model = new FlexModel(traitId, CoreSchema.Int32);

			Assert.True(model.HasTrait(traitId));
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithValidGuidIdentifier_ShouldWork()
		{
			Guid traitId = Guid.Parse("00000000-0000-0000-0000-000000000001");
			FlexModel model = new FlexModel(traitId, CoreSchema.Int32);

			Assert.True(model.HasTrait(traitId));
		}

		[Fact]
		[Trait("Region", "constructors")]
		public void ModelPairsConstructor_WithValidAttributeFriendlyGuidIdentifier_ShouldWork()
		{
			AttributeFriendlyGuid traitId = new AttributeFriendlyGuid("00000000-0000-0000-0000-000000000001");
			FlexModel model = new FlexModel(traitId, CoreSchema.Int32);

			Assert.True(model.HasTrait(traitId));
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
			model.AddTrait(trait);
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
			model.AddTrait(trait);
			int value = model.GetTrait<int>(traitId);
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldThrow_WhenTraitDoesNotExist()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			Assert.Throws<ArgumentException>(() => model.GetTrait<int>("00000000-0000-0000-0000-000000000004"));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnTrueAndTraitValue_WhenTraitExists()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			string traitId = "00000000-0000-0000-0000-000000000005";
			Trait<int> trait = new Trait<int>(traitId, 72);
			model.AddTrait(trait);
			bool result = model.TryGetTrait<int>(traitId, out int value);
			Assert.True(result);
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnFalse_WhenTraitDoesNotExist()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexModel model = new FlexModel();
			bool result = model.TryGetTrait<int>("00000000-0000-0000-0000-000000000006", out int value);
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
			model.AddTrait(trait);
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
