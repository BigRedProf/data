using System;
using CommandLine;

namespace BigRedProf.Data.PackRatCompiler
{
	public class CommandLineOptions
	{
		[Value(0, MetaName = "Project File", HelpText = "The C# project file to parse and create pack rats for.")]
		public string? ProjectFile { get; set; }

		[Option('o', "outputPath", Required = true, HelpText = "The output directory for the PackRat files.")]
		public string? OutputDirectory { get; set; }
	}
}
