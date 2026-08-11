using BigRedProf.Data.PackRatCompiler;
using Microsoft.Build.Locator;

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
		#region class constructors
		static CompilationContextFixture()
		{
			// Must happen before the first MSBuildWorkspace is created, and exactly once per
			// process -- RegisterDefaults throws if called twice.
			MSBuildLocator.RegisterDefaults();
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

			// HACKHACK: relative path from the test binary to the Core project.
			string hackHackProjectPath = @"../../../../Core/BigRedProf.Data.Core.csproj";
			_compilationContext.AddProject(new FileInfo(hackHackProjectPath));
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
