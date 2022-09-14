using System;
using System.Diagnostics;
using BigRedProf.Data.PackRatCompiler.Internal;
using CommandLine;

namespace BigRedProf.Data.PackRatCompiler
{
	public class Program
	{
		#region static private methods
		static private int Main(string[] args)
		{
			int exitCode = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args)
				.MapResult<CommandLineOptions, int>(
					options => RunOptions(options),
					errors => HandleParseError(errors)
				);
			return exitCode;
		}

		static private int RunOptions(CommandLineOptions options)
		{
			Compiler compiler = new Compiler();
			return compiler.Compile(options);
		}

		static private int HandleParseError(IEnumerable<Error> errors)
		{
			Console.WriteLine("Invalid usage.");
			foreach(Error error in errors)
			{
				Console.Error.WriteLine(error.ToString());
			}
			return 1;
		}
		#endregion
	}
}
