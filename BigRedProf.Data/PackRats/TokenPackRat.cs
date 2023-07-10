using System;
using System.Collections.Generic;

namespace BigRedProf.Data.PackRats
{
	/// <summary>
	/// The <see cref="TokenPackRat{TTokenDef}"/> encodes potentially large models that are repeated
	/// often as small <see cref="Code"/> values called tokens. This helps to achieve efficiency by
	/// packing the models into small tokens while still working with them as natural models.
	/// </summary>
	/// <remarks>
	/// Clients can register multiple token pack rats with the pied piper. For static usage, where all
	/// the possible models are known at compile time, the client can immediately define all tokens
	/// at registration time. Furthermore, the client can use efficient data structures and algorithms,
	/// such as Huffman encoding, to select tokens. For dynamic usage, where new tokens for new
	/// models can be added over time, the client must take care to ensure tokens are unique and
	/// each token pack rat has defined a model before it encounters that model's token.
	/// </remarks>
	/// <typeparam name="TTokenDef">The model, or token definition, to be tokenized.</typeparam>
	public class TokenPackRat<TTokenDef> : PackRat<TTokenDef>
	{
		#region fields
		private IDictionary<Code, TTokenDef> _tokenToTokenDefinitionMap;
		private IDictionary<TTokenDef, Code> _tokenDefinitionToTokenMap;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a <see cref="TokenPackRat{TTokenDef}"/>.
		/// </summary>
		/// <param name="piedPiper">The pied piper.</param>
		public TokenPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
			_tokenToTokenDefinitionMap = new Dictionary<Code, TTokenDef>();
			_tokenDefinitionToTokenMap = new Dictionary<TTokenDef, Code>();
		}
		#endregion

		#region methods
		/// <summary>
		/// Defines a new token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="tokenDefinition">The model, or token definition.</param>
		public void DefineToken(Code token,  TTokenDef tokenDefinition)
		{
			if (_tokenToTokenDefinitionMap.ContainsKey(token))
				throw new ArgumentException("Cannot define the same token twice.", nameof(token));

			_tokenToTokenDefinitionMap[token] = tokenDefinition;
			_tokenDefinitionToTokenMap[tokenDefinition] = token;
		}
		#endregion

		#region PackRat methods
		/// <inheritdoc/>
		public override void PackModel(CodeWriter writer, TTokenDef tokenDefinition)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code token;
			if (!_tokenDefinitionToTokenMap.TryGetValue(tokenDefinition, out token))
				throw new InvalidOperationException($"Token not defined for '{tokenDefinition}'.");

			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(SchemaId.Code);
			codePackRat.PackModel(writer, token);
		}

		/// <inheritdoc/>
		public override TTokenDef UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(SchemaId.Code);
			Code token = codePackRat.UnpackModel(reader);

			TTokenDef tokenDefinition;
			if (!_tokenToTokenDefinitionMap.TryGetValue(token, out tokenDefinition))
				throw new InvalidOperationException($"Token '{token}' not defined.");

			return tokenDefinition;
		}
		#endregion
	}
}
