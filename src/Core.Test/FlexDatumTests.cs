using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class FlexDatumTests
	{
		#region constants
		private const string Trait1 = "00000000-0000-0000-0000-000000000001";
		private const string Trait2 = "00000000-0000-0000-0000-000000000002";
		private const string Trait3 = "00000000-0000-0000-0000-000000000003";
		private const string TraitText = "00000000-0000-0000-0000-000000000009";
		private const string TraitEnum = "00000000-0000-0000-0000-00000000000a";

		// Deliberately never defined on any pied piper in these tests.
		private const string UndefinedTrait = "ffffffff-0000-0000-0000-00000000000f";
		#endregion

		#region test helpers
		private enum TestGameType
		{
			Standard = 1,
			Deluxe = 2
		}

		private static IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineTrait(new Trait(Trait1, CoreSchema.Int32));
			piedPiper.DefineTrait(new Trait(Trait2, CoreSchema.Int32));
			piedPiper.DefineTrait(new Trait(Trait3, CoreSchema.Int32));
			piedPiper.DefineTrait(new Trait(TraitText, CoreSchema.TextUtf8));
			piedPiper.DefineTrait(new Trait(TraitEnum, CoreSchema.Int32));
			return piedPiper;
		}
		#endregion

		#region builder tests
		[Fact]
		[Trait("Region", "builder")]
		public void Builder_ShouldProduceAnEmptyFlexDatum()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper).Build();

			Assert.Equal(0, flexDatum.Count);
			Assert.Empty(flexDatum.GetTraitIds());
		}

		[Fact]
		[Trait("Region", "builder")]
		public void AddTrait_ShouldPackTheValue()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 43)
				.AddTrait(TraitText, "Memorial Stadium")
				.Build();

			Assert.True(flexDatum.HasTrait(Trait1));
			Assert.Equal(43, flexDatum.GetTrait<int>(Trait1, piedPiper));
			Assert.Equal("Memorial Stadium", flexDatum.GetTrait<string>(TraitText, piedPiper));
		}

		[Fact]
		[Trait("Region", "builder")]
		public void AddTrait_ShouldReplaceAnExistingAnswer()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 43)
				.AddTrait(Trait1, 95)
				.Build();

			// A subject has at most one answer per question.
			Assert.Equal(1, flexDatum.Count);
			Assert.Equal(95, flexDatum.GetTrait<int>(Trait1, piedPiper));
		}

		[Fact]
		[Trait("Region", "builder")]
		public void RemoveTrait_ShouldWork()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatumBuilder builder = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 43);

			Assert.True(builder.RemoveTrait(Trait1));
			Assert.False(builder.RemoveTrait(Trait1));
			Assert.Equal(0, builder.Build().Count);
		}

		[Fact]
		[Trait("Region", "builder")]
		public void ToBuilder_ShouldNotMutateTheOriginal()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum original = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 43).Build();

			FlexDatum revised = original.ToBuilder(piedPiper).AddTrait(Trait2, 72).Build();

			Assert.Equal(1, original.Count);
			Assert.Equal(2, revised.Count);
			Assert.False(original.HasTrait(Trait2));
		}
		#endregion

		#region constructor tests
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowOnARepeatedTrait()
		{
			// Multiplicity lives inside an answer's schema, never in repeated entries.
			Assert.Throws<ArgumentException>(
				() => new FlexDatum(
					new[]
					{
						new TraitValue(Trait1, "101"),
						new TraitValue(Trait1, "110")
					}
				)
			);
		}
		#endregion

		#region method tests
		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldThrowWhenTheTraitIsAbsent()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 43).Build();

			Assert.Throws<KeyNotFoundException>(() => flexDatum.GetTrait<int>(Trait2, piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void TryGetTrait_ShouldReturnFalseWhenTheTraitIsAbsent()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 43).Build();

			Assert.False(flexDatum.TryGetTrait<int>(Trait2, piedPiper, out int value));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldWorkForAnEnumOverAnIntegralSchema()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(TraitEnum, (int) TestGameType.Deluxe)
				.Build();

			Assert.Equal(TestGameType.Deluxe, flexDatum.GetTrait<TestGameType>(TraitEnum, piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AddTrait_ShouldWorkForAnEnumOverAnIntegralSchema()
		{
			// The caller should not have to cast on the way in when GetTrait does not make them
			// cast on the way out.
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(TraitEnum, TestGameType.Deluxe)
				.Build();

			Assert.Equal(TestGameType.Deluxe, flexDatum.GetTrait<TestGameType>(TraitEnum, piedPiper));
			Assert.Equal((int) TestGameType.Deluxe, flexDatum.GetTrait<int>(TraitEnum, piedPiper));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void GetTrait_ShouldThrowWhenTheModelCannotBeCast()
		{
			IPiedPiper piedPiper = CreatePiedPiper();
			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 43).Build();

			Assert.Throws<InvalidOperationException>(
				() => flexDatum.GetTrait<string>(Trait1, piedPiper)
			);
		}
		#endregion

		#region canonical order tests
		[Fact]
		[Trait("Region", "canonical order")]
		public void TraitValues_ShouldBeInCanonicalOrderRegardlessOfInsertionOrder()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum forwards = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 1).AddTrait(Trait2, 2).AddTrait(Trait3, 3)
				.Build();

			// Insert backwards, and churn the dictionary with an add-and-remove in between,
			// because insertion order is exactly what must not survive into the code.
			FlexDatum backwards = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait3, 3)
				.AddTrait(TraitText, "scratch")
				.AddTrait(Trait2, 2)
				.AddTrait(Trait1, 1)
				.Build()
				.ToBuilder(piedPiper)
				.Build();
			FlexDatumBuilder builder = backwards.ToBuilder(piedPiper);
			builder.RemoveTrait(TraitText);
			backwards = builder.Build();

			Assert.Equal(
				forwards.TraitValues.Select(traitValue => traitValue.TraitId).ToList(),
				backwards.TraitValues.Select(traitValue => traitValue.TraitId).ToList()
			);
			Assert.Equal(
				piedPiper.PackModel<FlexDatum>(forwards, CoreSchema.FlexDatum),
				piedPiper.PackModel<FlexDatum>(backwards, CoreSchema.FlexDatum)
			);
		}
		#endregion

		#region equality tests
		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldBeStructuralAndOrderIndependent()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum a = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 1).AddTrait(Trait2, 2).Build();
			FlexDatum b = new FlexDatumBuilder(piedPiper).AddTrait(Trait2, 2).AddTrait(Trait1, 1).Build();

			Assert.Equal(a, b);
			Assert.True(a == b);
			Assert.Equal(a.GetHashCode(), b.GetHashCode());
		}

		[Fact]
		[Trait("Region", "object methods")]
		public void Equals_ShouldDistinguishDifferentAnswers()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum a = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 1).Build();
			FlexDatum b = new FlexDatumBuilder(piedPiper).AddTrait(Trait1, 2).Build();

			Assert.NotEqual(a, b);
			Assert.True(a != b);
		}
		#endregion

		#region unrecognised trait tests
		[Fact]
		[Trait("Region", "unrecognised traits")]
		public void UndefinedTrait_ShouldBeHeldAndEnumerated()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 43)
				.AddTraitValue(new TraitValue(UndefinedTrait, "110100"))
				.Build();

			Assert.True(flexDatum.HasTrait(UndefinedTrait));
			Assert.Equal(2, flexDatum.TraitValues.Count);
			Assert.True(flexDatum.TryGetTraitCode(UndefinedTrait, out Code code));
			Assert.Equal<Code>("110100", code);
		}

		[Fact]
		[Trait("Region", "unrecognised traits")]
		public void UndefinedTrait_ShouldFailOnlyWhenInterpreted()
		{
			IPiedPiper piedPiper = CreatePiedPiper();

			FlexDatum flexDatum = new FlexDatumBuilder(piedPiper)
				.AddTrait(Trait1, 43)
				.AddTraitValue(new TraitValue(UndefinedTrait, "110100"))
				.Build();

			// The trait we do understand stays readable.
			Assert.Equal(43, flexDatum.GetTrait<int>(Trait1, piedPiper));

			// The one we do not fails here, where a caller asked, and nowhere else.
			Assert.ThrowsAny<Exception>(() => flexDatum.GetTrait<int>(UndefinedTrait, piedPiper));
		}
		#endregion
	}
}
