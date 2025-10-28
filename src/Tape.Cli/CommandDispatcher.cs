using System;

namespace BigRedProf.Data.Tape.Cli
{
	public static class CommandDispatcher
	{
		#region functions
		public static int Dispatch(string[] args)
		{
			if (args == null)
				throw new ArgumentNullException(nameof(args));

			if (args.Length == 0)
			{
				HelpWriter.WriteGeneralUsage();
				return 1;
			}

			string command = args[0];

			if (IsHelpOption(command))
			{
				HelpWriter.WriteGeneralUsage();
				return 0;
			}

			if (string.Equals(command, "tape", StringComparison.OrdinalIgnoreCase))
			{
				string[] tapeArgs = SliceArguments(args);
				return TapeCommandHandler.Execute(tapeArgs);
			}

			HelpWriter.WriteUnknownCommand(command);
			return 1;
		}

		private static string[] SliceArguments(string[] args)
		{
			if (args.Length <= 1)
			return Array.Empty<string>();

			string[] slice = new string[args.Length - 1];
			Array.Copy(args, 1, slice, 0, slice.Length);
			return slice;
		}

		private static bool IsHelpOption(string value)
		{
			if (value == null)
				return false;

			if (string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase))
				return true;

			if (string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase))
				return true;

			return false;
		}
		#endregion
	}
}
