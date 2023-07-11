using System;
using System.Collections.Generic;

namespace BigRedProf.Data.PackRats
{
	/// <summary>
	/// The <see cref="TokenPackRat{TValue}"/> encodes potentially large models that are repeated
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
	/// <typeparam name="TValue">The model, or token definition, to be tokenized.</typeparam>
	public class TokenPackRat<TValue> : PackRat<TokenValue<TValue>>
	{
		#region fields
		private IDictionary<Code, TValue> _tokenToTokenDefinitionMap;
		private IDictionary<TValue, Code> _tokenDefinitionToTokenMap;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a <see cref="TokenPackRat{TTokenValue}"/>.
		/// </summary>
		/// <param name="piedPiper">The pied piper.</param>
		public TokenPackRat(IPiedPiper piedPiper)
			: base(piedPiper)
		{
			_tokenToTokenDefinitionMap = new Dictionary<Code, TValue>();
			_tokenDefinitionToTokenMap = new Dictionary<TValue, Code>();
		}
		#endregion

		#region methods
		/// <summary>
		/// Defines a new token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="tokenDefinition">The model, or token definition.</param>
		public void DefineToken(Code token,  TValue tokenDefinition)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (_tokenToTokenDefinitionMap.ContainsKey(token))
				throw new ArgumentException("Cannot define the same token twice.", nameof(token));

			_tokenToTokenDefinitionMap[token] = tokenDefinition;
			_tokenDefinitionToTokenMap[tokenDefinition] = token;
		}

		/// <summary>
		/// Checks if a token has been defined.
		/// </summary>
		/// <param name="token">The token to check.</param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		public bool HasTokenDefinition(Code token)
		{
			return _tokenToTokenDefinitionMap.ContainsKey(token);
		}

		/// <summary>
		/// Gets the token value for a given token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The token value.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public TokenValue<TValue> GetTokenValue(Code token)
		{
			if(token == null)
				throw new ArgumentNullException(nameof(token));

			TValue tokenDefinition;
			if (!_tokenToTokenDefinitionMap.TryGetValue(token, out tokenDefinition))
				throw new ArgumentException("Token not defined.", nameof(token));

			TokenValue<TValue> tokenValue = new TokenValue<TValue>();
			tokenValue.Token = token; 
			tokenValue.Value = tokenDefinition;

			return tokenValue;
		}

		/// <summary>
		/// Checks to see if a token has been defined and, provided it has been, returns
		/// the token value.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="tokenValue">
		/// The out parameter in which to return the token value, if it has been defined.
		/// </param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetTokenValue(Code token, out TokenValue<TValue> tokenValue)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			TValue tokenDefinition;
			bool hasTokenDefinition = _tokenToTokenDefinitionMap.TryGetValue(token, out tokenDefinition);

			tokenValue = new TokenValue<TValue>();
			if (hasTokenDefinition)
			{
				tokenValue.Token = token;
				tokenValue.Value = tokenDefinition;
			}

			return hasTokenDefinition;
		}
		#endregion

		#region PackRat methods
		/// <inheritdoc/>
		public override void PackModel(CodeWriter writer, TokenValue<TValue> tokenValue)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(SchemaId.Code);
			codePackRat.PackModel(writer, tokenValue.Token);
		}

		/// <inheritdoc/>
		public override TokenValue<TValue> UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(SchemaId.Code);
			Code token = codePackRat.UnpackModel(reader);

			TokenValue<TValue> tokenValue;
			if (!TryGetTokenValue(token, out tokenValue))
				throw new InvalidOperationException($"Token '{token}' not defined.");

			return tokenValue;
		}
		#endregion
	}
}
