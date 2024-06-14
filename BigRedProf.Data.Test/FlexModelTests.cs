using System;
using System.Collections.Generic;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class FlexModelTests
	{
		#region methods

		[Fact]
		[Trait("Region", "methods")]
		public void GetTraitIds_ShouldReturnEmptyList_WhenNoTraitsAdded()
		{
			var model = new FlexModel();
			var traitIds = model.GetTraitIds();
			Assert.Empty(traitIds);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void HasTrait_ShouldReturnFalse_WhenTraitNotPresent()
		{
			var model = new FlexModel();
			bool hasTrait = model.HasTrait("nonexistent");
			Assert.False(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AddTrait_ShouldAddTrait()
		{
			var model = new FlexModel();
			var trait = new Trait<int>("Height", "Int32", 72);
			model.AddTrait(trait);
			bool hasTrait = model.HasTrait("Height");
			Assert.True(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldReturnTraitValue_WhenTraitExists()
		{
			var model = new FlexModel();
			var trait = new Trait<int>("Height", "Int32", 72);
			model.AddTrait(trait);
			int value = model.GetTrait<int>("Height");
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldThrow_WhenTraitDoesNotExist()
		{
			var model = new FlexModel();
			Assert.Throws<ArgumentException>(() => model.GetTrait<int>("nonexistent"));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnTrueAndTraitValue_WhenTraitExists()
		{
			var model = new FlexModel();
			var trait = new Trait<int>("Height", "Int32", 72);
			model.AddTrait(trait);
			bool result = model.TryGetTrait("Height", out int value);
			Assert.True(result);
			Assert.Equal(72, value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnFalse_WhenTraitDoesNotExist()
		{
			var model = new FlexModel();
			bool result = model.TryGetTrait("nonexistent", out int value);
			Assert.False(result);
			Assert.Equal(default(int), value);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RemoveTrait_ShouldReturnTrue_WhenTraitExists()
		{
			var model = new FlexModel();
			var trait = new Trait<int>("Height", "Int32", 72);
			model.AddTrait(trait);
			bool removed = model.RemoveTrait("Height");
			Assert.True(removed);
			bool hasTrait = model.HasTrait("Height");
			Assert.False(hasTrait);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void RemoveTrait_ShouldReturnFalse_WhenTraitDoesNotExist()
		{
			var model = new FlexModel();
			bool removed = model.RemoveTrait("nonexistent");
			Assert.False(removed);
		}

		#endregion
	}
}
