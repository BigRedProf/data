﻿namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class AssemblyRegistrationHelperGenerator
	{
		#region fields
		private CSharpWriter _writer;
		#endregion

		#region constructors
		public AssemblyRegistrationHelperGenerator(DirectoryInfo outputDirectory)
		{
			FileInfo outputFile = new FileInfo(Path.Combine(outputDirectory.FullName, "GeneratedAssemblyRegistrationHelper.g.cs"));
			FileStream fileStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write);
			_writer = new CSharpWriter(fileStream);
		}
		#endregion

		#region methods
		public void GenerateStart()
		{
			_writer.WriteLine("// <auto-generated/>");
			_writer.WriteLine();

			_writer.WriteLine("using BigRedProf.Data.Core;");
			_writer.WriteLine();

			_writer.WriteLine("namespace BigRedProf.Data.PackRatCompiler.Generated");
			_writer.WriteOpeningCurlyBrace();

			_writer.WriteLine("[AssemblyRegistrationHelper]");
			_writer.WriteLine("public class GeneratedAssemblyRegistrationHelper : AssemblyRegistrationHelper");
			_writer.WriteOpeningCurlyBrace();

			_writer.WriteLine("#region AssemblyRegistrationHelper methods");
			
			_writer.WriteLine("override public void RegisterPackRats(IPiedPiper piedPiper)");
			_writer.WriteOpeningCurlyBrace();
		}

		public void GenerateEnd()
		{
			_writer.WriteClosingCurlyBrace();	// RegisterPackRats method

			_writer.WriteLine("#endregion");	// AssemblyRegistation methods

			_writer.WriteClosingCurlyBrace();   // class
			_writer.WriteClosingCurlyBrace();	// namespace

			_writer.Dispose();

			// TODO: Probably should make this class disposable too
		}

		public void AddPackRat(string type, string @class, string schemaId)
		{
			_writer.WriteLine($"piedPiper.RegisterPackRat<{type}>(new {@class}(piedPiper), new AttributeFriendlyGuid(\"{schemaId}\"));");
		}

		public void StartGeneratedPackRatsSection()
		{
			_writer.WriteLine("// generated pack rats");
		}

		public void StartCodedPackRatsSection()
		{
			_writer.WriteLine();
			_writer.WriteLine("// coded pack rats");
		}
		#endregion
	}
}
