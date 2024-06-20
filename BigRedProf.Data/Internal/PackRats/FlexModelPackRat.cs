using System;
using System.Collections.Generic;

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
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Dictionary<string, EncodedTrait> encodedTraits = model.InternalEncodedTraits;
			int traitCount = encodedTraits.Count;

			// first pack the trait count
			PiedPiper.PackModel<int>(writer, traitCount, SchemaId.EfficientWholeNumber31);

			// then pack all n trait identifiers and encoded model lengths
			writer.AlignToNextByteBoundary();
			foreach(KeyValuePair<string, EncodedTrait> pair in encodedTraits)
			{
				EncodedTrait encodedTrait = pair.Value;
				PiedPiper.PackModel<Guid>(writer, new Guid(encodedTrait.TraitId), SchemaId.Guid);
				PiedPiper.PackModel<int>(writer, encodedTrait.EncodedModel.Length, SchemaId.Int32);
			}

			// and finally pack all n trait models
			writer.AlignToNextByteBoundary();
			foreach (KeyValuePair<string, EncodedTrait> pair in encodedTraits)
			{
				EncodedTrait encodedTrait = pair.Value;
				writer.WriteCode(encodedTrait.EncodedModel);
			}
		}

		public override FlexModel UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			// first unpack the trait count
			int traitCount = PiedPiper.UnpackModel<int>(reader, SchemaId.EfficientWholeNumber31);

			// then unpack all n trait identifiers and encoded model lengths
			reader.AlignToNextByteBoundary();
			IList<Guid> traitIds = new List<Guid>(traitCount);
			IList<int> encodedModelLengths = new List<int>(traitCount);
			for(int i = 0; i < traitCount; ++i)
			{
				Guid traitId = PiedPiper.UnpackModel<Guid>(reader, SchemaId.Guid);
				int encodedModelLength = PiedPiper.UnpackModel<int>(reader, SchemaId.Int32);

				traitIds.Add(traitId);
				encodedModelLengths.Add(encodedModelLength);
			}

			// and finally unpack all n trait models
			FlexModel model = new FlexModel(traitCount);
			reader.AlignToNextByteBoundary();
			for (int i = 0; i < traitCount; ++i)
			{
				Guid traitId = traitIds[i];
				int encodedModelLength = encodedModelLengths[i];

				Code encodedModel = reader.Read(encodedModelLength);
				EncodedTrait encodedTrait = new EncodedTrait();
				encodedTrait.TraitId = traitId.ToString();
				encodedTrait.EncodedModel = encodedModel;

				model.InternalEncodedTraits.Add(traitId.ToString(), encodedTrait);
			}

			return model;
		}
		#endregion
	}
}
