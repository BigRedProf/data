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
				FlexModel label = ReadLabel();
				if (label.TryGetTrait<int>(TapeTrait.TapePosition, out int position))
					return position;
				
				throw new InvalidOperationException("Tape position is not defined in the label.");
			}
			internal set
			{
				FlexModel label = ReadLabel();
				label.AddTrait(new Trait<int>(TapeTrait.TapePosition, value));
				int existingContentLength;
				bool hasContentLength = label.TryGetTrait<int>(CoreTrait.ContentLength, out existingContentLength);
				if (!hasContentLength || value > existingContentLength)
					label.AddTrait(new Trait<int>(CoreTrait.ContentLength, value));
				WriteLabel(label);
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

			TapeLabel tapeLabel = new TapeLabel()
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

		public void WriteLabel(FlexModel label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");

			TapeHelper.WriteLabel(this, label);
		}
		#endregion
	}
}
