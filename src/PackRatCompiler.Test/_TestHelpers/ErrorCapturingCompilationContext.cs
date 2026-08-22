using BigRedProf.Data.PackRatCompiler;
using Microsoft.CodeAnalysis.CSharp;

namespace BigRedProf.Data.Test._TestHelpers
{
	/// <summary>
	/// Wraps a real compilation context and records the errors reported against it, so a test
	/// can assert that the generator rejected something.
	/// </summary>
	public sealed class ErrorCapturingCompilationContext : ICompilationContext
	{
		#region fields
		private readonly ICompilationContext _inner;
		private readonly List<(int Code, string Message)> _errors = new List<(int, string)>();
		#endregion

		#region constructors
		public ErrorCapturingCompilationContext(ICompilationContext inner)
		{
			_inner = inner;
		}
		#endregion

		#region properties
		public int ExitCode => _inner.ExitCode;
		public CSharpCompilation Compilation => _inner.Compilation;
		public IReadOnlyList<(int Code, string Message)> Errors => _errors;
		#endregion

		#region ICompilationContext methods
		public void AddProject(FileInfo projectFile)
		{
			_inner.AddProject(projectFile);
		}

		public void ReportError(int code, string message, string? filePath, int? lineNumber, int? column)
		{
			_errors.Add((code, message));
		}

		public void ReportWarning(int code, string message, string? filePath, int? lineNumber, int? column)
		{
		}
		#endregion
	}
}
