using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Core
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
		/// Defines, or redefines, a token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">The model, or token definition.</param>
		public void DefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			_tokenToModelMap[token] = model;
			_modelToTokenMap[model] = token;
		}

		/// <summary>
		/// Checks if a token has been defined.
		/// </summary>
		/// <param name="token">The token to check.</param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		public bool IsTokenDefined(Code token)
		{
			return _tokenToModelMap.ContainsKey(token);
		}

		/// <summary>
		/// Checks if a model has been definied.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		public bool IsModelTokenized(TModel model)
		{
			return _modelToTokenMap.ContainsKey(model);
		}

		/// <summary>
		/// Gets the model for a given token.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <returns>The tokenized model.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public TModel GetModel(Code token)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			TModel model;
			if (!_tokenToModelMap.TryGetValue(token, out model))
				throw new ArgumentException("Token not defined.", nameof(token));

			return model;
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
		public bool TryGetModel(Code token, out TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			bool hasTokenDefinition = _tokenToModelMap.TryGetValue(token, out model);

			return hasTokenDefinition;
		}

		/// <summary>
		/// Gets the token for a given model.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <returns>The token corresponding to the model.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the model is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the model has not been tokenized.</exception>
		public Code GetToken(TModel model)
		{
			if(model == null)
				throw new ArgumentNullException(nameof(model));

			Code token;
			if(!_modelToTokenMap.TryGetValue(model, out token))
				throw new ArgumentException("Model not tokenized.", nameof(model));

			return token;
		}

		/// <summary>
		/// Checks to see if a model has been tokenized and, provided it has been, returns
		/// the token.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <param name="token">
		/// The out parameter in which to return the token, if the model has been
		/// tokenized.
		/// </param>
		/// <returns>The token.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the model is null.</exception>
		public bool TryGetToken(TModel model, out Code token)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (!_modelToTokenMap.TryGetValue(model, out token))
				return false;

			return true;
		}
		#endregion
	}
}
