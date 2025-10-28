using System;

namespace BigRedProf.Data.Tape.Cli
{
	public static class Program
	{
		#region methods
		public static int Main(string[] args)
		{
			return CommandDispatcher.Dispatch(args);
		}
		#endregion
	}
}
