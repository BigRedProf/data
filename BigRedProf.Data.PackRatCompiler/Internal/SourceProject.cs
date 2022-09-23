﻿using BigRedProf.Data;
using BigRedProf.Data.PackRatCompiler.Internal.Symbols;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal sealed class SourceProject
	{
		#region fields
		private readonly ICompilationContext _compilationContext;
		#endregion

		#region constructors
		public SourceProject(ICompilationContext compilationContext, FileInfo projectFile)
		{
			Debug.Assert(compilationContext != null);
			Debug.Assert(projectFile != null);

			_compilationContext = compilationContext;

			// TODO: decide whether to use filePath here and get rid of redundant project in compilation context
			// (probably best) or vice versa

			// HACKHACK: add these AddReferences and WithOptions calls to avoid 
			// error CS5001: Program does not contain a static 'Main' method suitable for an entry point
			// https://stackoverflow.com/a/45823751/5682

			//Debug.WriteLine("** Compilation 1 (from the project file)...");
			//CSharpCompilation compilation1 = (CSharpCompilation)_compilationContext.Project.GetCompilationAsync().Result;
			//ReportCompilationDiagnostics(compilation1);
			_compilationContext.AddProject(projectFile);

			//Debug.WriteLine("** Compilation 2 (Add References)...");
			//CSharpCompilation compilation2 = compilation1.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
			//ReportCompilationDiagnostics(compilation2);

			//Debug.WriteLine("** Compilation 3 (DLL)...");
			//CSharpCompilation compilation3 = compilation2.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			//ReportCompilationDiagnostics(compilation3);

			//Debug.Assert(compilation1.SyntaxTrees.Length == 1);
			//_semanticModel = compilation1.GetSemanticModel(compilation1.SyntaxTrees[0]);
			//_compilation = compilation3;
		}
		#endregion

		#region methods
		public IEnumerable<INamedTypeSymbol> GetModelClasses3()
		{
			return SymbolHelper.GetTypes(_compilationContext.Compilation.GlobalNamespace)
				//.Where(t => t.GetAttributes().Where(a => a.AttributeClass!.ToDisplayString().Contains("RegisterPackRat")).Any());
				.Where(t => SymbolHelper.HasAttribute(t, "BigRedProf.Data.RegisterPackRat"));
		}

		public IEnumerable<ISymbol> GetModelClasses2()
		{
			//return _compilation.GlobalNamespace.GetTypeMembers();

			PrintMembers(_compilationContext.Compilation.GlobalNamespace);

			return new SymbolEnumerable(_compilationContext.Compilation.GlobalNamespace);
		}

		public void PrintMembers(ISymbol symbol)
		{
			if (symbol.Name.Contains("Console") || symbol.Name.Contains("PackRat") || symbol.Name.Contains("BigRedProf"))
				Debug.WriteLine($"{symbol.Name} : {symbol.Kind}");

			INamespaceOrTypeSymbol? namespaceOrTypeSymbol = symbol as INamespaceOrTypeSymbol;
			if (namespaceOrTypeSymbol != null)
			{
				foreach(ISymbol childSymbol in namespaceOrTypeSymbol.GetMembers())
					PrintMembers(childSymbol);
			}
		}
		#endregion
	}
}
