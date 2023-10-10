using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal class CodeTester
	{
		#region fields
		private MemoryStream _stream;
		private CodeWriter _writer;
		private CodeReader _reader;
		#endregion

		#region constructors
		public CodeTester() 
		{
			_stream = new MemoryStream();
			_writer = new CodeWriter(_stream);
		}
		#endregion

		#region properties
		public CodeWriter Writer
		{
			get
			{
				if (_writer == null)
					throw new InvalidOperationException("Already reading.");

				return _writer;
			}
		}

		public CodeReader Reader
		{
			get
			{
				if (_reader == null)
					throw new InvalidOperationException("Still writing.");

				return _reader;
			}
		}
		#endregion

		#region methods
		public void StopWritingAndStartReading()
		{
			if (_reader != null)
				throw new InvalidOperationException("Reader already created.");

			_writer.Dispose();
			_writer = null;
			byte[] bytes = _stream.ToArray();
			Stream readerStream = new MemoryStream(bytes);
			_reader = new CodeReader(readerStream);
		}

		public void Write(Code code)
		{
			Writer.WriteCode(code);
		}

		public void ReadAndVerify(Code expectedCode)
		{
			Code actualCode = Reader.Read(expectedCode.Length);
			Assert.Equal<Code>(expectedCode, actualCode);
		}
		#endregion
	}
}
