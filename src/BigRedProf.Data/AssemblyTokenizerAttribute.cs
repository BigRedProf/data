using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// Identifies a <see cref="Tokenizer{TModel}"/> as needing to be registered when 
	/// <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/> is called.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class AssemblyTokenizerAttribute : Attribute
	{
		#region constructors
		public AssemblyTokenizerAttribute(string tokenizerId)
		{
			TokenizerId = tokenizerId;
		}
		#endregion

		#region properties
		/// <summary>
		/// The tokenizer identifier.
		/// </summary>
		public string TokenizerId
		{
			get;
		}
		#endregion
	}
}
