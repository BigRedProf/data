using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;
using System.Diagnostics;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeMover
	{
		#region fields
		private Tape? _tape;
		#endregion

		#region properties
		public Tape Tape
		{
			get
			{
				VerifyTapeIsInserted();
				return _tape!;
			}
		}

		public bool IsTapeInserted => _tape != null;
		#endregion

		#region methods
		public void RewindOrFastForwardTo(int position)
		{
			VerifyTapeIsInserted();

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

		#region protected methods
		protected void VerifyTapeIsInserted()
		{
			if (!IsTapeInserted)
				throw new InvalidOperationException("Tape is not inserted.");
		}

		protected void InsertTape(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape));

			if (IsTapeInserted)
				throw new InvalidOperationException("A tape is already inserted.");

			_tape = tape;
		}

		protected void EjectTape()
		{
			if (!IsTapeInserted)
				throw new InvalidOperationException("No tape is currently inserted.");

			_tape = null;
		}
		#endregion

		#region private methods
		private void SetPosition(int position)
		{
			Debug.Assert(position >= 0);
			Debug.Assert(position <= Tape.MaxContentLength);

			VerifyTapeIsInserted();

			// TODO: Consider renaming AddTrait to SetTrait and changing arguments to take
			// trait identifier and value directly.
			//Tape.Label.SetTrait<int>(TapeTrait.TapePosition, position);
			Trait<int> trait = new Trait<int>(TapeTrait.TapePosition, position);
			Tape.ReadLabel().AddTrait(trait);
		}
		#endregion
	}
}
