using BigRedProf.Data.PackRatCompiler.Internal.Symbols;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

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
			_compilationContext.AddProject(projectFile);
		}
		#endregion

		#region methods
		public IEnumerable<INamedTypeSymbol> GetGeneratePackRatClasses()
		{
			return SymbolHelper.GetTypes(_compilationContext.Compilation.GlobalNamespace)
				.Where(t => SymbolHelper.HasAttribute(t, "BigRedProf.Data.GeneratePackRat"));
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
