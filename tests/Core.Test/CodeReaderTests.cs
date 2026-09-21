using BigRedProf.Data.Core;
using System;
using System.IO;
using BigRedProf.Data.Test._TestHelpers;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class CodeReaderTests
	{
		#region constructors
		[Fact]
		[Trait("Region", "constructors")]
		public void Constructor_ShouldThrowWhenStreamIsNull()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				{
					new CodeReader(null);
				}
			);
		}
		#endregion

		#region methods
		[Fact]
		[Trait("Region", "methods")]
		public void AlignToNextByteBoundary_ShouldThrowWhenObjectIsDisposed()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);
			codeReader.Dispose();

			Assert.Throws<ObjectDisposedException>(
				() =>
				{
					codeReader.AlignToNextByteBoundary();
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void AlignToNextByteBoundary_ShouldWork()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);
			codeReader.Read(3);	// read "101"

			codeReader.AlignToNextByteBoundary();	// skip "10000"

			Code code = codeReader.Read(8);	// read "01010101"
			Assert.Equal<Code>("01010101", code);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldThrowWhenObjectIsDisposed()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);
			codeReader.Dispose();

			Assert.Throws<ObjectDisposedException>(
				() =>
				{
					codeReader.Read(1);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldThrowWhenBitCountIsTooSmall()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);

			// Reading nothing is legitimate and yields the empty code, consuming nothing.
			Assert.Equal(new Code(0), codeReader.Read(0));
			Assert.Equal<Code>("1", codeReader.Read(1));

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeReader.Read(-1);
				}
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeReader.Read(-1);
				}
			);

			Assert.Throws<ArgumentOutOfRangeException>(
				() =>
				{
					codeReader.Read(-20000);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldThrowWhenBitCountIsTooLarge()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					codeReader.Read((bytes.Length * 8) + 1);
				}
			);
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldWorkForSmallCodes()
		{
			byte[] bytes = new byte[] { 0b00001101, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);

			Assert.Equal<Code>("10", codeReader.Read(2));
			Assert.Equal<Code>("1100", codeReader.Read(4));
			Assert.Equal<Code>("0", codeReader.Read(1));
			Assert.Equal<Code>("00", codeReader.Read(2));   // wraps around byte boundary
			Assert.Equal<Code>("101", codeReader.Read(3));
			Assert.Equal<Code>("0101", codeReader.Read(4));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldWorkForLargeCodes()
		{
			byte[] bytes = new byte[] { 0b00001101, 0xFF, 0xFF, 0x00, 0x00, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);

			Assert.Equal<Code>("10", codeReader.Read(2));
			Assert.Equal<Code>("1100", codeReader.Read(4));
			Assert.Equal<Code>("00", codeReader.Read(2));
			Assert.Equal<Code>("11111111 11111111", codeReader.Read(16));
			Assert.Equal<Code>("00000000 0000000", codeReader.Read(15));
			Assert.Equal<Code>("00", codeReader.Read(2));
			Assert.Equal<Code>("101", codeReader.Read(3));
			Assert.Equal<Code>("0101", codeReader.Read(4));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldWorkWhenTheStreamReturnsFewerBytesThanAskedFor()
		{
			// An HTTP request body hands a large payload over a few bytes at a time. Ignoring the
			// count Read returns left the rest of the buffer zero-filled and every later field
			// misaligned, which is how Stories.Api came to refuse every batch of client logs.
			//
			// Ten bytes handed over three at a time take four reads. No byte is zero, so a gap
			// the reader failed to fill cannot pass for data.
			byte[] bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC };
			CodeReader codeReader = new CodeReader(new TrickleStream(bytes, 3));

			Assert.Equal<Code>(new Code(bytes, 80), codeReader.Read(80));
		}

		[Fact]
		[Trait("Region", "methods")]
		public void Read_ShouldThrowWhenTheStreamEndsPartWayThroughACode()
		{
			// A short read that is really the end of the stream is a truncated payload, and must
			// not quietly come back padded with zeroes.
			byte[] bytes = new byte[] { 0xFF, 0xFF, 0xFF };
			CodeReader codeReader = new CodeReader(new TrickleStream(bytes, 2));

			Assert.Throws<InvalidOperationException>(
				() =>
				{
					codeReader.Read(32);
				}
			);
		}
		#endregion

		#region Dispose methods
		[Fact]
		[Trait("Region", "Dispose methods")]
		public void Dispose_ShouldDisposeUnderlyingStream()
		{
			byte[] bytes = new byte[] { 0b00001101, 0xFF, 0xFF, 0x00, 0x00, 0b10101010 };
			Stream stream = new MemoryStream(bytes);
			CodeReader codeReader = new CodeReader(stream);

			codeReader.Dispose();

			Assert.Throws<ObjectDisposedException>(
				() =>
				{
					stream.ReadByte();
				}
			);
		}
		#endregion
	}
}
