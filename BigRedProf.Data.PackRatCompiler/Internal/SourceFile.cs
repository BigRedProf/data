using BigRedProf.Data;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class SourceFile
	{
		#region fields
		private SyntaxTree _syntaxTree;
		private SemanticModel _semanticModel;
		#endregion

		#region constructors
		public SourceFile(Stream stream)
		{ 
			Debug.Assert(stream != null);

			using(StreamReader reader = new StreamReader(stream))
			{
				string inputFileText = reader.ReadToEnd();
				_syntaxTree = CSharpSyntaxTree.ParseText(inputFileText);
			}
			
			CSharpCompilation compilation = CSharpCompilation.Create(null, new SyntaxTree[] { _syntaxTree });
			_semanticModel = compilation.GetSemanticModel(_syntaxTree);
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

		public IEnumerable<IFieldSymbol> GetFields(ClassDeclarationSyntax @class)
		{
			IEnumerable<IFieldSymbol> fields = @class.DescendantNodes()
				.OfType<FieldDeclarationSyntax>()
				.Where(
					f => f.AttributeLists.Any(
						a => a.Attributes.Any(
							attr => SyntaxHelper.IsAttribute(attr, typeof(PackFieldAttribute))
						)
					)
				)
				.Select(
					s => (IFieldSymbol) _semanticModel.GetDeclaredSymbol(s.Declaration.Variables[0])!
				);
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
