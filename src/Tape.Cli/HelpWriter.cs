using System;
using System.Diagnostics;

namespace BigRedProf.Data.Tape.Cli
{
	internal static class HelpWriter
	{
		#region methods
		internal static void WriteGeneralUsage()
		{
			Console.Error.WriteLine("Usage: tapelib <command> [options]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Commands:");
			Console.Error.WriteLine("  tape      Manage tape libraries.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Run 'tapelib <command> --help' for more information on a command.");
		}

		internal static void WriteTapeUsage()
		{
			Console.Error.WriteLine("Usage: tapelib tape <subcommand>");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Subcommands:");
			Console.Error.WriteLine("  ls, list   List tapes from the disk library rooted at the current directory.");
		}

		internal static void WriteUnknownCommand(string command)
		{
			Debug.Assert(command != null);
			Console.Error.WriteLine("Unknown command '{0}'.", command);
			Console.Error.WriteLine();
			WriteGeneralUsage();
		}

		internal static void WriteUnknownTapeCommand(string subcommand)
		{
			Debug.Assert(subcommand != null);
			Console.Error.WriteLine("Unknown tape command '{0}'.", subcommand);
			Console.Error.WriteLine();
			WriteTapeUsage();
		}
		#endregion
	}
}
