using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler
{
	public class CompilationContext : ICompilationContext
	{
		#region fields
		private StreamWriter _errorWriter;
		private StreamWriter _warningWriter;
		private int _exitCode;
		private Project _project;
		#endregion

		#region constructors
		public CompilationContext(Project project)
		{
			_errorWriter = new StreamWriter(Console.OpenStandardOutput());
			_warningWriter = _errorWriter;
			_exitCode = 0;
			_project = project;
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

		public Project Project
		{
			get 
			{
				return _project;
			}
		}
		#endregion

		#region ICompilationContext methods
		public void ReportError(int code, string message, string? filePath, int? lineNumber, int? column)
		{
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
			string output = FormatOutput("warning", code, message, filePath, lineNumber, column);
			_warningWriter.WriteLine(output);
			_warningWriter?.Flush();

#if DEBUG
			Debug.WriteLine(output);
#endif
		}
		#endregion

		#region private methods
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
	}
}
