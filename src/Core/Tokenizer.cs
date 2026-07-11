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
		/// Creates a <see cref="Tokenizer{TModel}"/> that compares models using their
		/// default equality.
		/// </summary>
		public Tokenizer()
		{
			_tokenToModelMap = new Dictionary<Code, TModel>();
			_modelToTokenMap = new Dictionary<TModel, Code>();
		}

		/// <summary>
		/// Creates a <see cref="Tokenizer{TModel}"/> that compares models using the specified
		/// comparer. This allows entity models to be tokenized by identity without each model
		/// class having to override <see cref="object.Equals(object)"/>.
		/// </summary>
		/// <param name="modelComparer">The comparer used to match models.</param>
		public Tokenizer(IEqualityComparer<TModel> modelComparer)
		{
			if (modelComparer == null)
				throw new ArgumentNullException(nameof(modelComparer));

			_tokenToModelMap = new Dictionary<Code, TModel>();
			_modelToTokenMap = new Dictionary<TModel, Code>(modelComparer);
		}

		/// <summary>
		/// Creates a <see cref="Tokenizer{TModel}"/> that compares models by the identity the
		/// specified selector extracts (typically an entity's Id). Two model instances with the
		/// same identity are the same model, even if their other fields differ or they are
		/// different object instances (as when one was decoded off the wire).
		/// </summary>
		/// <param name="identitySelector">The function that extracts a model's identity.</param>
		public Tokenizer(Func<TModel, object> identitySelector)
			: this(new IdentitySelectorEqualityComparer(identitySelector))
		{
		}
		#endregion

		#region properties
		/// <summary>
		/// The number of tokens defined.
		/// </summary>
		public int Count
		{
			get
			{
				return _tokenToModelMap.Count;
			}
		}

		/// <summary>
		/// The models that have been tokenized.
		/// </summary>
		public IEnumerable<TModel> Models
		{
			get
			{
				return _tokenToModelMap.Values;
			}
		}

		/// <summary>
		/// The defined tokens and the models they represent.
		/// </summary>
		public IEnumerable<KeyValuePair<Code, TModel>> Tokens
		{
			get
			{
				return _tokenToModelMap;
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Defines a token. Tokens are wire format, so a definition is forever: attempting to
		/// reuse an already-defined token, or to tokenize an already-tokenized model, throws.
		/// Use <see cref="RedefineToken(Code, TModel)"/> for the rare deliberate redefinition,
		/// such as updating the model behind a token after an entity changed.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">The model, or token definition.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the token or model is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the token is already defined or the model is already tokenized.
		/// </exception>
		public void DefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (_tokenToModelMap.ContainsKey(token))
				throw new ArgumentException($"Token '{token}' is already defined.", nameof(token));

			if (_modelToTokenMap.ContainsKey(model))
				throw new ArgumentException("Model is already tokenized.", nameof(model));

			_tokenToModelMap[token] = model;
			_modelToTokenMap[model] = token;
		}

		/// <summary>
		/// Redefines a token, keeping both lookup directions consistent: any model previously
		/// behind this token is forgotten, and any token previously assigned to this model is
		/// undefined.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">The model, or token definition.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the token or model is null.
		/// </exception>
		public void RedefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			TModel previousModel;
			if (_tokenToModelMap.TryGetValue(token, out previousModel))
				_modelToTokenMap.Remove(previousModel);

			Code previousToken;
			if (_modelToTokenMap.TryGetValue(model, out previousToken))
				_tokenToModelMap.Remove(previousToken);

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
		/// Checks if a model has been defined.
		/// </summary>
		/// <param name="model">The model to check.</param>
		/// <returns>True if the model has been tokenized, otherwise false.</returns>
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
		/// <returns>True if the model has been tokenized, otherwise false.</returns>
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

		#region nested classes
		private sealed class IdentitySelectorEqualityComparer : IEqualityComparer<TModel>
		{
			#region fields
			private readonly Func<TModel, object> _identitySelector;
			#endregion

			#region constructors
			public IdentitySelectorEqualityComparer(Func<TModel, object> identitySelector)
			{
				if (identitySelector == null)
					throw new ArgumentNullException(nameof(identitySelector));

				_identitySelector = identitySelector;
			}
			#endregion

			#region IEqualityComparer methods
			public bool Equals(TModel x, TModel y)
			{
				return object.Equals(_identitySelector(x), _identitySelector(y));
			}

			public int GetHashCode(TModel obj)
			{
				object identity = _identitySelector(obj);
				return identity == null ? 0 : identity.GetHashCode();
			}
			#endregion
		}
		#endregion
	}
}
