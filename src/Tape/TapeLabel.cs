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
		private TapeLabel(TapeLabel tapeLabelToClone)
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
		#endregion
	}
}
