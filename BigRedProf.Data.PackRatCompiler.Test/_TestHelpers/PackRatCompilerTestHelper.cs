using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.PackRatCompiler;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
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

			MSBuildLocator.RegisterDefaults();
			using (MSBuildWorkspace workspace = MSBuildWorkspace.Create())
			{
				Debug.WriteLine("MSBuild workspace created...");
				foreach (WorkspaceDiagnostic diagnostic in workspace.Diagnostics)
					Debug.WriteLine(diagnostic);
				Debug.WriteLine(new String('-', 79));

				string hackHackProjectPath = @"C:\code\BigRedProf\data\BigRedProf.Data.PackRatCompiler\BigRedProf.Data.PackRatCompiler.csproj";
				Project project = workspace.OpenProjectAsync(hackHackProjectPath).Result;

				ICompilationContext compilationContext = new CompilationContext(project);

				PackRatGenerator packRatGenerator = new PackRatGenerator(compilationContext);
				Stream model = PackRatCompilerTestHelper.GetResource(modelResourcePath);
				Stream expectedPackRatStream = PackRatCompilerTestHelper.GetResource(expectedPackRatResourcePath);
				string expectedPackRat = ReadStream(expectedPackRatStream);

				MemoryStream actualPackRatStream = new MemoryStream();
				//packRatGenerator.GeneratePackRat(model, actualPackRatStream, modelResourcePath);
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
		#endregion	
	}
}
