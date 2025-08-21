using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Diagnostics;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeMover
	{
		#region fields
		private Tape _tape;
		#endregion

		#region constructors
		protected TapeMover(Tape tape)
		{
			_tape = tape ?? throw new ArgumentNullException(nameof(tape));
		}
		#endregion

		#region properties
		public Tape Tape
		{
			get
			{
				return _tape;
			}
		}
		#endregion

		#region methods
		public void RewindOrFastForwardTo(int position)
		{
			if (position < 0 || position > Tape.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(position), 
					$"Position must be in the range [0, {Tape.MaxContentLength}."
				);
			}

			SetPosition(position);
		}
		#endregion

		#region private methods
		private void SetPosition(int position)
		{
			Debug.Assert(position >= 0);
			Debug.Assert(position <= Tape.MaxContentLength);

			// TODO: Consider renaming AddTrait to SetTrait and changing arguments to take
			// trait identifier and value directly.
			//Tape.Label.SetTrait<int>(TapeTrait.TapePosition, position);
			Trait<int> trait = new Trait<int>(TapeTrait.TapePosition, position);
			Tape.ReadLabel().AddTrait(trait);
		}
		#endregion
	}
}
