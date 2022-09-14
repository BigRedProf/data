using BigRedProf.Data;
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
	internal class SourceFile
	{
		#region fields
		private ICompilationContext _compilationContext;
		private string _filePath;
		private SyntaxTree _syntaxTree;
		private SemanticModel _semanticModel;
		#endregion

		#region constructors
		public SourceFile(ICompilationContext compilationContext, Stream stream, string filePath)
		{
			Debug.Assert(compilationContext != null);
			Debug.Assert(stream != null);
			Debug.Assert(filePath != null);

			_compilationContext = compilationContext;
			_filePath = filePath;

			using (StreamReader reader = new StreamReader(stream))
			{
				string inputFileText = reader.ReadToEnd();
				_syntaxTree = CSharpSyntaxTree.ParseText(inputFileText);
			}

			//string dotNetCoreDir = Path.GetDirectoryName(typeof(object).Assembly.Location);

			/*CSharpCompilation compilation = CSharpCompilation.Create(
				null, 
				new SyntaxTree[] { _syntaxTree, packFieldSyntaxTree }, 
				new MetadataReference[] { MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "mscorlib.dll"))}
			);
			*/

			// TODO: THIS Gets cached, but may still want to move it our of here if we stick with this approach
			//CSharpCompilation compilation = (CSharpCompilation) _compilationContext.Project.GetCompilationAsync().Result
			//	.AddSyntaxTrees(_syntaxTree);
			Debug.WriteLine("** Compilation 1...");
			CSharpCompilation compilation1 = (CSharpCompilation)_compilationContext.Project.GetCompilationAsync().Result;
			ReportCompilationDiagnostics(compilation1);

			Debug.WriteLine("** Compilation 2...");
			CSharpCompilation compilation2 = CSharpCompilation.Create(
				"MyAssembly",
				compilation1!.SyntaxTrees.Append(_syntaxTree),
				compilation1.References,
				null
			);
			ReportCompilationDiagnostics(compilation2);

			_semanticModel = compilation2.GetSemanticModel(_syntaxTree);
		}

		private void ReportCompilationDiagnostics(CSharpCompilation compilation1)
		{
			ImmutableArray<Diagnostic> diagnostics = compilation1!.GetDiagnostics();
			foreach (Diagnostic diagnostic in diagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					_compilationContext.ReportError(
						CompilerError.CSharpCompilation,
						diagnostic.ToString(),
						_filePath,
						diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
						diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1
					);
				}
				else if (diagnostic.Severity == DiagnosticSeverity.Warning)
				{
					_compilationContext.ReportWarning(
						CompilerError.CSharpCompilation,
						diagnostic.GetMessage(),
						_filePath,
						diagnostic.Location.GetLineSpan().StartLinePosition.Line,
						diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1
					);
				}
			}
		}
		#endregion

		#region properties
		public string FilePath
		{
			get
			{
				return _filePath;
			}
		}
		#endregion

		#region methods
		public bool RegistersPackRat()
		{
			return FoundRegisterPackRatAttribute();
		}

		public string GetNamespace()
		{
			string @namespace = string.Empty;

			CompilationUnitSyntax root = _syntaxTree.GetCompilationUnitRoot();
			SyntaxNode? namespaceNode = root.DescendantNodes()
				.OfType<NamespaceDeclarationSyntax>()
				.FirstOrDefault();
			if(namespaceNode != null)
			{
				ISymbol? namespaceSymbol = _semanticModel.GetDeclaredSymbol(namespaceNode);
				if(namespaceSymbol != null)
					@namespace = namespaceSymbol.ToString() ?? string.Empty;
			}

			return @namespace;
		}

		public IEnumerable<ClassDeclarationSyntax> GetModelClasses()
		{
			CompilationUnitSyntax root = _syntaxTree.GetCompilationUnitRoot();
			IEnumerable<ClassDeclarationSyntax> classesWithRegisterPackRatAttribute = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Where(
					n => n.AttributeLists.Any(
						a => a.Attributes.Any(
							attr => SyntaxHelper.IsAttribute(attr, typeof(RegisterPackRatAttribute))
						)
					)
				);
			return classesWithRegisterPackRatAttribute;
		}

		public IEnumerable<PackFieldInfo> GetFields(ClassDeclarationSyntax @class)
		{
			IEnumerable<PackFieldInfo> fields = @class.DescendantNodes()
				.OfType<FieldDeclarationSyntax>()
				.Where(
					f => f.AttributeLists.Any(
						a => a.Attributes.Any(
							attr => SyntaxHelper.IsAttribute(attr, typeof(PackFieldAttribute))
						)
					)
				)
				.Select(
					s => SyntaxHelper.GetPackFieldInfo((IFieldSymbol)_semanticModel.GetDeclaredSymbol(s.Declaration.Variables[0])!)
				) ;
			return fields;
		}

		#endregion

		#region private methods
		private bool FoundRegisterPackRatAttribute()
		{
			IEnumerable<SyntaxNode> classesWithRegisterPackRatAttribute = GetModelClasses();
			return classesWithRegisterPackRatAttribute.Any();
		}
		#endregion
	}
}
