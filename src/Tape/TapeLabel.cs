using BigRedProf.Data.Core;
using System;

namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// A typed view over the <see cref="FlexDatum"/> that labels a tape.
	/// </summary>
	/// <remarks>
	/// A view rather than a subclass. "This flex datum is a tape label" is an expectation a reader
	/// brings, not something the wire carries, and inheritance would claim otherwise. The
	/// underlying flex datum is available as <see cref="FlexDatum"/> and is what actually gets
	/// written; traits this version of the library knows nothing about ride along untouched.
	/// </remarks>
	public class TapeLabel
	{
		#region fields
		private readonly IPiedPiper _piedPiper;
		private readonly FlexDatum _flexDatum;
		#endregion

		#region constructors
		private TapeLabel(IPiedPiper piedPiper, FlexDatum flexDatum)
		{
			_piedPiper = piedPiper;
			_flexDatum = flexDatum;
		}
		#endregion

		#region properties
		/// <summary>
		/// The flex datum this label reads.
		/// </summary>
		public FlexDatum FlexDatum => _flexDatum;

		public Guid TapeId => _flexDatum.GetTrait<Guid>(CoreTrait.Id, _piedPiper);
		public string Name => _flexDatum.GetTrait<string>(CoreTrait.Name, _piedPiper);
		public Multihash ContentDigest => _flexDatum.GetTrait<Multihash>(CoreTrait.ContentDigest, _piedPiper);
		public Guid SeriesId => _flexDatum.GetTrait<Guid>(CoreTrait.SeriesId, _piedPiper);
		public string SeriesName => _flexDatum.GetTrait<string>(CoreTrait.SeriesName, _piedPiper);
		public int SeriesNumber => _flexDatum.GetTrait<int>(CoreTrait.SeriesNumber, _piedPiper);
		public Multihash SeriesParentDigest => _flexDatum.GetTrait<Multihash>(CoreTrait.SeriesParentDigest, _piedPiper);
		public Multihash SeriesHeadDigest => _flexDatum.GetTrait<Multihash>(CoreTrait.SeriesHeadDigest, _piedPiper);
		public int TapePosition => _flexDatum.GetTrait<int>(TapeTrait.TapePosition, _piedPiper);
		#endregion

		#region functions
		/// <summary>
		/// Reads an existing flex datum as a tape label.
		/// </summary>
		public static TapeLabel Over(IPiedPiper piedPiper, FlexDatum flexDatum)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			if (flexDatum == null)
				throw new ArgumentNullException(nameof(flexDatum));

			return new TapeLabel(piedPiper, flexDatum);
		}

		/// <summary>
		/// Creates a tape label with no traits yet.
		/// </summary>
		public static TapeLabel Empty(IPiedPiper piedPiper)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));

			return new TapeLabel(piedPiper, new FlexDatumBuilder(piedPiper).Build());
		}
		#endregion

		#region methods
		public TapeLabel WithTapeId(Guid id)
		{
			return With(builder => builder.AddTrait(CoreTrait.Id, id));
		}

		public TapeLabel WithSeriesInfo(Guid seriesId, string seriesName, int seriesNumber)
		{
			return With(
				builder => builder
					.AddTrait(CoreTrait.SeriesId, seriesId)
					.AddTrait(CoreTrait.SeriesName, seriesName)
					.AddTrait(CoreTrait.SeriesNumber, seriesNumber)
			);
		}

		public TapeLabel WithName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

			return With(builder => builder.AddTrait(CoreTrait.Name, name));
		}

		public TapeLabel WithSeriesDescription(string description)
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			return With(builder => builder.AddTrait(TapeTrait.SeriesDescription, description));
		}

		public TapeLabel WithContentMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			return With(builder => builder.AddTrait(CoreTrait.ContentDigest, digest));
		}

		public TapeLabel WithSeriesParentMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			return With(builder => builder.AddTrait(CoreTrait.SeriesParentDigest, digest));
		}

		public TapeLabel WithSeriesHeadMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			return With(builder => builder.AddTrait(CoreTrait.SeriesHeadDigest, digest));
		}

		public TapeLabel WithClientCheckpoint(Code checkpoint)
		{
			if (checkpoint == null)
				throw new ArgumentNullException(nameof(checkpoint));

			return With(builder => builder.AddTrait(TapeTrait.ClientCheckpointCode, checkpoint));
		}

		public TapeLabel WithTapePosition(int position)
		{
			return With(builder => builder.AddTrait(TapeTrait.TapePosition, position));
		}

		public TapeLabel WithoutClientCheckpoint()
		{
			return With(builder => builder.RemoveTrait(TapeTrait.ClientCheckpointCode));
		}

		/// <summary>
		/// Checks whether a trait is present on the underlying flex datum.
		/// </summary>
		public bool HasTrait(AttributeFriendlyGuid traitId)
		{
			return _flexDatum.HasTrait(traitId);
		}

		/// <summary>
		/// Interprets a trait on the underlying flex datum.
		/// </summary>
		public M GetTrait<M>(AttributeFriendlyGuid traitId)
		{
			return _flexDatum.GetTrait<M>(traitId, _piedPiper);
		}

		/// <summary>
		/// Interprets a trait on the underlying flex datum if it is present.
		/// </summary>
		public bool TryGetTrait<M>(AttributeFriendlyGuid traitId, out M value)
		{
			return _flexDatum.TryGetTrait<M>(traitId, _piedPiper, out value);
		}

		public bool TryGetSeriesDescription(out string description)
		{
			return _flexDatum.TryGetTrait<string>(TapeTrait.SeriesDescription, _piedPiper, out description);
		}

		public bool TryGetContentLength(out int contentLength)
		{
			return _flexDatum.TryGetTrait<int>(CoreTrait.ContentLength, _piedPiper, out contentLength);
		}

		public bool TryGetClientCheckpoint(out Code checkpoint)
		{
			return _flexDatum.TryGetTrait<Code>(TapeTrait.ClientCheckpointCode, _piedPiper, out checkpoint);
		}
		#endregion

		#region private methods
		private TapeLabel With(Action<FlexDatumBuilder> revise)
		{
			FlexDatumBuilder builder = _flexDatum.ToBuilder(_piedPiper);
			revise(builder);

			return new TapeLabel(_piedPiper, builder.Build());
		}
		#endregion
	}
}
