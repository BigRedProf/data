using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A <see cref="Tokenizer{TModel}"/> encodes potentially large models that are repeated
	/// often as small <see cref="Code"/> values called tokens. This helps to achieve efficiency by
	/// packing the models into small tokens while still working with them as natural models.
	/// </summary>
	/// <remarks>
	/// Thread safety: all members are safe for concurrent use, so a tokenizer can be hydrated
	/// from a dynamic source (like a story) on one thread while other threads read. Once
	/// hydration is complete, call <see cref="Freeze"/> to make reads lock-free and further
	/// definitions throw.
	/// </remarks>
	/// <typeparam name="TModel">The model, or token definition, to be tokenized.</typeparam>
	public class Tokenizer<TModel>
	{
		#region fields
		private readonly object _lock = new object();
		private readonly IDictionary<Code, TModel> _tokenToModelMap;
		private readonly IDictionary<TModel, Code> _modelToTokenMap;
		private volatile bool _isFrozen;
		private int _nextOrdinal;
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
		/// True once <see cref="Freeze"/> has been called: reads are lock-free and further
		/// definitions throw.
		/// </summary>
		public bool IsFrozen
		{
			get
			{
				return _isFrozen;
			}
		}

		/// <summary>
		/// The number of tokens defined.
		/// </summary>
		public int Count
		{
			get
			{
				if (_isFrozen)
					return _tokenToModelMap.Count;

				lock (_lock)
				{
					return _tokenToModelMap.Count;
				}
			}
		}

		/// <summary>
		/// The models that have been tokenized. Before <see cref="Freeze"/> this returns a
		/// snapshot; after, it exposes the (now immutable) definitions directly.
		/// </summary>
		public IEnumerable<TModel> Models
		{
			get
			{
				if (_isFrozen)
					return _tokenToModelMap.Values;

				lock (_lock)
				{
					return new List<TModel>(_tokenToModelMap.Values);
				}
			}
		}

		/// <summary>
		/// The defined tokens and the models they represent. Before <see cref="Freeze"/> this
		/// returns a snapshot; after, it exposes the (now immutable) definitions directly.
		/// </summary>
		public IEnumerable<KeyValuePair<Code, TModel>> Tokens
		{
			get
			{
				if (_isFrozen)
					return _tokenToModelMap;

				lock (_lock)
				{
					return new List<KeyValuePair<Code, TModel>>(_tokenToModelMap);
				}
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Defines a token. Tokens are wire format, so a definition is forever: attempting to
		/// reuse an already-defined token, or to tokenize an already-tokenized model, throws.
		/// Use <see cref="RedefineToken(Code, TModel)"/> for the rare deliberate redefinition,
		/// such as updating the model behind a token after an entity changed. New code should
		/// prefer <see cref="AllocateNextToken(TModel)"/> and let the tokenizer pick the token;
		/// this method exists for pinning legacy hand-authored tokens.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="model">The model, or token definition.</param>
		/// <exception cref="ArgumentNullException">
		/// Thrown if the token or model is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the token is already defined or the model is already tokenized.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the tokenizer is frozen.
		/// </exception>
		public void DefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			lock (_lock)
			{
				ThrowIfFrozen();
				DefineTokenCore(token, model);
			}
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
		/// <exception cref="InvalidOperationException">
		/// Thrown if the tokenizer is frozen.
		/// </exception>
		public void RedefineToken(Code token, TModel model)
		{
			if (token == null)
				throw new ArgumentNullException(nameof(token));

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			lock (_lock)
			{
				ThrowIfFrozen();

				TModel previousModel;
				if (_tokenToModelMap.TryGetValue(token, out previousModel))
					_modelToTokenMap.Remove(previousModel);

				Code previousToken;
				if (_modelToTokenMap.TryGetValue(model, out previousToken))
					_tokenToModelMap.Remove(previousToken);

				_tokenToModelMap[token] = model;
				_modelToTokenMap[model] = token;
			}
		}

		/// <summary>
		/// Assigns the next token in the canonical sequence to the specified model and returns
		/// it. The canonical encoding is wire format and fixed forever: the token for ordinal
		/// <c>n</c> (zero-based) is the binary representation of <c>n + 1</c>, most significant
		/// bit first, with no leading zeros — "1", "10", "11", "100", and so on. In a tokenizer
		/// with no hand-pinned tokens, allocation is therefore fully deterministic: replaying
		/// the same definitions in the same order always assigns the same tokens. Ordinals whose
		/// canonical token collides with a hand-pinned token are skipped.
		/// </summary>
		/// <param name="model">The model, or token definition.</param>
		/// <returns>The token assigned to the model.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the model is null.</exception>
		/// <exception cref="ArgumentException">
		/// Thrown if the model is already tokenized.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if the tokenizer is frozen.
		/// </exception>
		public Code AllocateNextToken(TModel model)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			lock (_lock)
			{
				ThrowIfFrozen();

				Code token = EncodeOrdinal(_nextOrdinal++);
				while (_tokenToModelMap.ContainsKey(token))
					token = EncodeOrdinal(_nextOrdinal++);

				DefineTokenCore(token, model);

				return token;
			}
		}

		/// <summary>
		/// Freezes the tokenizer: subsequent reads are lock-free and subsequent definitions
		/// throw. Call this once hydration is complete. Freezing is idempotent.
		/// </summary>
		public void Freeze()
		{
			lock (_lock)
			{
				_isFrozen = true;
			}
		}

		/// <summary>
		/// Checks if a token has been defined.
		/// </summary>
		/// <param name="token">The token to check.</param>
		/// <returns>True if the token has been defined, otherwise false.</returns>
		public bool IsTokenDefined(Code token)
		{
			if (_isFrozen)
				return _tokenToModelMap.ContainsKey(token);

			lock (_lock)
			{
				return _tokenToModelMap.ContainsKey(token);
			}
		}

		/// <summary>
		/// Checks if a model has been defined.
		/// </summary>
		/// <param name="model">The model to check.</param>
		/// <returns>True if the model has been tokenized, otherwise false.</returns>
		public bool IsModelTokenized(TModel model)
		{
			if (_isFrozen)
				return _modelToTokenMap.ContainsKey(model);

			lock (_lock)
			{
				return _modelToTokenMap.ContainsKey(model);
			}
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
			if (!TryGetModel(token, out model))
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

			if (_isFrozen)
				return _tokenToModelMap.TryGetValue(token, out model);

			lock (_lock)
			{
				return _tokenToModelMap.TryGetValue(token, out model);
			}
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
			if (!TryGetToken(model, out token))
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

			if (_isFrozen)
				return _modelToTokenMap.TryGetValue(model, out token);

			lock (_lock)
			{
				return _modelToTokenMap.TryGetValue(model, out token);
			}
		}
		#endregion

		#region private methods
		private void DefineTokenCore(Code token, TModel model)
		{
			if (_tokenToModelMap.ContainsKey(token))
				throw new ArgumentException($"Token '{token}' is already defined.", nameof(token));

			if (_modelToTokenMap.ContainsKey(model))
				throw new ArgumentException("Model is already tokenized.", nameof(model));

			_tokenToModelMap[token] = model;
			_modelToTokenMap[model] = token;
		}

		private void ThrowIfFrozen()
		{
			if (_isFrozen)
			{
				throw new InvalidOperationException(
					"The tokenizer is frozen. Tokens cannot be defined after Freeze() is called."
				);
			}
		}
		#endregion

		#region private functions
		private static Code EncodeOrdinal(int ordinal)
		{
			// The canonical ordinal-to-token encoding. This is wire format: it must never
			// change, just like schema identifiers.
			return new Code(Convert.ToString((long)ordinal + 1, 2));
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
