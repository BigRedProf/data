using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	[GeneratePackRat("123")]
	internal class Compiler
	{
		#region methods
		public int Compile(CommandLineOptions options)
		{
			Debug.Assert(options != null);
			Debug.Assert(options.ProjectFile != null);
			Debug.Assert(options.OutputDirectory != null);

			FileInfo projectFile = new FileInfo(options.ProjectFile);
			if (!projectFile.Exists)
				throw new Exception("project file not found");  // TODO: report as error

			StreamWriter stdoutStreamWriter = new StreamWriter(Console.OpenStandardOutput());
			stdoutStreamWriter.AutoFlush = true;

			int exitCode = 0;
			using (	CompilationContext compilationContext = new CompilationContext(stdoutStreamWriter,	stdoutStreamWriter))
			{
				PackRatGenerator packRatGenerator = new PackRatGenerator(compilationContext);

				DirectoryInfo outputDirectory = new DirectoryInfo(options.OutputDirectory);
				if (!outputDirectory.Exists)
					outputDirectory.Create();

				ProcessProject(packRatGenerator, compilationContext, projectFile, outputDirectory);

				exitCode = compilationContext.ExitCode;
			}

			return exitCode;
		}
		#endregion

		#region private methods
		private void ProcessProject(PackRatGenerator packRatGenerator, ICompilationContext context, FileInfo projectFile, DirectoryInfo outputDirectory)
		{
			SourceProject sourceProject = new SourceProject(context, projectFile);
			foreach (INamedTypeSymbol modelClass in sourceProject.GetModelClasses3())
			{
				FileInfo outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, modelClass.Name + "PackRat.g.cs"));
				ProcessModelClass(packRatGenerator, modelClass, outputFile);
			}
		}

		private void ProcessModelClass(PackRatGenerator packRatGenerator, INamedTypeSymbol modelClass, FileInfo outputFile)
		{
			using (FileStream outputStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write))
			{
				packRatGenerator.GeneratePackRat(modelClass, outputStream);
			}
		}
		#endregion
	}
}
