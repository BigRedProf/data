namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Contains the list of core trait identifiers.
	/// </summary>
	public static class CoreTrait
	{
		#region static fields
		/// <summary>
		/// Associates a globally unique identifier with something.
		/// </summary>
		public const string Guid = "7759e69c-15cd-44ee-a02e-3f29759fbe35";

		/// <summary>
		/// Associates a name with the series that something is a part of.
		/// </summary>
		public const string SeriesName = "0bc66d67-3976-436d-90a3-c4faa811ab34";

		/// <summary>
		/// Associates a sequential number with something in a series.
		/// </summary>
		public const string SeriesNumber = "ce22d178-02ec-470c-b8de-60c71961dec2";

		/// <summary>
		/// Associates the SHA-256 hash of something with it.
		/// </summary>
		public const string Sha256Hash = "93a2dbed-065e-4f64-8ab0-8448a82a30ea";

		/// <summary>
		/// Associates the running SHA-256 hash of something in a series of things.
		/// </summary>
		public const string RunningSha256Hash = "8c110105-4569-4a7c-a0e9-9e417ac252d2";
		#endregion
	}
}
