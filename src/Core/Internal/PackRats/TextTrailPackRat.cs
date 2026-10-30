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
		public override void PackModel(BitStream stream, TextTrail model)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			PiedPiper.PackList<string>(stream, model.Segments.ToList(), CoreSchema.TextUtf8, false, false, ByteAligned.Yes);
		}

		public override TextTrail UnpackModel(BitStream stream)
		{
			if(stream == null)
				throw new ArgumentNullException(nameof(stream));

			// allowNullLists is false, so the list is never null -- a correlation between an
			// argument and a return that the static type cannot express.
			IList<string> segments =
				PiedPiper.UnpackList<string>(stream, CoreSchema.TextUtf8, false, false, ByteAligned.Yes)!;
			TextTrail model = new TextTrail(segments.ToArray());

			return model;
		}
		#endregion
	}
}
