﻿// <auto-generated/>

using BigRedProf.Data;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	[AssemblyPackRat("b8a0492a-c5e6-4955-819b-f47797005105")]
	public sealed class NullableTestModelPackRat : PackRat<NullableTestModel>
	{
		public NullableTestModelPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}

		public override void PackModel(CodeWriter writer, NullableTestModel model)
		{
			// ExplicitlyNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ExplicitlyNullableField);

			// ExplicitlyNonNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ExplicitlyNonNullableField);

			// ImplicitlyNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ImplicitlyNullableField);

			// ImplicitlyNonNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ImplicitlyNonNullableField);
		}

		public override NullableTestModel UnpackModel(CodeReader reader)
		{
			NullableTestModel model = new NullableTestModel();

			// ExplicitlyNullableField
			model.ExplicitlyNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			// ExplicitlyNonNullableField
			model.ExplicitlyNonNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			// ImplicitlyNullableField
			model.ImplicitlyNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			// ImplicitlyNonNullableField
			model.ImplicitlyNonNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			return model;
		}
	}
}
