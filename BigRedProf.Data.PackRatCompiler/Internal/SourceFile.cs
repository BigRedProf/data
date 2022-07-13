﻿using BigRedProf.Data;
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
		public SourceFile(FileInfo fileInfo)
		{ 
			Debug.Assert(fileInfo != null);

			using (FileStream stream = fileInfo.OpenRead())
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
		#endregion

		#region private methods
		private bool FoundRegisterPackRatAttribute()
		{
			CompilationUnitSyntax root = _syntaxTree.GetCompilationUnitRoot();
			IEnumerable<SyntaxNode> classesWithRegisterPackRatAttribute = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Where(
					n => n.AttributeLists.Any(
						a => a.Attributes.Any(
							attr => SyntaxHelper.IsAttribute(attr, typeof(RegisterPackRatAttribute))
						)
					)
				);

			return classesWithRegisterPackRatAttribute.Any();
		}
		#endregion
	}
}
