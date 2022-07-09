using System;
using System.Diagnostics;
using BigRedProf.Data.PackRatCompiler.Internal;
using CommandLine;

namespace BigRedProf.Data.PackRatCompiler
{
	public class Program
	{
		#region static private methods
		static private void Main(string[] args)
		{
			CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
				.WithParsed(RunOptions)
				.WithNotParsed(HandleParseError);
		}

		static private void RunOptions(CommandLineOptions options)
		{
			Compiler compiler = new Compiler();
			compiler.Compile(options);
		}

		static private void HandleParseError(IEnumerable<Error> errors)
		{
			Console.WriteLine("Invalid usage.");
			foreach(Error error in errors)
			{
				Console.Error.WriteLine(error.ToString());
			}
		}
		#endregion
	}
}
