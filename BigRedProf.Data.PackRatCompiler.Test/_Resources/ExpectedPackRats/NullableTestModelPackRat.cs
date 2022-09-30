﻿// <auto-generated/>

using BigRedProf.Data;

namespace BigRedProf.Data.PackRatCompiler.Test._Resources.Models
{
	public sealed class NullableTestModelPackRat : PackRat<NullableTestModel>
	{
		private readonly IPiedPiper _piedPiper;

		public NullableTestModelPackRat(IPiedPiper piedPiper)
		{
			_piedPiper = piedPiper;
		}

		public override void PackModel(CodeWriter writer, NullableTestModel model)
		{
			// NullableField
			_piedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.NullableField);

			// NonNullableField
			_piedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.PackModel(writer, model.NonNullableField);
		}

		public override NullableTestModel UnpackModel(CodeReader reader)
		{
			NullableTestModel model = default;

			// NullableField
			model.NullableField = _piedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			// NonNullableField
			model.NonNullableField = _piedPiper.GetPackRat<string>("9CDF52B4-4C47-4B6D-BC17-34F33312B7A7")
				.UnpackModel(reader);

			return model;
		}
	}
}