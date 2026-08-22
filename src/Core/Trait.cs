using System;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A named aspect a subject may have. A trait identifier designates both the question being
	/// asked and the schema of the answer.
	/// </summary>
	/// <remarks>
	/// A trait identifier is a global agreement, not a local label. It is what lets two parties
	/// who have never met, and who share no common design, mean the same thing by the same
	/// question.
	///
	/// Three rules follow, and they are permanent.
	///
	/// Permanence. A trait identifier is bound to its schema forever. To change the schema of an
	/// answer is to ask a different question, and a different question needs a different
	/// identifier. Trait identifiers are minted, never reused or recycled.
	///
	/// Presence. A subject either has an answer to a question or it does not. There is no empty
	/// answer and no answer meaning "none" -- absence is how you say no.
	///
	/// Multiplicity. A subject has at most one answer per question. If a chair may have three
	/// photographs then the question is not "what is your photograph" but "what are your
	/// photographs", and its answer's schema is one of lists. Multiplicity lives inside the
	/// answer, never in repeated entries.
	/// </remarks>
	public class Trait
	{
		#region constructors
		public Trait()
		{
		}

		public Trait(AttributeFriendlyGuid traitId, AttributeFriendlyGuid schemaId)
		{
			TraitId = traitId;
			SchemaId = schemaId;
		}
		#endregion

		#region properties
		/// <summary>
		/// The trait identifier: the question being asked.
		/// </summary>
		public Guid TraitId
		{
			get;
			set;
		}

		/// <summary>
		/// The schema of the answer.
		/// </summary>
		public Guid SchemaId
		{
			get;
			set;
		}
		#endregion
	}
}
