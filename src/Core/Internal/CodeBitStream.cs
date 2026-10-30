using System;
using System.Diagnostics;
using System.IO;

namespace BigRedProf.Data.Core.Internal
{
	/// <summary>
	/// A write-only <see cref="BitStream"/> that builds a <see cref="Code"/> of the exact bit
	/// length written. This replaces <c>CodeStream</c> inside <see cref="PiedPiper"/>.
	/// </summary>
	/// <remarks>
	/// <see cref="ToCode"/> reports the bits that were written, not a length rounded up to a byte.
	/// Unused high bits of the final byte are zero, which is the same padding a byte stream would
	/// store, and they are not part of the code's length. Disposal does not extend that length:
	/// a code has no separate bit-count channel, so rounding it up would change every packed model.
	/// </remarks>
	internal sealed class CodeBitStream : BitStream
	{
		#region static fields
		private static readonly byte[] _bitMask = new byte[]
		{
			0b00000001,
			0b00000010,
			0b00000100,
			0b00001000,
			0b00010000,
			0b00100000,
			0b01000000,
			0b10000000
		};
		#endregion

		#region fields
		private readonly MemoryStream _bytes;
		private byte _currentByte;
		private int _offsetIntoCurrentByte;
		#endregion

		#region constructors
		public CodeBitStream()
		{
			_bytes = new MemoryStream();
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;
		}
		#endregion

		#region BitStream properties
		/// <inheritdoc/>
		public override bool CanRead
		{
			get
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public override bool CanWrite
		{
			get
			{
				return !IsDisposed;
			}
		}

		/// <inheritdoc/>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// The bits written so far, at their exact length.
		/// </summary>
		/// <returns>The code.</returns>
		public Code ToCode()
		{
			ThrowIfDisposed();
			ThrowIfFaulted();

			byte[] completed = _bytes.ToArray();
			if (_offsetIntoCurrentByte == 0)
				return new Code(completed, completed.Length * 8);

			byte[] withPartial = new byte[completed.Length + 1];
			Array.Copy(completed, 0, withPartial, 0, completed.Length);
			withPartial[completed.Length] = _currentByte;
			return new Code(withPartial, (completed.Length * 8) + _offsetIntoCurrentByte);
		}
		#endregion

		#region BitStream methods
		/// <inheritdoc/>
		public override Code ReadBits(int bitCount)
		{
			PrepareRead(bitCount);
			return new Code(0);
		}

		/// <inheritdoc/>
		public override void WriteBits(Code code)
		{
			PrepareWrite(code);
			if (code.Length == 0)
				return;

			RunIo(
				delegate()
				{
					if (_offsetIntoCurrentByte == 0)
						WriteCodeFast(code);
					else
						WriteCodeSlow(code);

					AdvancePosition(code.Length);
				}
			);
		}

		/// <inheritdoc/>
		public override void AlignToNextByteBoundary()
		{
			PrepareOperation();
			if (_offsetIntoCurrentByte == 0)
				return;

			int padding = 8 - _offsetIntoCurrentByte;
			RunIo(
				delegate()
				{
					WriteCurrentByte();
					AdvancePosition(padding);
				}
			);
		}

		/// <inheritdoc/>
		public override void Flush()
		{
			base.Flush();
		}
		#endregion

		#region BitStream methods (dispose)
		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			_bytes.Dispose();
		}
		#endregion

		#region private methods
		private void WriteCurrentByte()
		{
			_bytes.WriteByte(_currentByte);
			_currentByte = 0;
			_offsetIntoCurrentByte = 0;
		}

		private void WriteBit(Bit bit)
		{
			if (bit == 1)
			{
				byte bitMask = _bitMask[_offsetIntoCurrentByte];
				_currentByte |= bitMask;
			}

			++_offsetIntoCurrentByte;
			if (_offsetIntoCurrentByte == 8)
				WriteCurrentByte();
		}

		private void WriteCodeFast(Code code)
		{
			Debug.Assert(code != null);
			Debug.Assert(_offsetIntoCurrentByte == 0);

			int fullByteLength = code.Length / 8;
			int lastByteBitLength = code.Length % 8;
			if (fullByteLength > 0)
				_bytes.Write(code.ByteArray, 0, fullByteLength);

			if (lastByteBitLength != 0)
				WriteCodeSlow(code[fullByteLength * 8, lastByteBitLength]);
		}

		private void WriteCodeSlow(Code code)
		{
			Debug.Assert(code != null);

			foreach (Bit bit in code)
				WriteBit(bit);
		}
		#endregion
	}
}
