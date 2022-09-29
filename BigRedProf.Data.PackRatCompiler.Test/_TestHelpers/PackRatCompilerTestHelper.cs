using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.PackRatCompiler;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal static class PackRatCompilerTestHelper
	{
		#region class constructors
		static PackRatCompilerTestHelper()
		{
			MSBuildLocator.RegisterDefaults();
		}
		#endregion

		#region methods
		public static Stream GetResource(string path)
		{
			Assembly assembly = typeof(PackRatCompilerTestHelper).GetTypeInfo().Assembly;
			path = path.Replace('/', '.').Replace('\\', '.');
			string[] names = assembly.GetManifestResourceNames();
			string resourceName = $"{assembly.GetName().Name}.{path}";
			Stream? resource = assembly.GetManifestResourceStream(resourceName);
			if (resource == null)
				throw new ArgumentException($"Resource {path} not found.", nameof(path));

			return resource;
		}

		public static void TestGeneratePackRat(string modelResourcePath, string expectedPackRatResourcePath)
		{
			Debug.Assert(modelResourcePath != null);
			Debug.Assert(expectedPackRatResourcePath != null);

			StreamWriter stdoutStreamWriter = new StreamWriter(Console.OpenStandardOutput());
			stdoutStreamWriter.AutoFlush = true;
			using (CompilationContext compilationContext = new CompilationContext(stdoutStreamWriter, stdoutStreamWriter))
			{
				string hackHackProjectPath = @"C:\code\BigRedProf\data\BigRedProf.Data\BigRedProf.Data.csproj";
				compilationContext.AddProject(new FileInfo(hackHackProjectPath));

				PackRatGenerator packRatGenerator = new PackRatGenerator(compilationContext);
				Stream model = PackRatCompilerTestHelper.GetResource(modelResourcePath);
				Stream expectedPackRatStream = PackRatCompilerTestHelper.GetResource(expectedPackRatResourcePath);
				string expectedPackRat = ReadStream(expectedPackRatStream);

				string modelCSharp = ReadStream(model);
				CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree) compilationContext.AddCSharp(modelCSharp);
				ClassDeclarationSyntax classDeclarationSyntax = GetModelClasses(syntaxTree).First();
				INamedTypeSymbol modelSymbol = GetTypes(compilationContext.Compilation.GlobalNamespace)
					.Where(t => t.Name == classDeclarationSyntax.Identifier.ToString())
					.First();

				MemoryStream actualPackRatStream = new MemoryStream();
				packRatGenerator.GeneratePackRat(modelSymbol, actualPackRatStream);
				actualPackRatStream.Close();
				MemoryStream actualPackRatStreamForRead = new MemoryStream(actualPackRatStream.ToArray());
				string actualPackRat = ReadStream(actualPackRatStreamForRead);

				Assert.Equal(expectedPackRat, actualPackRat);
			}
		}

		public static string ReadStream(Stream stream)
		{
			Debug.Assert(stream != null);

			string? output = null;
			using (StreamReader reader = new StreamReader(stream))
			{
				output = reader.ReadToEnd();
			}

			return output;
		}

		public static IEnumerable<ClassDeclarationSyntax> GetModelClasses(CSharpSyntaxTree syntaxTree)
		{
			CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
			IEnumerable<ClassDeclarationSyntax> classesWithRegisterPackRatAttribute = root.DescendantNodes()
				.OfType<ClassDeclarationSyntax>()
				.Where(
					n => n.AttributeLists.Any(
						a => a.Attributes.Any(
							attr => IsAttribute(attr, typeof(RegisterPackRatAttribute))
						)
					)
				);
			return classesWithRegisterPackRatAttribute;
		}

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

		public static IEnumerable<INamedTypeSymbol> GetTypes(INamespaceSymbol namespaceSymbol)
		{
			// get the types in this namespace
			IEnumerable<INamedTypeSymbol> types = namespaceSymbol.GetTypeMembers();

			// and recursively traverse descendant namespace to add their types
			IEnumerable<INamespaceSymbol> childNamespaces = namespaceSymbol.GetNamespaceMembers();
			foreach (INamespaceSymbol childNamespace in childNamespaces)
			{
				IEnumerable<INamedTypeSymbol> descendantNamespaces = GetTypes(childNamespace);
				types = types.Concat(descendantNamespaces);
			}

			return types;
		}
		#endregion
	}
}
