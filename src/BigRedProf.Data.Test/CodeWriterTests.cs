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

			byte[] bytes = stream.ToArray();
			Assert.Single(bytes);
			Assert.Equal(1, bytes[0]);
		}

		[Fact]
		public void WritingAMediumCodeShouldWork()
		{
			MemoryStream stream = new MemoryStream();
			CodeWriter codeWriter = new CodeWriter(stream);

			codeWriter.WriteCode(new Code(1, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1));
			codeWriter.Dispose();

			byte[] bytes = stream.ToArray();
			Assert.Equal(3, bytes.Length);
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
			codeWriter.WriteCode("0");
			codeWriter.WriteCode(new Code(0, 1));
			codeWriter.WriteCode(new Code(0, 0, 1));
			codeWriter.WriteCode(new Code(1, 1, 1));
			codeWriter.Dispose();

			byte[] bytes = stream.ToArray();
			Assert.Equal(3, bytes.Length);
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

			byte[] bytes = stream.ToArray();
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

		[Fact]
		[Trait("Region", "methods")]
		public void ToDebugCode_ShouldWorkThrowWhenStartIsNegative()
		{
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeWriter.ToDebugCode(-1);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void ToDebugCode_ShouldWorkThrowWhenLengthIsNegative()
		{
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeWriter.ToDebugCode(0, -1);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void ToDebugCode_ShouldWorkThrowWhenLengthIsTooLong()
		{
			CodeWriter codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("1111 0000 1111 000");
			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeWriter.ToDebugCode(0, 16);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void ToDebugCode_ShouldWorkForOneByteWrites()
		{
			CodeWriter codeWriter;

			codeWriter	= new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("0");
			Assert.Equal<Code>("0", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("1");
			Assert.Equal<Code>("1", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("1011 001");
			Assert.Equal<Code>("1011 001", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("1011 1101");
			Assert.Equal<Code>("11 1101", codeWriter.ToDebugCode(2));

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("0110 0010");
			Assert.Equal<Code>("0001", codeWriter.ToDebugCode(3, 4));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void ToDebugCode_ShouldWorkForMultiByteWrites()
		{
			CodeWriter codeWriter;

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("00000000 00000000");
			Assert.Equal<Code>("00000000 00000000", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("11111111 11111111");
			Assert.Equal<Code>("11111111 11111111", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("11111111 11111111 0101010");
			Assert.Equal<Code>("11111111 11111111 0101010", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("11111111 11111111 0000111");
			Assert.Equal<Code>("11111111 11111111 0000111", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("10110001 00110110 10111101 0");
			Assert.Equal<Code>("10110001 00110110 10111101 0", codeWriter.ToDebugCode());

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("10110001 00110110 10111101 0");
			Assert.Equal<Code>("110001 00110110 10111101 0", codeWriter.ToDebugCode(2));

			codeWriter = new CodeWriter(new MemoryStream());
			codeWriter.WriteCode("10110001 00110110 10111101 0");
			Assert.Equal<Code>("10001 00110110 1", codeWriter.ToDebugCode(3, 14));
		}
		#endregion
	}
}
