using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class CSharpWriter : IDisposable
	{
		#region fields
		private StreamWriter _streamWriter;
		private int _indentation;
		private bool _isDisposed;
		#endregion

		#region constructors
		public CSharpWriter(Stream stream)
		{
			Debug.Assert(stream != null);

			_streamWriter = new StreamWriter(stream);
			_indentation = 0;
		}
		#endregion

		#region methods
		public void WriteOpeningCurlyBrace()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CSharpWriter));

			WriteIndentation();
			_streamWriter.Write('{');
			_streamWriter.WriteLine();

			++_indentation;
		}

		public void WriteClosingCurlyBrace()
		{
			if (_isDisposed)
				throw new ObjectDisposedException(nameof(CSharpWriter));

			--_indentation;

			WriteIndentation();
			_streamWriter.Write('}');
			_streamWriter.WriteLine();
		}

		public void WriteLine(string lineOfCode)
		{
			WriteIndentation();
			_streamWriter.WriteLine(lineOfCode);
		}

		public void WriteLine()
		{
			_streamWriter.WriteLine();
		}
		#endregion

		#region IDisposable methods
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_streamWriter.Dispose();
			}

			_isDisposed = true;
		}

		~CSharpWriter()
		{
			Dispose(false);
		}
		#endregion

		#region private methods
		private void WriteIndentation()
		{
			_streamWriter.Write(new String('\t', _indentation));
		}
		#endregion
	}
}
