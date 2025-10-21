using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Tape
{
	public class TapeLabel : FlexModel
	{
		#region constructors
		public TapeLabel()
		{
		}
		#endregion

		#region private constructors
		private TapeLabel(FlexModel tapeLabelToClone)
			: base(tapeLabelToClone)
		{
		}
		#endregion

		#region properties
		public Guid TapeId => this.GetTrait<Guid>(CoreTrait.Id);
		public string Name => this.GetTrait<string>(CoreTrait.Name);
		public Multihash ContentMultihash => this.GetTrait<Multihash>(CoreTrait.ContentDigest);
		public Guid SeriesId => this.GetTrait<Guid>(CoreTrait.SeriesId);
		public string SeriesName => this.GetTrait<string>(CoreTrait.SeriesName);
		public int SeriesNumber => this.GetTrait<int>(CoreTrait.SeriesNumber);
		public Multihash SeriesParentMultihash => this.GetTrait<Multihash>(CoreTrait.SeriesParentDigest);
		public Multihash SeriesHeadMultihash => this.GetTrait<Multihash>(CoreTrait.SeriesHeadDigest);
		public int TapePosition => this.GetTrait<int>(TapeTrait.TapePosition);
		#endregion

		#region functions
		public static TapeLabel FromFlexModel(FlexModel flexModel)
		{
			if (flexModel == null)
				throw new ArgumentNullException(nameof(flexModel), "FlexModel cannot be null.");
		
			return new TapeLabel(flexModel);
		}
		#endregion

		#region methods
		public TapeLabel WithTapeId(Guid id)
		{
			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Guid>(CoreTrait.Id, id));
			return tapeLabel;
		}

		public TapeLabel WithSeriesInfo(Guid seriesId, string seriesName, int seriesNumber)
		{
			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Guid>(CoreTrait.SeriesId, seriesId));
			tapeLabel.AddTrait(new Trait<string>(CoreTrait.SeriesName, seriesName));
			tapeLabel.AddTrait(new Trait<int>(CoreTrait.SeriesNumber, seriesNumber));
			return tapeLabel;
		}

		public TapeLabel WithName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<string>(CoreTrait.Name, name));
			return tapeLabel;
		}

		public TapeLabel WithSeriesDescription(string description)
		{
			if (description == null)
				throw new ArgumentNullException(nameof(description));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<string>(TapeTrait.SeriesDescription, description));
			return tapeLabel;
		}

		public TapeLabel WithContentMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Multihash>(CoreTrait.ContentDigest, digest));
			return tapeLabel;
		}

		public TapeLabel WithSeriesParentMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Multihash>(CoreTrait.SeriesParentDigest, digest));
			return tapeLabel;
		}

		public TapeLabel WithSeriesHeadMultihash(Multihash digest)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Multihash>(CoreTrait.SeriesHeadDigest, digest));
			return tapeLabel;
		}

		public TapeLabel WithClientCheckpoint(Code checkpoint)
		{
			if (checkpoint == null)
				throw new ArgumentNullException(nameof(checkpoint));

			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Code>(TapeTrait.ClientCheckpointCode, checkpoint));
			return tapeLabel;
		}

		public TapeLabel WithoutClientCheckpoint()
		{
			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.RemoveTrait(TapeTrait.ClientCheckpointCode);
			return tapeLabel;
		}

		public bool TryGetSeriesDescription(out string description)
		{
			return this.TryGetTrait<string>(TapeTrait.SeriesDescription, out description);
		}

		public bool TryGetClientCheckpoint(out Code checkpoint)
		{
			return this.TryGetTrait<Code>(TapeTrait.ClientCheckpointCode, out checkpoint);
		}
		#endregion
	}
}
