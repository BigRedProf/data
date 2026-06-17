using BigRedProf.Data.Core.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

			foreach(string segment in trail)
			{
				if(string.IsNullOrEmpty(segment))
					throw new ArgumentException("Trail segments cannot be null or empty.", nameof(trail));
			}

			_trail = new List<string>(trail);
		}
		#endregion

		#region properties
		public IReadOnlyList<string> Segments => _trail.AsReadOnly();
		#endregion

		#region methods
		public TextTrail Append(string segment)
		{
			if(string.IsNullOrEmpty(segment))
				throw new ArgumentException("Segment cannot be null or empty.", nameof(segment));

			string[] segments = new string[_trail.Count + 1];
			for(int i = 0; i < _trail.Count; ++i)
				segments[i] = _trail[i];
			segments[_trail.Count] = segment;

			return new TextTrail(segments);
		}
		public Multihash GetMultihash(MultihashAlgorithm algorithm)
		{
			Code encodedTextTrail = _piedPiper.EncodeModel(this, CoreSchema.TextTrail);
			return Multihash.FromCode(encodedTextTrail, algorithm);
		}
		#endregion

		#region object methods
		public override string ToString()
		{
			return ToStringRepresentation(this, '/');
		}
		#endregion

		#region functions
		public static string ToStringRepresentation(TextTrail textTrail, char separator)
		{
			if(textTrail == null)
				throw new ArgumentNullException(nameof(textTrail));

			StringBuilder sb = new StringBuilder();
			for(int i = 0; i < textTrail.Segments.Count; ++i)
			{
				string segment = textTrail.Segments[i];

				if(i > 0)
					sb.Append(separator);

				for(int j = 0; j < segment.Length; ++j)
				{
					char c = segment[j];
					if (c == separator)
					{
						// If the separator character appears in the segment, we need to escape it by doubling it.
						sb.Append(separator);
						sb.Append(separator);
					}
					else
					{
						// Otherwise, we can just append the character as is.
						sb.Append(c);
					}
				}
			}
			return sb.ToString();
		}

		public static TextTrail FromStringRepresentation(string stringRepresentation, char separator)
		{
			Debug.Assert(stringRepresentation != null, "stringRepresentation cannot be null.");
			List<string> segments = new List<string>();
			StringBuilder currentSegment = new StringBuilder();
			for(int i = 0; i < stringRepresentation.Length; ++i)
			{
				char c = stringRepresentation[i];
				if (c == separator)
				{
					if (i + 1 < stringRepresentation.Length && stringRepresentation[i + 1] == separator)
					{
						// If we encounter a separator character followed by another separator character, this is an escaped separator, so we add a single separator to the current segment and skip the next character.
						currentSegment.Append(separator);
						i++;
					}
					else
					{
						// Otherwise, this is a segment separator, so we add the current segment to the list of segments and start a new segment.
						segments.Add(currentSegment.ToString());
						currentSegment.Clear();
					}
				}
				else
				{
					// If it's not a separator character, we just add it to the current segment.
					currentSegment.Append(c);
				}
			}
			// Add the last segment if there is one.
			if (currentSegment.Length > 0)
			{
				segments.Add(currentSegment.ToString());
			}
			return new TextTrail(segments.ToArray());
		}
		#endregion
	}
}
