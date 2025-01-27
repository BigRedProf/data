using System;
using System.Diagnostics;
using BigRedProf.Data.PackRatCompiler.Internal;
using CommandLine;
using Microsoft.Build.Locator;

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

			// HACKHACK: per https://docs.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2017/msbuild/updating-an-existing-application?view=vs-2017#use-microsoftbuildlocator,
			// we have to call RegisterDefaults in a method other than the one that use project.
			// That doesn't seem to fix our 
			// CS5001: Program does not contain a static 'Main' method suitable for an entry point
			// bug though. :(
			MSBuildLocator.RegisterDefaults();

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
