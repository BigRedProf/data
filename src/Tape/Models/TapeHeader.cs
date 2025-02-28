namespace BigRedProf.Data.Tape.Models
{
	public class TapeHeader
	{
		#region properties
		/// <summary>
		/// The version of this tape. Should always be 1. But if we ever
		/// need breaking changes, this field will be useful.
		/// </summary>
		public int Version {	get; set; }

		/// <summary>
		/// The amount of space allocated for the packed label. We currently
		/// support values of 1000, 5000, 25000, and 125000.
		/// </summary>
		public int BytesAllocatedForLabel { get; set; }

		// TODO: Not sure if we want this here yet or not. One one hand, we
		// "need" it to implement TapeHeaderPackRat.PackModel if it's going
		// to include things like Guid and SeriesName as those traits live
		// on the Label. On the other hand, it might be faster if we don't
		// have to modify the header every time somebody changes the label.
		public FlexModel Label { get; set; } = default!;
		#endregion
	}
}
