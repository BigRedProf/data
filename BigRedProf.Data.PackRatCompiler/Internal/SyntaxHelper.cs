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
		#endregion
	}
}
