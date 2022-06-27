using System;
using System.Diagnostics;
using CommandLine;

namespace BigRedProf.PackRatCompiler
{
	public class Program
	{
		static private void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(RunOptions)
				.WithNotParsed(HandleParseError);
		}

		#region event handlers
		static private void RunOptions(CommandLineOptions options)
		{
			Console.WriteLine("Hello, World!");

			if (options.InputPath != null)
			{
				Console.Write("Input directory is ");
				Console.Write(options.InputPath, ConsoleColor.DarkGreen);
				Console.WriteLine();
			}

			if (options.OutputPath != null)
			{
				Console.Write("Output directory is ");
				Console.WriteLine(options.OutputPath, ConsoleColor.Red);
			}
		}

		static private void HandleParseError(IEnumerable<Error> errors)
		{
			Console.WriteLine("Invalid usage.");
			foreach(Error error in errors)
			{
				Console.WriteLine(error.ToString(), ConsoleColor.Red);
			}
		}
		#endregion
	}
}
