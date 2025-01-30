using System;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeMover
	{
		#region fields
		private int _position;
		#endregion

		#region protected fields
		protected readonly TapeProvider _provider;
		#endregion

		#region constructors
		protected TapeMover(TapeProvider provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}
		#endregion

		#region properties
		public int Position => _position;
		#endregion

		#region methods
		public void RewindOrFastForwardTo(int position)
		{
			if (position < 0 || position > TapeProvider.MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(
					nameof(position), 
					$"Position must be in the range [0, {TapeProvider.MaxContentLength}."
				);
			}

			_position = position;
		}
		#endregion	
	}
}
