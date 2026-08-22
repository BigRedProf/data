using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;

namespace BigRedProf.Data.Tape
{
	public class Tape
	{
		#region constants
		/// <summary>
		/// The maximum length, in bits, for a tape's content.
		/// </summary>
		public const int MaxContentLength = 1_000_000_000; // 1 billion bits
		#endregion

		#region fields
		private readonly TapeProvider _tapeProvider;
		#endregion

		#region internal constructors
		internal Tape(TapeProvider provider, Guid tapeId)
		{
			_tapeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
			Id = tapeId != Guid.Empty ? tapeId : throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));
		}
		#endregion

		#region properties
		public Guid Id
		{
			get;
			private set;
		}

		public int Position
		{
			// TODO: Not sure this belongs on a label. Perhaps should delegate to TapeProvider?
			get
			{
				TapeLabel label = ReadLabel();
				if (label.FlexDatum.TryGetTrait<int>(TapeTrait.TapePosition, TapeProvider.PiedPiper, out int position))
					return position;
				
				throw new InvalidOperationException("Tape position is not defined in the label.");
			}
			internal set
			{
				TapeLabel label = ReadLabel();
				FlexDatumBuilder builder = label.FlexDatum.ToBuilder(TapeProvider.PiedPiper);
				builder.AddTrait(TapeTrait.TapePosition, value);
				int existingContentLength;
				bool hasContentLength = label.FlexDatum.TryGetTrait<int>(
					CoreTrait.ContentLength, TapeProvider.PiedPiper, out existingContentLength);
				if (!hasContentLength || value > existingContentLength)
					builder.AddTrait(CoreTrait.ContentLength, value);
				WriteLabel(TapeLabel.Over(TapeProvider.PiedPiper, builder.Build()));
			}
		}
		#endregion

		#region internal properties
		public TapeProvider TapeProvider
		{
			get { return _tapeProvider; }
		}
		#endregion

		#region functions
		public static Tape CreateNew(TapeProvider provider, Guid tapeId)
		{
			Tape tape = new Tape(provider, tapeId);

			provider.AddTapeInternal(tape);

			TapeLabel tapeLabel = TapeLabel.Empty(provider.PiedPiper)
				.WithTapeId(tapeId);
			tape.WriteLabel(tapeLabel);
			
			tape.Position = 0;

			return tape;
		}
		#endregion

		#region methods
		public TapeLabel ReadLabel()
		{
			return TapeHelper.ReadLabel(this);
		}

		public void WriteLabel(TapeLabel label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");

			TapeHelper.WriteLabel(this, label);
		}
		#endregion
	}
}
