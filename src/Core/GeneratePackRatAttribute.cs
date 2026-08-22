using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Instructs the pack rat compiler to generate a <see cref="PackRat"/> for this
	/// model.
	/// </summary>
	/// <remarks>
	/// This declares a structured schema: a code read as an ordered sequence of parts, one per
	/// <see cref="PackFieldAttribute"/>, each part read under the schema that attribute names.
	/// Think of a printed form with numbered blanks. A filled-in form carries the answers and
	/// nothing else; you know what blank 2 means because you hold the blank form, not because the
	/// filled one told you.
	///
	/// Three consequences, and they are permanent.
	///
	/// A schema's sequence of parts is fixed forever. Insert a part, reorder two, or change the
	/// schema a part is read under, and every code already written now reads WRONG -- not
	/// unreadable, which would be survivable, but wrong. A changed form is a different form and
	/// needs a different schema identifier.
	///
	/// A part may be retired but never renumbered. Drop a field and leave its position empty; the
	/// positions after it keep the numbers they have always had. The compiler permits gaps for
	/// exactly this reason.
	///
	/// Appending a part is not free either. A reader built against the old schema stops after the
	/// parts it knows, and unless something independently marks where this code ends it cannot
	/// tell more was written -- in a stream it will read the next record's first part as this
	/// one's. Appending is safe only where the code's extent is framed from outside, as it is
	/// inside a <see cref="Datum"/> or a <see cref="FlexDatum"/>. Otherwise, mint a new schema.
	///
	/// Where producers and consumers change independently, prefer a <see cref="FlexDatum"/>: its
	/// parts are identified globally rather than by position, so a reader can use what it
	/// recognises and ignore the rest. A structured schema is the compact choice, and it asks
	/// that both parties hold the identical form.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class)]
	public class GeneratePackRatAttribute : Attribute
	{
		#region constructors
		public GeneratePackRatAttribute(string schemaId)
		{
			SchemaId = schemaId;
		}
		#endregion

		#region properties
		/// <summary>
		/// The schema identifier.
		/// </summary>
		public string SchemaId
		{
			get; 
		}
		#endregion
	}
}
