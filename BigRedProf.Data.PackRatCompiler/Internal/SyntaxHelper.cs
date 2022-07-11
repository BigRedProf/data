using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
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

			bool result = attributeSyntax.DescendantNodes()
				.OfType<IdentifierNameSyntax>()
				.Any(
					s => s.Identifier.ValueText == candidateAttributeType.Name
				);

			return result;
		}
		#endregion
	}
}
