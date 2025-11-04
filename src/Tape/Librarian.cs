using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;

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

			public IList<Tape> FetchTapesInSeries(Guid seriesId)
			{
				if (seriesId == Guid.Empty)
					throw new ArgumentException("Series ID cannot be empty.", nameof(seriesId));

				IEnumerable<Tape> allTapes = _tapeProvider.FetchAllTapesInternal();
				List<Tape> tapesInSeries = new List<Tape>();

				foreach (Tape tape in allTapes)
				{
					if (tape == null)
						continue;

					TapeLabel label = tape.ReadLabel();
					Guid tapeSeriesId;
					if (!label.TryGetTrait<Guid>(CoreTrait.SeriesId, out tapeSeriesId))
						continue;

					if (tapeSeriesId != seriesId)
						continue;

					tapesInSeries.Add(tape);
				}

				tapesInSeries.Sort((left, right) =>
				{
					TapeLabel leftLabel = left.ReadLabel();
					TapeLabel rightLabel = right.ReadLabel();
					return leftLabel.SeriesNumber.CompareTo(rightLabel.SeriesNumber);
				});

				return tapesInSeries;
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
