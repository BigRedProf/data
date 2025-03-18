using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Core.PackRats
{
	/// <summary>
	/// The <see cref="TokenizedModelPackRat{TValue}"/> encodes potentially large models that are repeated
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
	/// <typeparam name="TModel">The model, or token definition, to be tokenized.</typeparam>
	public class TokenizedModelPackRat<TModel> : PackRat<TModel>
	{
		#region fields
		private readonly Tokenizer<TModel> _tokenizer;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a <see cref="TokenizedModelPackRat{TModel}"/>.
		/// </summary>
		/// <param name="piedPiper">The pied piper.</param>
		public TokenizedModelPackRat(IPiedPiper piedPiper, Tokenizer<TModel> tokenizer)
			: base(piedPiper)
		{
			if(tokenizer == null)
				throw new ArgumentNullException(nameof(tokenizer));

			_tokenizer = tokenizer;
		}
		#endregion

		#region PackRat methods
		/// <inheritdoc/>
		public override void PackModel(CodeWriter writer, TModel model)
		{
			if(writer == null)
				throw new ArgumentNullException(nameof(writer));

			Code token = _tokenizer.GetToken(model);
			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(CoreSchema.Code);
			codePackRat.PackModel(writer, token);
		}

		/// <inheritdoc/>
		public override TModel UnpackModel(CodeReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException(nameof(reader));

			PackRat<Code> codePackRat = PiedPiper.GetPackRat<Code>(CoreSchema.Code);
			Code token = codePackRat.UnpackModel(reader);

			TModel model;
			if (!_tokenizer.TryGetModel(token, out model))
				throw new InvalidOperationException($"Token '{token}' not defined.");

			return model;
		}
		#endregion
	}
}
