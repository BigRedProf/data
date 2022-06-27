using System;
using CommandLine;

namespace BigRedProf.PackRatCompiler
{
	public class CommandLineOptions
	{
		[Value(0, MetaName = "Input Path", HelpText = "The root directory of the project.")]
		public string? InputPath { get; set; }

		[Option('o', "outputPath", Required = true, HelpText = "The output directory for the PackRat files.")]
		public string? OutputPath { get; set; }
	}
}
