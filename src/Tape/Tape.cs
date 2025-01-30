using System;

namespace BigRedProf.Data.Tape
{
	public abstract class Tape
	{
		private readonly TapeProvider _provider;

		protected Tape(TapeProvider provider)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public TapePlayer CreatePlayer() => new TapePlayer(_provider);
		public TapeRecorder CreateRecorder() => new TapeRecorder(_provider);
	}
}
