using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Tape
{
	abstract public class Librarian
	{
		#region fields
		private IPiedPiper _piedPiper;
		#endregion

		#region protected constructors
		protected Librarian(IPiedPiper piedPiper)
		{
			_piedPiper = piedPiper ?? throw new ArgumentNullException(nameof(piedPiper));
			DefineCoreTraits();
		}
		#endregion

		#region methods
		public Tape FetchTape(Guid tapeId)
		{
			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));
			
			if (!TryFetchTape(tapeId, out Tape tape))
				throw new KeyNotFoundException($"Tape with ID '{tapeId}' not found.");
			
			return tape;
		}
		#endregion

		#region abstract methods
		abstract public IEnumerable<Tape> FetchAllTapes();
		abstract public IEnumerable<Tape> FetchTapesInSeries(Guid seriesId);
		abstract public bool TryFetchTape(Guid tapeId, out Tape tape);
		#endregion

		#region private methods
		private void DefineCoreTraits()
		{
			_piedPiper.DefineTrait(new TraitDefinition(TapeTrait.TapePosition, CoreSchema.Int32));
		}
		#endregion
	}
}
