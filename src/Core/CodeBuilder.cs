using System;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Builds a <see cref="Code"/> bit by bit.
	/// </summary>
	/// <remarks>
	/// A code is a value: two codes with the same bits are the same code, and that is what lets
	/// codes be compared, fingerprinted, and used as dictionary keys. So a code cannot be modified
	/// once it exists, and the filling-in happens here instead.
	/// </remarks>
	public class CodeBuilder
	{
		#region fields
		private readonly byte[] _byteArray;
		private readonly int _length;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a builder for a code of the specified length, with every bit zero.
		/// </summary>
		/// <param name="length">The length of the code, in bits.</param>
		public CodeBuilder(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length), "A code cannot have a negative length.");

			if (length > Code.MaxLength)
				throw new ArgumentOutOfRangeException(nameof(length), "A code cannot exceed 1 gigabit in length.");

			_byteArray = new byte[(length + 7) / 8];
			_length = length;
		}

		/// <summary>
		/// Creates a builder seeded with an existing code.
		/// </summary>
		/// <param name="code">The code to start from.</param>
		public CodeBuilder(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			// Copied, never aliased: the seed code is immutable and must stay that way while
			// this builder is written to.
			_byteArray = new byte[code.ByteArray.Length];
			Array.Copy(code.ByteArray, 0, _byteArray, 0, code.ByteArray.Length);
			_length = code.Length;
		}
		#endregion

		#region properties
		/// <summary>
		/// The length of the code being built, in bits.
		/// </summary>
		public int Length => _length;
		#endregion

		#region indexers
		/// <summary>
		/// Gets or sets the value of a specific <see cref="Bit"/> within the code being built.
		/// </summary>
		/// <param name="offset">The offset into the code.</param>
		public Bit this[int offset]
		{
			get
			{
				if (offset < 0 || offset >= _length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				return (_byteArray[offset / 8] & GetMaskAt(offset)) == 0 ? 0 : 1;
			}
			set
			{
				if (offset < 0 || offset >= _length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				if (value == 1)
					_byteArray[offset / 8] |= GetMaskAt(offset);
				else
					_byteArray[offset / 8] &= (byte) ~GetMaskAt(offset);
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Sets a range of bits from another code.
		/// </summary>
		/// <param name="offset">The offset into the code being built.</param>
		/// <param name="code">The code to copy in. Its full length is written.</param>
		public void SetRange(int offset, Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			if (offset < 0 || offset >= _length)
				throw new ArgumentOutOfRangeException(nameof(offset));

			if (offset + code.Length > _length)
			{
				throw new ArgumentOutOfRangeException(
					nameof(code),
					"The code does not fit at the specified offset."
				);
			}

			int currentOffset = offset;
			if ((offset % 8) == 0 && code.Length >= 8)
			{
				// Whole bytes land byte-aligned, so copy them wholesale.
				byte[] sourceBytes = code.ByteArray;
				int byteLengthOfCode = code.Length / 8;
				for (int i = 0; i < byteLengthOfCode; ++i)
					_byteArray[(offset / 8) + i] = sourceBytes[i];
				currentOffset += byteLengthOfCode * 8;
			}

			while (currentOffset < offset + code.Length)
			{
				this[currentOffset] = code[currentOffset - offset];
				++currentOffset;
			}
		}

		/// <summary>
		/// Builds the code.
		/// </summary>
		/// <remarks>
		/// The code does not share storage with this builder, so building again after further
		/// changes yields a new code and leaves the earlier one alone.
		/// </remarks>
		public Code Build()
		{
			return new Code(_byteArray, _length);
		}
		#endregion

		#region private functions
		private static byte GetMaskAt(int offset)
		{
			return (byte) (1 << (offset % 8));
		}
		#endregion
	}
}
