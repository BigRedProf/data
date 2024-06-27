using System;
using System.Collections.Generic;

namespace BigRedProf.Data
{
	/// <summary>
	/// A <see cref="Tokenizer{TModel}"/> encodes potentially large models that are repeated
	/// often as small <see cref="Code"/> values called tokens. This helps to achieve efficiency by
	/// packing the models into small tokens while still working with them as natural models.
	/// </summary>
	/// <typeparam name="TModel">The model, or token definition, to be tokenized.</typeparam>
	public class Tokenizer<TModel>
	{
		#region fields
		private IDictionary<Code, TModel> _tokenToModelMap;
		private IDictionary<TModel, Code> _modelToTokenMap;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a <see cref="Tokenizer{TModel}"/>.
		/// </summary>
		public Tokenizer()
		{
			_tokenToModelMap = new Dictionary<Code, TModel>();
			_modelToTokenMap = new Dictionary<TModel, Code>();
		}
		#endregion

		#region methods
		/// <summary>
		/// Defines a new token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">The model, or token definition.</param>
		public void DefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (_tokenToModelMap.ContainsKey(token))
				throw new ArgumentException("Cannot define the same token twice.", nameof(token));

			_tokenToModelMap[token] = model;
			_modelToTokenMap[model] = token;
		}

		/// <summary>
		/// Checks if a token has been defined.
		/// </summary>
		/// <param name="token">The token to check.</param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		public bool HasTokenDefinition(Code token)
		{
			return _tokenToModelMap.ContainsKey(token);
		}

		/// <summary>
		/// Gets the tokenized model for a given token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The tokenized model.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public TokenizedModel<TModel> GetTokenizedModel(Code token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			TModel model;
			if (!_tokenToModelMap.TryGetValue(token, out model))
				throw new ArgumentException("Token not defined.", nameof(token));

			TokenizedModel<TModel> tokenizedModel = new TokenizedModel<TModel>();
			tokenizedModel.Token = token;
			tokenizedModel.Model = model;

			return tokenizedModel;
		}

		/// <summary>
		/// Checks to see if a token has been defined and, provided it has been, returns
		/// the model it represents.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">
		/// The out parameter in which to return the model, if it has been defined.
		/// </param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public bool TryGetModel(Code token, out TokenizedModel<TModel> model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			TModel tokenDefinition;
			bool hasTokenDefinition = _tokenToModelMap.TryGetValue(token, out tokenDefinition);

			model = new TokenizedModel<TModel>();
			if (hasTokenDefinition)
			{
				model.Token = token;
				model.Model = tokenDefinition;
			}

			return hasTokenDefinition;
		}
		#endregion
	}
}
