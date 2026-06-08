using BigRedProf.Data.Core.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core
{
	public class TextTrail
	{
		#region static fields
		private static readonly IPiedPiper _piedPiper;
		#endregion

		#region fields
		private readonly List<string> _trail;
		#endregion

		#region class constructors
		static TextTrail()
		{
			_piedPiper = new PiedPiper();
			PackRat<TextTrail> packRat = new TextTrailPackRat(_piedPiper);
			_piedPiper.RegisterPackRat(new EfficientWholeNumber31PackRat(_piedPiper), CoreSchema.EfficientWholeNumber31);
			_piedPiper.RegisterPackRat(new TextPackRat(_piedPiper, Encoding.UTF8), CoreSchema.TextUtf8);
			_piedPiper.RegisterPackRat(packRat, CoreSchema.TextTrail);
		}
		#endregion

		#region constructors
		public TextTrail(params string[] trail)
		{
			if(trail == null)
				throw new ArgumentNullException(nameof(trail));

			if(trail.Length == 0)
				throw new ArgumentException("Trail must contain at least one element.", nameof(trail));

			_trail = new List<string>(trail);
		}
		#endregion

		#region properties
		public IReadOnlyList<string> Segments => _trail.AsReadOnly();
		#endregion

		#region methods
		public Multihash GetMultihash(MultihashAlgorithm algorithm)
		{
			Code encodedTextTrail = _piedPiper.EncodeModel(this, CoreSchema.TextTrail);
			return Multihash.FromCode(encodedTextTrail, algorithm);
		}
		#endregion
	}
}
