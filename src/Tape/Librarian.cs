using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BigRedProf.Data.Tape
{
	public class Librarian
	{
		#region fields
			private readonly TapeProvider _tapeProvider;
		#endregion

		#region constructors
			public Librarian(TapeProvider tapeProvider)
			{
				_tapeProvider = tapeProvider ?? throw new ArgumentNullException(nameof(tapeProvider));
			}

			public Librarian(IPiedPiper piedPiper, TapeProvider tapeProvider)
					: this(tapeProvider)
			{
				if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));
			}
		#endregion

		#region internal properties
			internal TapeProvider TapeProvider
			{
				get { return _tapeProvider; }
			}
		#endregion

		#region methods
			public Tape FetchTape(Guid tapeId)
			{
				if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if (!_tapeProvider.TryFetchTapeInternal(tapeId, out Tape? tape))
				throw new KeyNotFoundException($"Tape with ID '{tapeId}' not found.");

				if (tape == null)
				throw new InvalidOperationException($"Tape provider returned null for tape '{tapeId}'.");

				return tape;
			}

			public IEnumerable<Tape> FetchAllTapes()
			{
				return _tapeProvider.FetchAllTapesInternal();
			}

			public IEnumerable<Tape> FetchTapesInSeries(Guid seriesId)
			{
				if (seriesId == Guid.Empty)
				throw new ArgumentException("Series ID cannot be empty.", nameof(seriesId));

				return _tapeProvider.FetchAllTapesInternal()
				.Where(tape =>
				{
					if (!tape.ReadLabel().TryGetTrait<Guid>(CoreTrait.SeriesId, out Guid tapeSeriesId))
					return false;

					if (tapeSeriesId != seriesId)
					return false;

					return true;
				}
				);
			}

			public void AddTape(Tape tape)
			{
				if (tape == null)
				throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

				// TODO: Should this be here or in Tape.CreateNew?? Need to figure this out.
				//_tapeProvider.AddTapeInternal(tape);
			}
		#endregion
	}
}
