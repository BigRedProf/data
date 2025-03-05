using BigRedProf.Data.Core;
using BigRedProf.Data.PackRatCompiler.Internal.Symbols;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

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

			AssemblyRegistrationHelperGenerator assemblyRegistrationHelperGenerator = new AssemblyRegistrationHelperGenerator(outputDirectory);
			assemblyRegistrationHelperGenerator.GenerateStart();

			assemblyRegistrationHelperGenerator.StartGeneratedPackRatsSection();
			HashSet<string> generatedPackRatClassesSet = new HashSet<string>();
			foreach (INamedTypeSymbol modelClass in sourceProject.GetGeneratePackRatClasses())
			{
				FileInfo outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, modelClass.Name + "PackRat.g.cs"));
				ProcessModelClass(packRatGenerator, modelClass, outputFile);

				string type = SymbolHelper.GetFullName(modelClass);
				string @class = type + "PackRat";
				AttributeData generatePackRatAttribute = SymbolHelper.GetAttribute(modelClass, "BigRedProf.Data.Core.GeneratePackRat");
				string schemaId = (string) SymbolHelper.GetAttributeArgument(generatePackRatAttribute, 0)!;
				assemblyRegistrationHelperGenerator.AddPackRat(type, @class, schemaId);

				generatedPackRatClassesSet.Add(@class);
			}

			assemblyRegistrationHelperGenerator.StartCodedPackRatsSection();
			foreach (INamedTypeSymbol packRatClass in sourceProject.GetAssemblyPackRatClasses())
			{
				string baseClassGenericType = SymbolHelper.GetFullName((INamedTypeSymbol)packRatClass.BaseType!.TypeArguments[0])!;
				string type = $"BigRedProf.Data.Core.TokenizedModel<{baseClassGenericType}>";
				string @class = SymbolHelper.GetFullName(packRatClass);
				if (!generatedPackRatClassesSet.Contains(@class))
				{
					AttributeData assemblyPackRatAttribute = SymbolHelper.GetAttribute(packRatClass, "BigRedProf.Data.Core.AssemblyPackRat");
					string schemaId = (string)SymbolHelper.GetAttributeArgument(assemblyPackRatAttribute, 0)!;
					assemblyRegistrationHelperGenerator.AddPackRat(type, @class, schemaId);
				}
			}

			assemblyRegistrationHelperGenerator.GenerateEnd();
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
