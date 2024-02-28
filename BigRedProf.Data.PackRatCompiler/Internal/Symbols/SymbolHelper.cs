using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Tracing;
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

		public static IEnumerable<IPropertySymbol> GetProperties(INamedTypeSymbol typeSymbol)
		{
			return typeSymbol.GetMembers().OfType<IPropertySymbol>();
		}

		public static IEnumerable<ISymbol> GetFieldsAndProperties(INamedTypeSymbol typeSymbol)
		{
			return typeSymbol.GetMembers().Where(m => m is IFieldSymbol || m is IPropertySymbol);
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
			string fullyQualifiedAttributeName
		)
		{
			return GetAttributes(symbol, fullyQualifiedAttributeName)
				.Any();
		}

		public static AttributeData GetAttribute(
			ISymbol symbol,
			string fullyQualifiedAttributeName
		)
		{
			return GetAttributes(symbol, fullyQualifiedAttributeName).First();
		}

		public static object? GetAttributeArgument(AttributeData attribute, int position)
		{
			return attribute.ConstructorArguments[position].Value;
		}

		public static object? GetAttributeArgument(AttributeData attribute, string name)
		{
			return attribute.NamedArguments.Where(na => na.Key == name).FirstOrDefault().Value.Value;
		}

		public static object? GetAttributeArgument(AttributeData attribute, string name, int position)
		{
			object? value = GetAttributeArgument(attribute, name);
			if (value != null)
				return value;

			return GetAttributeArgument(attribute, position);
		}

		public static IList<PackFieldInfo> GetPackRatFields(INamedTypeSymbol modelClass)
		{
			return GetFieldsAndProperties(modelClass).
				Where(
					f => 
						HasAttribute(f, "BigRedProf.Data.PackField")
						|| HasAttribute(f, "BigRedProf.Data.PackListField")
					)
				.Select(m =>
					{
						IFieldSymbol? f = m as IFieldSymbol;
						if(f != null )
							return CreatePackFieldInfo(f);
						IPropertySymbol? p = m as IPropertySymbol;
						if(p != null)
							return CreatePackFieldInfo(p);
						throw new InvalidOperationException("Not field or property.");
					}
				)
				.OrderBy(f => f.Position)
				.ToList();
		}

		public static PackFieldInfo CreatePackFieldInfo(IFieldSymbol field)
		{
			AttributeData? packListFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackListField").FirstOrDefault();
			if (packListFieldAttribute != null)
				return CreatePackListFieldInfo(field);

			AttributeData packFieldAttribute = GetAttributes(field, "BigRedProf.Data.PackField").First();

			string type = field.Type.ToDisplayString();
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
				// HACKHACK: For now, let's treat every enum as an int to simplify the packing of
				// enum fields. Not sure yet if this will prove troublesome elsewhere.
				Type = type.Replace("?", string.Empty),
				IsEnum = IsEnum(field.Type),
				IsNullable = IsPackRatFieldNullable(field, packFieldAttribute),
				ByteAligned = byteAligned,
				Position = (int)packFieldAttribute.ConstructorArguments[0].Value!,
				SchemaId = (string)packFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}

		public static PackFieldInfo CreatePackFieldInfo(IPropertySymbol property)
		{
			AttributeData? packListFieldAttribute = GetAttributes(property, "BigRedProf.Data.PackListField").FirstOrDefault();
			if (packListFieldAttribute != null)
				return CreatePackListFieldInfo(property);

			AttributeData packFieldAttribute = GetAttributes(property, "BigRedProf.Data.PackField").First();

			string type = property.Type.ToDisplayString();
			LinePosition startLinePosition = property.Locations[0].GetLineSpan().StartLinePosition;

			// HACKHACK: Not sure if there a way to "instantiate" the attribute and read the actual
			// ByteAligned property. So in the mean-time we'll just have to know that the default
			// value when not specified is ByteAligned.No. Guess that same knowledge is required
			// below for knowing constructor order too.
			ByteAligned byteAligned = ByteAligned.No;
			if (packFieldAttribute.ConstructorArguments.Length >= 3)
				byteAligned = (ByteAligned)packFieldAttribute.ConstructorArguments[2].Value!;

			return new PackFieldInfo()
			{
				Name = property.Name,
				// HACKHACK: For now, let's treat every enum as an int to simplify the packing of
				// enum fields. Not sure yet if this will prove troublesome elsewhere.
				Type = type.Replace("?", string.Empty),
				IsEnum = IsEnum(property.Type),
				IsNullable = IsPackRatFieldNullable(property, packFieldAttribute),
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
			LinePosition startLinePosition = field.Locations[0].GetLineSpan().StartLinePosition;

			// TODO: account for arrays, List<T>, non-generic lists, etc.

			// HACKHACK: Surely there's a more elegant way to do this.
			bool isElementNullable = false;
			if (type.StartsWith("System.Collections.IList<"))
			{
				if (type.EndsWith("?"))
				{
					type = type.Substring(25, type.Length - 27);
				}
				else
				{
					type = type.Substring(25, type.Length - 26);
				}
				isElementNullable = type.EndsWith("?");
			}
			else if (type.StartsWith("System.Collections.Generic.IList<"))
			{
				if (type.EndsWith("?"))
				{
					type = type.Substring(33, type.Length - 35);
				}
				else
				{
					type = type.Substring(33, type.Length - 34);
				}
				isElementNullable = type.EndsWith("?");
			}
			else
			{
				throw new NotImplementedException(
					$"Can't process field '{field.Name}' of type '{type}'. " +
					"List types other than IList<T> not yet implemented."
				);
			}

			return new PackListFieldInfo()
			{
				Name = field.Name,
				// HACKHACK: For now, let's treat every enum as an int to simplify the packing of
				// enum fields. Not sure yet if this will prove troublesome elsewhere.
				Type = type.Replace("?", string.Empty),
				IsEnum = IsEnum(field.Type),
				IsNullable = IsPackRatFieldNullable(field, packListFieldAttribute),
				ByteAligned = (ByteAligned)packListFieldAttribute.ConstructorArguments[2].Value!,
				IsElementNullable = isElementNullable,
				Position = (int)packListFieldAttribute.ConstructorArguments[0].Value!,
				ElementSchemaId = (string)packListFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}

		public static PackListFieldInfo CreatePackListFieldInfo(IPropertySymbol property)
		{
			AttributeData packListFieldAttribute = GetAttributes(property, "BigRedProf.Data.PackListField").First();

			string type = property.Type.ToDisplayString();
			LinePosition startLinePosition = property.Locations[0].GetLineSpan().StartLinePosition;

			// TODO: account for arrays, List<T>, non-generic lists, etc.

			// HACKHACK: Surely there's a more elegant way to do this.
			bool isElementNullable = false;
			if (type.StartsWith("System.Collections.IList<"))
			{
				if (type.EndsWith("?"))
				{
					type = type.Substring(25, type.Length - 27);
				}
				else
				{
					type = type.Substring(25, type.Length - 26);
				}
				isElementNullable = type.EndsWith("?");
			}
			else if (type.StartsWith("System.Collections.Generic.IList<"))
			{
				if (type.EndsWith("?"))
				{
					type = type.Substring(33, type.Length - 35);
				}
				else
				{
					type = type.Substring(33, type.Length - 34);
				}
				isElementNullable = type.EndsWith("?");
			}
			else
			{
				throw new NotImplementedException(
					$"Can't process field '{property.Name}' of type '{type}'. " +
					"List types other than IList<T> not yet implemented."
				);
			}

			return new PackListFieldInfo()
			{
				Name = property.Name,
				// HACKHACK: For now, let's treat every enum as an int to simplify the packing of
				// enum fields. Not sure yet if this will prove troublesome elsewhere.
				Type = type.Replace("?", string.Empty),
				IsEnum = IsEnum(property.Type),
				IsNullable = IsPackRatFieldNullable(property, packListFieldAttribute),
				ByteAligned = (ByteAligned)packListFieldAttribute.ConstructorArguments[2].Value!,
				IsElementNullable = isElementNullable,
				Position = (int)packListFieldAttribute.ConstructorArguments[0].Value!,
				ElementSchemaId = (string)packListFieldAttribute.ConstructorArguments[1].Value!,
				SourceLineNumber = startLinePosition.Line + 1,
				SourceColumn = startLinePosition.Character
			};
		}

		public static bool IsEnum(ITypeSymbol symbol)
		{
			return symbol.TypeKind == TypeKind.Enum;
		}

		public static bool TryGetNamedArgumentValue<M>(
			AttributeData attribute,
			string argumentName,
			out M? value
		)
		{
			if (!attribute.NamedArguments.Where(kvp => kvp.Key == argumentName).Any())
			{
				value = default;
				return false;
			}

			KeyValuePair<string, TypedConstant>? argument =
				attribute.NamedArguments
				.Where(kvp => kvp.Key == argumentName)
				.First()
			;
			value = (M?)argument.Value.Value.Value;
			return true;
		}

		public static bool IsPackRatFieldNullable(ISymbol field, AttributeData packFieldAttribute)
		{
			bool isNullable;
			if (TryGetNamedArgumentValue<bool?>(packFieldAttribute, "IsNullable", out bool? isExplicitlyNullable))
			{
				// first see if the IsNullable named argument was provided
				isNullable = (isExplicitlyNullable == true);
			}
			else
			{
				// if not, fallback to the Nullable attribute (C# question mark)
				isNullable = SymbolHelper.HasAttribute(field, "System.Runtime.CompilerServices.Nullable");

				if(!isNullable)
				{
					// HACKHACK: The Nullable attribute check above doesn't seem to work
					// for lists. This hack does.
					// UPDATE: I think this is because we're currently using C# 7.3 (.NET 7.0). If we update the
					// prc compiler to a newer version of C# the hack might not be required. For example,
					// you may see (apparently non-critical) prc errors like:
					// error PRC101: (15,16): error CS8370: Feature 'nullable reference types' is not available in
					// C# 7.3. Please use language version 8.0 or greater.
					IFieldSymbol? fieldAsField = field as IFieldSymbol;
					if (fieldAsField != null)
						isNullable = fieldAsField.Type.ToDisplayString().EndsWith("?");
					IPropertySymbol? fieldAsProperty = field as IPropertySymbol;
					if(fieldAsProperty != null)
						isNullable = fieldAsProperty.Type.ToDisplayString().EndsWith("?");
				}
			}

			return isNullable;
		}

		public static string GetFullName(INamedTypeSymbol type)
		{
			return $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";
		}
		#endregion
	}
}
