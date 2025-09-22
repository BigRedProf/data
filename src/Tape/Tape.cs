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

		#region constructors
		public Tape(TapeProvider provider, Guid tapeId)
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
			get
			{
				FlexModel label = ReadLabel();
				if (label.TryGetTrait<int>(TapeTrait.TapePosition, out int position))
					return position;
				
				throw new InvalidOperationException("Tape position is not defined in the label.");
			}
		}
		#endregion

		#region internal properties
		public TapeProvider TapeProvider
		{
			get { return _tapeProvider; }
		}
		#endregion

		#region methods
		public FlexModel ReadLabel()
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
