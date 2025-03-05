namespace BigRedProf.Data.Core
{
	public struct TokenizedModel<TModel>
	{
		#region properties
		/// <summary>
		/// The token.
		/// </summary>
		public Code Token { get; set; }

		/// <summary>
		/// The model represented by the token.
		/// </summary>
		public TModel Model { get; set; }
		#endregion
	}
}
