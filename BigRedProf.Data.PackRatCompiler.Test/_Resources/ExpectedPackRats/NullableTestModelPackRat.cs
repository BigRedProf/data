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
			PiedPiper.PackNullableModel<string>(writer, model.ExplicitlyNullableField, "9CDF52B4-4C47-4B6D-BC17-34F33312B7A7", ByteAligned.No);

			// ExplicitlyNonNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ExplicitlyNonNullableField);

			// ImplicitlyNullableField
			PiedPiper.PackNullableModel<string>(writer, model.ImplicitlyNullableField, "9CDF52B4-4C47-4B6D-BC17-34F33312B7A7", ByteAligned.No);

			// ImplicitlyNonNullableField
			PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.ImplicitlyNonNullableField);
		}

		public override NullableTestModel UnpackModel(CodeReader reader)
		{
			NullableTestModel model = new NullableTestModel();

			// ExplicitlyNullableField
			model.ExplicitlyNullableField = PiedPiper.UnpackNullableModel<string>(reader, PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7"), ByteAligned.No);

			// ExplicitlyNonNullableField
			model.ExplicitlyNonNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			// ImplicitlyNullableField
			model.ImplicitlyNullableField = PiedPiper.UnpackNullableModel<string>(reader, PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7"), ByteAligned.No);

			// ImplicitlyNonNullableField
			model.ImplicitlyNonNullableField = PiedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			return model;
		}
	}
}
