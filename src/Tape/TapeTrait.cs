namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// Contains the list of core trait identifiers.
	/// </summary>
	public static class TapeTrait
	{
		#region static fields
		/// <summary>
		/// Represents the current position in the tape, in bits. 0 is fully rewound, 1,000,000,000 is
		/// fully fast-forwarded.
		/// </summary>
		public const string TapePosition = "bd25f6b9-628b-4f9c-8328-55d9a0c847f4";
		#endregion
	}
}
