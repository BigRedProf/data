using System;
using System.IO;
using System.Linq;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class CodeWriterTests
	{
		#region constructor tests
		[Fact]
		public void ConstructorShouldThrowIfStreamIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new CodeWriter(null);
				}
			);
		}
		#endregion

		#region IDisposable tests
		[Fact]
		public void WriteCodeShouldThrowWhenDisposed()
		{
			Stream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);
			
			codeWriter.Dispose();

			Assert.Throws<ObjectDisposedException>(
				() =>
				{
					codeWriter.WriteCode(new Code(1));
				}
			);
		}
		#endregion

		#region main tests
		[Fact]
		public void WritingAShortCodeShouldWork()
		{
			MemoryStream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);

			codeWriter.WriteCode(new Code(1, 0));
			codeWriter.Dispose();

			byte[] bytes = stream.GetBuffer();
			Assert.Equal(1, bytes.Length);
			Assert.Equal(1, bytes[0]);
		}

		[Fact]
		public void WritingAMediumCodeShouldWork()
		{
			MemoryStream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);

			codeWriter.WriteCode(new Code(1, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1));
			codeWriter.Dispose();

			byte[] bytes = stream.GetBuffer();
			Assert.Equal(21, bytes.Length);
			Assert.Equal(0b01011101, bytes[0]);
			Assert.Equal(0b01001110, bytes[1]);
			Assert.Equal(0b00011110, bytes[2]);
		}

		[Fact]
		public void WritingMultipleCodesShouldWork()
		{
			MemoryStream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);

			codeWriter.WriteCode(new Code(1, 0, 1));
			codeWriter.WriteCode(new Code(1, 1, 0, 1, 0, 0, 1, 1, 1));
			codeWriter.WriteCode(new Code(0));
			codeWriter.WriteCode(new Code(0, 1));
			codeWriter.WriteCode(new Code(0, 0, 1));
			codeWriter.WriteCode(new Code(1, 1, 1));
			codeWriter.Dispose();

			byte[] bytes = stream.GetBuffer();
			Assert.Equal(21, bytes.Length);
			Assert.Equal(0b01011101, bytes[0]);
			Assert.Equal(0b01001110, bytes[1]);
			Assert.Equal(0b00011110, bytes[2]);
		}

		[Fact]
		public void WritingByteAlignedBigCodesShouldWork()
		{
			MemoryStream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);

			codeWriter.WriteCode(new Code(1, 0, 1));
			codeWriter.AlignToNextByteBoundary();
			codeWriter.WriteCode(new Code(Enumerable.Repeat(0b11110001, 8192).Select(x => (byte)x).ToArray()));
			codeWriter.WriteCode(new Code(1, 1, 1));
			codeWriter.Dispose();

			byte[] bytes = stream.GetBuffer();
			Assert.Equal(8194, bytes.Length);
			Assert.Equal(0b00000101, bytes[0]);
			Assert.Equal(0b11110001, bytes[1]);
			Assert.Equal(0b11110001, bytes[2]);
			Assert.Equal(0b11110001, bytes[3]);
			Assert.Equal(0b11110001, bytes[8190]);
			Assert.Equal(0b11110001, bytes[8191]);
			Assert.Equal(0b11110001, bytes[8192]);
			Assert.Equal(0b00000111, bytes[8193]);
		}
		#endregion
	}
}
