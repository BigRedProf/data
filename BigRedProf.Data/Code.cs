using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// A <see cref="Code"/> is an ordered set of bits. The caller is responsible for encoding models into codes, decoding codes into models
	/// and defining the meaning of codes it uses.
	/// </summary>
	public class Code : IEnumerable<Bit>
	{
		#region fields
		private byte[] _byteArray;
		private int _length;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new zeroed-out <see cref="Code"/> of the specified length.
		/// </summary>
		/// <param name="length">The length of code to create.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public Code(int length)
		{
			if(length <= 0)
				throw new ArgumentOutOfRangeException(nameof(length));

			_byteArray = new byte[(length / 8) + ((length % 8) > 0 ? 1 : 0)];
			_length = length;
		}

		/// <summary>
		/// Creates a new <see cref="Code"/> from the specified bits.
		/// </summary>
		/// <param name="bits">The bits that comprise the code.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public Code(params Bit[] bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			if (bits.Length == 0)
				throw new ArgumentException("A code must contain at least one bit.", nameof(bits));

			_byteArray = new byte[((bits.Length - 1) / 8) + 1];
			_length = bits.Length;

			// TODO: replace this with a bit stream writer when we have one; will be faster
			for (int i = 0; i < bits.Length; ++i)
				this[i] = bits[i];
		}

		/// <summary>
		/// Creates a <see cref="Code"/> from a string of bits.
		/// </summary>
		/// <param name="bits">The bits, as '0' and '1' characters, that comprise the code.</param>
		public Code(string bits)
			: this(ConvertStringToBitArray(bits))
		{
		}

		/// <summary>
		/// Creates a new <see cref="Code"/> from the specified array of bytes.
		/// </summary>
		/// <param name="byteArray">The byte array that comprises the code.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Code(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException(nameof(byteArray));

			_byteArray = byteArray;
			_length = byteArray.Length * 8;
		}
		#endregion

		#region properties
		/// <summary>
		/// The length, in bits, of the code.
		/// </summary>
		public int Length
		{
			get
			{
				return _length;
			}
		}

		/// <summary>
		/// Exposes the underlying byte array, internally, to make <see cref="CodeWriter"/> more efficient.
		/// </summary>
		internal byte[] ByteArray
		{
			get
			{
				return _byteArray;
			}
		}

		/// <summary>
		/// Gets or sets the value of a specific <see cref="Bit"/> within the code.
		/// </summary>
		/// <param name="offset">The offset into the code.</param>
		/// <returns>The bit at the specified offset.</returns>
		public Bit this[int offset]
		{
			get
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				int offsetIntoByteArray = GetByteOffsetAt(offset);
				int mask = GetMaskForByteAt(offset);
				return (_byteArray[offsetIntoByteArray] & mask) == 0 ? 0 : 1;
			}
			set
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				int offsetIntoCurrentByte = GetByteOffsetAt(offset);
				if(value == 1)
					_byteArray[offsetIntoCurrentByte] |= GetMaskForByteAt(offset);
				else
					_byteArray[offsetIntoCurrentByte] &= GetInvertedMaskForByteAt(offset);
			}
		}

		/// <summary>
		/// Gets or sets the value of a specific range of bits within the code.
		/// </summary>
		/// <param name="offset">The offset into the code.</param>
		/// <param name="length">The length of code to return.</param>
		/// <returns>The code at the specified offset.</returns>
		public Code this[int offset, int length]
		{
			get
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				if (length == 0 || offset + length > Length)
					throw new ArgumentOutOfRangeException(nameof(length));

				Code code = new Code(length);

				// do what we can quickly with byte-by-byte copies
				int currentOffset = offset;
				if((offset % 8) == 0 && length >= 8)
				{
					int offsetIntoByteArray = GetByteOffsetAt(offset);
					int byteLengthOfCode = (length % 8);
					for (int i = 0; i < byteLengthOfCode; ++i)
						code.ByteArray[i] = _byteArray[offsetIntoByteArray + i];
					currentOffset += byteLengthOfCode * 8;
				}

				// do the remainder bit-by-bit
				while (currentOffset < offset + length)
				{
					code[currentOffset - offset] = this[currentOffset];
					++currentOffset;
				}

				return code;
			}
			set
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				if (length == 0 || offset + length > Length)
					throw new ArgumentOutOfRangeException(nameof(length));

				// do what we can quickly with byte-by-byte copies
				int currentOffset = offset;
				if ((offset % 8) == 0 && length >= 8)
				{
					int offsetIntoByteArray = GetByteOffsetAt(offset);
					int byteLengthOfCode = (length % 8);
					for (int i = 0; i < byteLengthOfCode; ++i)
						_byteArray[offsetIntoByteArray + i] = value.ByteArray[i];
					currentOffset += byteLengthOfCode * 8;
				}

				// do the remainder bit-by-bit
				while (currentOffset < offset + length)
				{
					this[currentOffset] = value[currentOffset - offset];
					++currentOffset;
				}
			}
		}
		#endregion

		#region object methods
		public override bool Equals(object obj)
		{
			Code that = obj as Code;
			if(that == null)
				return false;

			if (this.Length != that.Length)
				return false;

			int byteArrayLength = this.ByteArray.Length;
			for(int i = 0; i < byteArrayLength; ++i)
			{
				if (this.ByteArray[i] != that.ByteArray[i])
					return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return (this.ByteArray.GetHashCode() ^ this.Length);
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(Length);
			for (int i = 0; i < Length; ++i)
				stringBuilder.Append(this[i].ToString());

			return stringBuilder.ToString();
		}
		#endregion

		#region IEnumerable<Bit>
		/// <inheritdoc/>
		public IEnumerator<Bit> GetEnumerator()
		{
			return new CodeEnumerator(this);
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region operator overloads
		public static bool operator ==(Code left, Code right)
		{
			if (object.ReferenceEquals(left, right))
				return true;

			if (object.ReferenceEquals(left, null))
				return false;

			if (object.ReferenceEquals(right, null))
				return false;

			return left.Equals(right);
		}

		public static bool operator !=(Code left, Code right)
		{
			return !(left == right);
		}
		#endregion

		#region casts
		public static implicit operator string(Code code)
		{
			return code.ToString();
		}

		public static implicit operator Code(string @string)
		{
			return new Code(@string);
		}
		#endregion


		#region private static methods
		private static int GetByteOffsetAt(int bitOffset)
		{
			return bitOffset / 8;
		}

		private static int GetBitOffsetAt(int bitOffset)
		{
			return bitOffset % 8;
		}

		private static byte GetMaskForByteAt(int bitOffset)
		{
			return (byte) (1 << (bitOffset % 8));
		}

		private static byte GetInvertedMaskForByteAt(int bitOffset)
		{
			return (byte)(~GetMaskForByteAt(bitOffset));
		}

		private static Bit[] ConvertStringToBitArray(string bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			List<Bit> bitList = new List<Bit>(bits.Length);
			foreach (char c in bits)
			{
				if (c == '0')
					bitList.Add(0);
				else if (c == '1')
					bitList.Add(1);
				else if (char.IsWhiteSpace(c))
					continue;
				else
					throw new ArgumentException($"Illegal character '{c}' in code.", nameof(bits));
			}

			if (bitList.Count == 0)
				throw new ArgumentException("A code must contain at least one bit.", nameof(bits));

			return bitList.ToArray();
		}
		#endregion
	}
}
