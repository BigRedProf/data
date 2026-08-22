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
		public override void PackModel(CodeWriter writer, Datum model)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			PiedPiper.PackModel<Guid>(writer, model.SchemaId, CoreSchema.Guid);
			PiedPiper.PackModel<int>(writer, model.Code.Length, CoreSchema.EfficientWholeNumber31);
			writer.WriteCode(model.Code);
		}

		public override Datum UnpackModel(CodeReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			Guid schemaId = PiedPiper.UnpackModel<Guid>(reader, CoreSchema.Guid);
			int length = PiedPiper.UnpackModel<int>(reader, CoreSchema.EfficientWholeNumber31);
			Code code = reader.Read(length);

			return new Datum(schemaId, code);
		}
		#endregion
	}
}
