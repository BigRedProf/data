using BigRedProf.Data.PackRatCompiler;
using Microsoft.Build.Locator;
using System.Reflection;

namespace BigRedProf.Data.Test._TestHelpers
{
	/// <summary>
	/// Opens BigRedProf.Data.Core once and shares the resulting Roslyn compilation with every
	/// test in a class.
	/// </summary>
	/// <remarks>
	/// Opening the project is by far the most expensive thing these tests do: it is a real
	/// MSBuild design-time build followed by a full semantic pass over Core, and it does not
	/// depend on which model is being generated. Paying for it once per test made a six-test
	/// class the slowest thing in the repository. Sharing is safe because each test adds its
	/// own uniquely named model type and the generator reads nothing from the compilation
	/// beyond the symbol it is handed.
	/// </remarks>
	public sealed class CompilationContextFixture : IDisposable
	{
		#region constants
		private const string CoreProjectPathKey = "CoreProjectPath";
		#endregion

		#region class constructors
		static CompilationContextFixture()
		{
			// The MSBuild workspace runs its design-time builds in a separate BuildHost process,
			// which locates MSBuild for itself. Registering here is not merely unnecessary: by
			// the time this runs the test host has already loaded MSBuild assemblies, and
			// MSBuildLocator throws when it finds them.
			if (!MSBuildLocator.IsRegistered)
			{
				try
				{
					MSBuildLocator.RegisterDefaults();
				}
				catch (InvalidOperationException)
				{
					// Already loaded by the host. Nothing to do.
				}
			}
		}
		#endregion

		#region fields
		private readonly StreamWriter _stdoutStreamWriter;
		private readonly CompilationContext _compilationContext;
		#endregion

		#region constructors
		public CompilationContextFixture()
		{
			_stdoutStreamWriter = new StreamWriter(Console.OpenStandardOutput());
			_stdoutStreamWriter.AutoFlush = true;

			_compilationContext = new CompilationContext(_stdoutStreamWriter, _stdoutStreamWriter);

			// Supplied by the build rather than computed from the working directory: the MSBuild
			// workspace builds out of process, so the working directory is not ours to assume,
			// and walking up from the output directory only works for one particular output
			// layout. See the AssemblyMetadata item in this project's .csproj.
			_compilationContext.AddProject(new FileInfo(GetCoreProjectPath()));
		}
		#endregion

		#region methods
		private static string GetCoreProjectPath()
		{
			AssemblyMetadataAttribute? metadata = typeof(CompilationContextFixture).Assembly
				.GetCustomAttributes<AssemblyMetadataAttribute>()
				.SingleOrDefault(a => a.Key == CoreProjectPathKey);

			if (metadata == null)
			{
				throw new InvalidOperationException(
					$"The test assembly carries no '{CoreProjectPathKey}' metadata. It is supplied " +
					"by an AssemblyMetadata item in BigRedProf.Data.PackRatCompiler.Test.csproj, " +
					"which these tests cannot run without."
				);
			}

			return metadata.Value!;
		}
		#endregion

		#region properties
		public CompilationContext CompilationContext
		{
			get
			{
				return _compilationContext;
			}
		}
		#endregion

		#region IDisposable methods
		public void Dispose()
		{
			_compilationContext.Dispose();
			_stdoutStreamWriter.Dispose();
		}
		#endregion
	}
}
