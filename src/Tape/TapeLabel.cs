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
		#endregion

		#region methods
		public TapeLabel WithTapeId(Guid id)
		{
			TapeLabel tapeLabel = new TapeLabel(this);
			tapeLabel.AddTrait(new Trait<Guid>(CoreTrait.Id, id));
			return tapeLabel;
		}
		#endregion
	}
}
