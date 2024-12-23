using System;
using System.Collections.Generic;
using System.Linq;

namespace BigRedProf.Data.Internal.PackRats
{
	internal class FlexModelPackRat : PackRat<FlexModel>
	{
		#region constructors
		public FlexModelPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, FlexModel model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			//Dictionary<Guid, UntypedTrait> untypedTraits = model.InternalUntypedTraits;
			IList<UntypedTrait> untypedTraits = model.InternalUntypedTraits.Values.ToList();
			int traitCount = untypedTraits.Count;

			// encode each trait
			Code[] encodedTraits = new Code[traitCount];
			for (int i = 0; i < traitCount; ++i)
			{
				UntypedTrait untypedTrait = untypedTraits[i];
				Guid schema = PiedPiper.GetTraitDefinition(untypedTrait.TraitId).SchemaId;
				Code code = ((PiedPiper)PiedPiper).EncodeModel(untypedTrait.Model, schema);
				encodedTraits[i] = code;
			}

			// first pack the trait count
			PiedPiper.PackModel<int>(writer, traitCount, CoreSchema.EfficientWholeNumber31);

			if (traitCount > 0)
			{
				// then pack all n trait identifiers and encoded model lengths
				writer.AlignToNextByteBoundary();
				for(int i = 0; i < traitCount; ++i)
				{
					PiedPiper.PackModel<Guid>(writer, untypedTraits[i].TraitId, CoreSchema.Guid);
					PiedPiper.PackModel<int>(writer, encodedTraits[i].Length, CoreSchema.Int32);
				}

				// and finally pack all n trait models
				writer.AlignToNextByteBoundary();
				for (int i = 0; i < traitCount; ++i)
					writer.WriteCode(encodedTraits[i]);
			}
		}

		public override FlexModel UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			// first unpack the trait count
			int traitCount = PiedPiper.UnpackModel<int>(reader, CoreSchema.EfficientWholeNumber31);

			// then unpack all n trait identifiers and encoded model lengths
			FlexModel model = new FlexModel(traitCount);
			if (traitCount > 0)
			{
				reader.AlignToNextByteBoundary();
				IList<Guid> traitIds = new List<Guid>(traitCount);
				IList<int> encodedModelLengths = new List<int>(traitCount);
				for (int i = 0; i < traitCount; ++i)
				{
					Guid traitId = PiedPiper.UnpackModel<Guid>(reader, CoreSchema.Guid);
					int encodedModelLength = PiedPiper.UnpackModel<int>(reader, CoreSchema.Int32);

					traitIds.Add(traitId);
					encodedModelLengths.Add(encodedModelLength);
				}

				// and finally unpack all n trait models
				reader.AlignToNextByteBoundary();
				for (int i = 0; i < traitCount; ++i)
				{
					Guid traitId = traitIds[i];
					int encodedModelLength = encodedModelLengths[i];

					Code encodedModel = reader.Read(encodedModelLength);
					UntypedTrait encodedTrait = new UntypedTrait();
					encodedTrait.TraitId = traitId;
					encodedTrait.Model = encodedModel;

					model.InternalUntypedTraits.Add(traitId, encodedTrait);
				}
			}

			return model;
		}
		#endregion
	}
}
