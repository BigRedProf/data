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
	internal static class SyntaxHelper
	{
		#region methods
		public static bool IsAttribute(AttributeSyntax attributeSyntax, Type candidateAttributeType)
		{
			// TODO: handle fully qualified case

			Debug.Assert(candidateAttributeType.Name.EndsWith("Attribute"));
			string candidateAttributeName = candidateAttributeType.Name.Substring(
				0,
				candidateAttributeType.Name.Length - ("Attribute").Length
			);

			bool result = attributeSyntax.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Any(
					s => s.Identifier.ValueText == candidateAttributeName
				);

			return result;
		}

		public static PackFieldInfo GetPackFieldInfo(IFieldSymbol symbol)
		{
			PackFieldInfo packFieldInfo = new PackFieldInfo();

			AttributeData packFieldAttribute =  symbol.GetAttributes()
				.Where(a => a.AttributeClass!.Name == "PackFieldAttribute")
				.First();

			packFieldInfo.Name = symbol.Name;
			packFieldInfo.Type = symbol.Type.ToDisplayString();
			packFieldInfo.Position = (int) packFieldAttribute.ConstructorArguments[0].Value!;
			packFieldInfo.SchemaId = (string) packFieldAttribute.ConstructorArguments[1].Value!;
			packFieldInfo.SourceLineNumber = symbol.Locations[0].GetLineSpan().StartLinePosition.Line;
			packFieldInfo.SourceColumn = symbol.Locations[0].GetLineSpan().StartLinePosition.Character + 1;
			return packFieldInfo;
		}
		#endregion
	}
}
