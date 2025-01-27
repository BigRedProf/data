using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler
{
	public interface ICompilationContext
	{
		#region properties
		public int ExitCode { get; }
		public CSharpCompilation Compilation { get; }
		#endregion

		#region methods
		void AddProject(FileInfo projectFile);
		void ReportError(int code, string message, string? filePath, int? lineNumber, int? column);
		void ReportWarning(int code, string message, string? filePath, int? lineNumber, int? column);
		#endregion
	}
}
