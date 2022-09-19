using BigRedProf.Data.PackRatCompiler.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler
{
	public class CompilationContext : ICompilationContext, IDisposable
	{
		#region fields
		private MSBuildWorkspace? _msBuildWorkspace;
		private CSharpCompilation? _compilation;
		private StreamWriter _errorWriter;
		private StreamWriter _warningWriter;
		private int _exitCode;
		#endregion

		#region constructors
		public CompilationContext(StreamWriter errorWriter, StreamWriter warningWriter)
		{
			_msBuildWorkspace = MSBuildWorkspace.Create();
			_compilation = null;
			_errorWriter = errorWriter;
			_warningWriter = warningWriter;
			_exitCode = 0;

			ReportWorkspaceDiagnostics(_msBuildWorkspace, errorWriter, warningWriter);
		}
		#endregion

		#region ICompilationContext properties
		public int ExitCode
		{
			get
			{ 
				return _exitCode; 
			}
		}

		public CSharpCompilation? Compilation
		{
			get 
			{
				return _compilation;
			}
		}
		#endregion

		#region ICompilationContext methods
		public void AddProject(FileInfo projectFile)
		{
			if (_msBuildWorkspace == null)
				throw new ObjectDisposedException(nameof(CompilationContext));

			if (_compilation == null)
			{
				Project project = _msBuildWorkspace.OpenProjectAsync(projectFile.FullName).Result;
				_compilation = (CSharpCompilation)project.GetCompilationAsync().Result!;
				ReportCompilationDiagnostics(_compilation);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public SyntaxTree AddCSharp(string cSharpText)
		{
			if (_msBuildWorkspace == null)
				throw new ObjectDisposedException(nameof(CompilationContext));

			if (_compilation == null)
			{
				throw new NotImplementedException();
			}
			else
			{
				LanguageVersion languageVersion = _compilation.LanguageVersion;
				CSharpParseOptions parseOptions = CSharpParseOptions.Default.WithLanguageVersion(languageVersion);
				CSharpSyntaxTree syntaxTree = (CSharpSyntaxTree) CSharpSyntaxTree.ParseText(cSharpText, parseOptions);
				_compilation = CSharpCompilation.Create(
					"FooSembly",
					_compilation.SyntaxTrees.Append(syntaxTree),
					_compilation.References,
					null);
				ReportCompilationDiagnostics(_compilation);
				
				return syntaxTree;
			}
		}

		public void ReportError(int code, string message, string? filePath, int? lineNumber, int? column)
		{
			if (_msBuildWorkspace == null)
				throw new ObjectDisposedException(nameof(CompilationContext));

			string output = FormatOutput("error", code, message, filePath, lineNumber, column);
			_errorWriter.WriteLine(output);
			_errorWriter?.Flush();

#if DEBUG
			Debug.WriteLine(output);
#endif

			_exitCode = 1;
		}

		public void ReportWarning(int code, string message, string? filePath, int? lineNumber, int? column)
		{
			if (_msBuildWorkspace == null)
				throw new ObjectDisposedException(nameof(CompilationContext));

			string output = FormatOutput("warning", code, message, filePath, lineNumber, column);
			_warningWriter.WriteLine(output);
			_warningWriter?.Flush();

#if DEBUG
			Debug.WriteLine(output);
#endif
		}
		#endregion

		#region IDisposable methods
		public void Dispose()
		{
			if (_msBuildWorkspace != null)
			{
				_msBuildWorkspace.Dispose();
				_msBuildWorkspace = null;
			}
		}
		#endregion

		#region private methods
		private void ReportCompilationDiagnostics(CSharpCompilation compilation)
		{
			ImmutableArray<Diagnostic> diagnostics = compilation!.GetDiagnostics();
			foreach (Diagnostic diagnostic in diagnostics)
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
				{
					this.ReportError(
						CompilerError.CSharpCompilation,
						diagnostic.ToString(),
						diagnostic.Location.MetadataModule?.Name,
						diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
						diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1
					);
				}
				else if (diagnostic.Severity == DiagnosticSeverity.Warning)
				{
					this.ReportWarning(
						CompilerError.CSharpCompilation,
						diagnostic.GetMessage(),
						diagnostic.Location.MetadataModule?.Name,
						diagnostic.Location.GetLineSpan().StartLinePosition.Line,
						diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1
					);
				}
			}
		}

		private string FormatOutput(string level, int code, string message, string? filePath, int? lineNumber, int? column)
		{
			StringBuilder stringBuilder = new StringBuilder(128);
			if (filePath != null)
			{
				stringBuilder.Append(filePath);

				if (lineNumber != null)
				{
					if (column == null)
						stringBuilder.Append($"({lineNumber})");
					else
						stringBuilder.Append($"({lineNumber},{column})");
				}
				stringBuilder.Append(": ");
			}
			stringBuilder.Append(level);
			stringBuilder.Append(' ');
			stringBuilder.Append("PRC");
			stringBuilder.Append(code.ToString());
			stringBuilder.Append(": ");
			stringBuilder.Append(message);

			return stringBuilder.ToString();
		}
		#endregion

		#region private functions
		private void ReportWorkspaceDiagnostics(MSBuildWorkspace workspace, StreamWriter errorWriter, StreamWriter warningWriter)
		{
			ImmutableList<WorkspaceDiagnostic> diagnostics = workspace.Diagnostics;
			foreach (WorkspaceDiagnostic diagnostic in diagnostics)
			{
				if (diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
				{
					errorWriter.WriteLine($"PRC{CompilerError.MSBuildWorkspaceCreation}: {diagnostic.Message}");
				}
				else
				{
					warningWriter.WriteLine($"PRC{CompilerWarning.MSBuildWorkspaceCreation}: {diagnostic.Message}");
				}
#if DEBUG
				Debug.WriteLine(diagnostic.Message);
#endif
			}
		}
		#endregion
	}
}
