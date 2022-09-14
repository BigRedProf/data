using System;
using CommandLine;

namespace BigRedProf.Data.PackRatCompiler
{
	public class CommandLineOptions
	{
		// TODO: allow msbuild-style wildcards like $(ProjectDir)/**
		[Value(0, MetaName = "Input Path", HelpText = "The root directory of the project.")]
		public string? InputPath { get; set; }

		[Option('o', "outputPath", Required = true, HelpText = "The output directory for the PackRat files.")]
		public string? OutputPath { get; set; }

		[Option('p', "projectFile", Required = true, HelpText = "The project file containing the PackRat files.")]
		public string? ProjectFile { get; set; }
	}
}
