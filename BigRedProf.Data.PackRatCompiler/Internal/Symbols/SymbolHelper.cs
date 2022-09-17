using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal.Symbols
{
	internal static class SymbolHelper
	{
		#region functions
		public static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceSymbol namespaceSymbol)
		{
			// get the types in this namespace
			IEnumerable<INamedTypeSymbol> types = namespaceSymbol.GetTypeMembers();
			
			// and recursively traverse descendant namespace to add their types
			IEnumerable<INamespaceSymbol> childNamespaces = namespaceSymbol.GetNamespaceMembers();
			foreach(INamespaceSymbol childNamespace in childNamespaces)
			{
				IEnumerable<INamedTypeSymbol> descendantNamespaces = GetTypes(childNamespace);
				types = types.Concat(descendantNamespaces);
			}

			return types;
		}

		public static IEnumerable<IFieldSymbol> GetFields(INamedTypeSymbol typeSymbol)
		{
			return typeSymbol.GetMembers().OfType<IFieldSymbol>();
		}

		public static IEnumerable<AttributeData> GetAttributes(
			ISymbol symbol,
			string fullyQualifiedAttributeName)
		{
			return symbol.GetAttributes()
				.Where(a => a.AttributeClass?.ToDisplayString() == fullyQualifiedAttributeName + "Attribute");
		}

		public static bool HasAttribute(
			ISymbol symbol, 
			string fullyQualifiedAttributeName)
		{
			return GetAttributes(symbol, fullyQualifiedAttributeName)
				.Any();
		}

		public static IList<PackFieldInfo> GetPackRatFields(INamedTypeSymbol modelClass)
		{
			Console.WriteLine($"** Dumping debug attribute data for {modelClass.Name}...");
			foreach (AttributeData attribute in modelClass.GetAttributes())
			{
				Console.WriteLine($"@{attribute.AttributeClass}");
				Console.WriteLine($"There are {attribute.ConstructorArguments.Length} constructor arguments.");
				foreach (TypedConstant typedConstant in attribute.ConstructorArguments)
					Console.WriteLine(typedConstant.Value);
			}

			return GetFields(modelClass).
				Where(f => HasAttribute(f, "BigRedProf.Data.PackField"))
				.Select(f => CreatePackFieldInfo(f))
				.OrderBy(f => f.Position)
				.ToList();
		}

		public static PackFieldInfo CreatePackFieldInfo(IFieldSymbol field)
		{
			AttributeData packFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackField").First();
			LinePosition startLinePosition = field.Locations[0].GetLineSpan().StartLinePosition;
			return new PackFieldInfo()
			{
				Name = field.Name,
				Type = field.Type.Name,
				Position = (int)packFieldAttribute.ConstructorArguments[0].Value!,
				SchemaId = (string)packFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}
		#endregion
	}
}
