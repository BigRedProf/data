namespace BigRedProf.Data
{
	public struct TokenValue<TValue>
	{
		#region properties
		/// <summary>
		/// The token.
		/// </summary>
		public Code Token { get; set; }

		/// <summary>
		/// The value, or token definition.
		/// </summary>
		public TValue Value { get; set; }
		#endregion
	}
}
