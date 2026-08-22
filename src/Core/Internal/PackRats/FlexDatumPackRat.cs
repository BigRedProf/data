using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	/// <summary>
	/// Packs a <see cref="FlexDatum"/> as a count, then every trait identifier and code length,
	/// then every code.
	/// </summary>
	/// <remarks>
	/// Two properties of this layout are guarantees, not accidents.
	///
	/// All trait identifiers and lengths are written together ahead of all payloads, so a reader
	/// can learn what a flex datum has without decoding what it says.
	///
	/// Nothing here consults the trait registry or the pack rat registry. A trait this reader has
	/// never heard of is read as a code and kept, exactly as written -- which is what lets an
	/// unrecognised trait be skipped, enumerated, compared, and carried forward. Interpretation
	/// happens later, when a caller asks for a trait by identifier.
	///
	/// Traits are written in the flex datum's canonical order, so equal traits produce equal
	/// codes.
	/// </remarks>
	internal class FlexDatumPackRat : PackRat<FlexDatum>
	{
		#region constructors
		public FlexDatumPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, FlexDatum model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			IReadOnlyList<TraitValue> traitValues = model.TraitValues;
			int traitCount = traitValues.Count;

			PiedPiper.PackModel<int>(writer, traitCount, CoreSchema.EfficientWholeNumber31);

			if (traitCount > 0)
			{
				writer.AlignToNextByteBoundary();
				for (int i = 0; i < traitCount; ++i)
				{
					PiedPiper.PackModel<Guid>(writer, traitValues[i].TraitId, CoreSchema.Guid);
					PiedPiper.PackModel<int>(
						writer,
						traitValues[i].Code.Length,
						CoreSchema.EfficientWholeNumber31
					);
				}

				writer.AlignToNextByteBoundary();
				for (int i = 0; i < traitCount; ++i)
					writer.WriteCode(traitValues[i].Code);
			}
		}

		public override FlexDatum UnpackModel(CodeReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			int traitCount = PiedPiper.UnpackModel<int>(reader, CoreSchema.EfficientWholeNumber31);
			if (traitCount == 0)
				return new FlexDatum(Array.Empty<TraitValue>());

			reader.AlignToNextByteBoundary();
			Guid[] traitIds = new Guid[traitCount];
			int[] codeLengths = new int[traitCount];
			for (int i = 0; i < traitCount; ++i)
			{
				traitIds[i] = PiedPiper.UnpackModel<Guid>(reader, CoreSchema.Guid);
				codeLengths[i] = PiedPiper.UnpackModel<int>(reader, CoreSchema.EfficientWholeNumber31);
			}

			reader.AlignToNextByteBoundary();
			TraitValue[] traitValues = new TraitValue[traitCount];
			for (int i = 0; i < traitCount; ++i)
				traitValues[i] = new TraitValue(traitIds[i], reader.Read(codeLengths[i]));

			return new FlexDatum(traitValues);
		}
		#endregion
	}
}
