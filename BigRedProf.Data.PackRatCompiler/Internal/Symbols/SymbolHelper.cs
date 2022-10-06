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
			return GetFields(modelClass).
				Where(
					f => 
						HasAttribute(f, "BigRedProf.Data.PackField")
						|| HasAttribute(f, "BigRedProf.Data.PackListField")
					)
				.Select(f => CreatePackFieldInfo(f))
				.OrderBy(f => f.Position)
				.ToList();
		}

		public static PackFieldInfo CreatePackFieldInfo(IFieldSymbol field)
		{
			AttributeData packListFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackListField").FirstOrDefault();
			if (packListFieldAttribute != null)
				return CreatePackListFieldInfo(field);

			AttributeData packFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackField").First();

			string type = field.Type.ToDisplayString();
			bool isNullable = SymbolHelper.HasAttribute(field, "System.Runtime.CompilerServices.Nullable");
			LinePosition startLinePosition = field.Locations[0].GetLineSpan().StartLinePosition;

			// HACKHACK: Not sure if there a way to "instantiate" the attribute and read the actual
			// ByteAligned property. So in the mean-time we'll just have to know that the default
			// value when not specified is ByteAligned.No. Guess that same knowledge is required
			// below for knowing constructor order too.
			ByteAligned byteAligned = ByteAligned.No;
			if (packFieldAttribute.ConstructorArguments.Length >= 3)
				byteAligned = (ByteAligned)packFieldAttribute.ConstructorArguments[2].Value!;

			return new PackFieldInfo()
			{
				Name = field.Name,
				Type = type
					.Replace("?", string.Empty),
				IsNullable = isNullable,
				ByteAligned = byteAligned,
				Position = (int)packFieldAttribute.ConstructorArguments[0].Value!,
				SchemaId = (string)packFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}

		public static PackListFieldInfo CreatePackListFieldInfo(IFieldSymbol field)
		{
			AttributeData packListFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackListField").First();

			string type = field.Type.ToDisplayString();
			bool isNullable = SymbolHelper.HasAttribute(field, "System.Runtime.CompilerServices.Nullable");
			LinePosition startLinePosition = field.Locations[0].GetLineSpan().StartLinePosition;

			// TODO: account for arrays, List<T>, non-generic lists, etc.

			// HACKHACK: Surely there's a more elegant way to do this.
			bool isElementNullable = false;
			if (type.StartsWith("System.Collections.IList<"))
			{
				if (type.EndsWith("?"))
				{
					type = type.Substring(25, type.Length - 27);
					isNullable = true;
				}
				else
				{
					type = type.Substring(25, type.Length - 26);
					isNullable = false;
				}
				isElementNullable = type.EndsWith("?");
			}
			else
			{
				throw new NotImplementedException("List types other than IList<T> not yet implemented.");
			}

			return new PackListFieldInfo()
			{
				Name = field.Name,
				Type = type
					.Replace("?", string.Empty),
				IsNullable = isNullable,
				ByteAligned = (ByteAligned)packListFieldAttribute.ConstructorArguments[2].Value!,
				IsElementNullable = isElementNullable,
				Position = (int)packListFieldAttribute.ConstructorArguments[0].Value!,
				ElementSchemaId = (string)packListFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}
		#endregion
	}
}
