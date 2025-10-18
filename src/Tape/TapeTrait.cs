namespace BigRedProf.Data.Tape
{
	/// <summary>
	/// Contains the list of core trait identifiers.
	/// </summary>
	public static class TapeTrait
	{
		/// <summary>
		/// Allows backup clients to store arbitrary checkpoints on tape labels to support incremental
		/// backups.
		/// </summary>
		public const string ClientCheckpointCode = "1522097a-9ab5-44d3-86ea-3635c4701bcc";

		#region static fields
		/// <summary>
		/// Represents the current position in the tape, in bits. 0 is fully rewound, 1,000,000,000 is
		/// fully fast-forwarded.
		/// </summary>
		public const string TapePosition = "bd25f6b9-628b-4f9c-8328-55d9a0c847f4";
		#endregion
	}
}
