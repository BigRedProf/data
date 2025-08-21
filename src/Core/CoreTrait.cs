namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Contains the list of core trait identifiers.
	/// </summary>
	public static class CoreTrait
	{
		#region static fields
		/// <summary>
		/// Uniquely identifies this model.
		/// </summary>
		public const string Id = "7759e69c-15cd-44ee-a02e-3f29759fbe35";

		/// <summary>
		/// Serves as a, not necessarily unique, name for this model.
		/// </summary>
		public const string Name = "0bc66d67-3976-436d-90a3-c4faa811ab34";

		/// <summary>
		/// Serves as the primary content, or payload, of this model.
		/// </summary>
		public const string Content = "ce22d178-02ec-470c-b8de-60c71961dec2";

		/// <summary>
		/// Provides a digest, or cryptographic hash, of this model's content.
		/// </summary>
		public const string ContentDigest = "93a2dbed-065e-4f64-8ab0-8448a82a30ea";

		/// <summary>
		/// Used to determine the length of the content in bits.
		/// </summary>
		public const string ContentLength = "6f182156-5ac4-4670-a1da-0d5339f64509";

		/// <summary>
		/// Uniquely identifies the series to which this model belongs.
		/// </summary>
		public const string SeriesId = "9080538a-aafc-4ab9-a90f-e1c0d2d3f814";

		/// <summary>
		/// Serves as a, not necessarily unique, name for the series to which this model belongs.
		/// </summary>
		public const string SeriesName = "cbeabd91-8580-45ed-97d4-c797c36d0611";

		/// <summary>
		/// Identifies the series 1-based number of this model within its series.
		public const string SeriesNumber = "9866367b-f1ae-4699-b123-32691149488b";

		/// <summary>
		/// Provides a running hash of this series' content.
		/// </summary>
		/// <remarks>
		/// This series hash represents the running hash of all content in this series
		/// up to and including this model. This value can be computed by generating
		/// the hash of SeriesParentHash and ContentHash.
		/// </remarks>
		public const string SeriesHeadDigest = "8c110105-4569-4a7c-a0e9-9e417ac252d2";

		/// <summary>
		/// Represents the digest of the parent content in this series. It should be
		/// equal to the <see cref="ContentDigest"/> of the previous model in the series.
		/// </summary>
		public const string SeriesParentDigest = "35c4fbf0-d0e9-4e5c-822e-22bd4a64eb30";
		#endregion
	}
}
