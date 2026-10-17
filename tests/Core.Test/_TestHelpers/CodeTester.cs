using BigRedProf.Data.Core;
using System;
using System.IO;
using Xunit;

namespace BigRedProf.Data.Test._TestHelpers
{
	internal class CodeTester
	{
		#region fields
		private MemoryStream _stream;
		private BitStream _writer;
		private BitStream _reader;
		#endregion

		#region constructors
		public CodeTester() 
		{
			_stream = new MemoryStream();
			_writer = new StreamBitWriter(_stream);
		}
		#endregion

		#region properties
		public BitStream Writer
		{
			get
			{
				if (_writer == null)
					throw new InvalidOperationException("Already reading.");

				return _writer;
			}
		}

		public BitStream Reader
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
			_reader = new StreamBitReader(readerStream);
		}

		public void Write(Code code)
		{
			Writer.WriteBits(code);
		}

		public void ReadAndVerify(Code expectedCode)
		{
			Code actualCode = Reader.ReadBits(expectedCode.Length);
			
			// Compare the text representation of the codes as it makes troubleshooting much easier.
			//Assert.Equal<Code>(expectedCode, actualCode);
			string expectedCodeText = expectedCode.ToString();
			string actualCodeText = actualCode.ToString();
			Assert.Equal(expectedCodeText, actualCodeText);
		}
		#endregion
	}
}
