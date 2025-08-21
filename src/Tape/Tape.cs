using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;

namespace BigRedProf.Data.Tape
{
	public abstract class Tape
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
		protected Tape(TapeProvider provider)
		{
			_tapeProvider = provider ?? throw new ArgumentNullException(nameof(provider));
		}
		#endregion

		#region properties
		public Guid Id
		{
			get
			{
				FlexModel label = ReadLabel();
				if (label.TryGetTrait<Guid>(CoreTrait.Id, out Guid id))
					return id;
				
				throw new InvalidOperationException("Tape ID is not defined in the label.");
			}
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
			return TapeHelper.ReadLabel(TapeProvider, Id);
		}

		public void WriteLabel(FlexModel label)
		{
			if (label == null)
				throw new ArgumentNullException(nameof(label), "Label cannot be null.");
		
			TapeHelper.WriteLabel(TapeProvider, Id, label);
		}
		#endregion
	}
}
