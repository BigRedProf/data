using System;
using System.Collections.Generic;
using System.Linq;

namespace BigRedProf.Data.Core.Internal.PackRats
{
	internal class TextTrailPackRat : PackRat<TextTrail>
	{
		#region constructors
		public TextTrailPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
		}
		#endregion

		#region PackRat methods
		public override void PackModel(CodeWriter writer, TextTrail model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			PiedPiper.PackList<string>(writer, model.Segments.ToList(), CoreSchema.TextUtf8, false, false, ByteAligned.Yes);
		}

		public override TextTrail UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			IList<string> segments = PiedPiper.UnpackList<string>(reader, CoreSchema.TextUtf8, false, false, ByteAligned.Yes);
			TextTrail model = new TextTrail(segments.ToArray());

			return model;
		}
		#endregion
	}
}
