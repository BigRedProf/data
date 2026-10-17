using System;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	/// <summary>
	/// Packs a <see cref="Datum"/> as its schema identifier, the length of its code, and then the
	/// code itself.
	/// </summary>
	/// <remarks>
	/// The length is what lets a reader step over a datum whose schema it does not recognise, so
	/// this pack rat never consults the pack rat registry. That is the whole point: an unreadable
	/// datum stays opaque instead of failing the stream around it.
	///
	/// The length is not a field anyone supplies. It is taken from the code, so it cannot
	/// disagree with what was actually written.
	/// </remarks>
	internal class DatumPackRat : PackRat<Datum>
	{
		#region constructors
		public DatumPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(BitStream stream, Datum model)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			PiedPiper.PackModel<Guid>(stream, model.SchemaId, CoreSchema.Guid);
			PiedPiper.PackModel<int>(stream, model.Code.Length, CoreSchema.EfficientWholeNumber31);
			stream.WriteBits(model.Code);
		}

		public override Datum UnpackModel(BitStream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));

			Guid schemaId = PiedPiper.UnpackModel<Guid>(stream, CoreSchema.Guid);
			int length = PiedPiper.UnpackModel<int>(stream, CoreSchema.EfficientWholeNumber31);
			Code code = stream.ReadBits(length);

			return new Datum(schemaId, code);
		}
		#endregion
	}
}
