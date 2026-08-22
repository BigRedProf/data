using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A <see cref="Code"/> is an ordered set of bits. The caller is responsible for encoding models into codes, decoding codes into models
	/// and defining the meaning of codes it uses.
	/// </summary>
	public class Code : IEnumerable<Bit>
	{
		#region constants
		/// <summary>
		/// The maximum length of a code, 1 gigabit.
		/// </summary>
		public const int MaxLength = 1000 * 1000 * 1000;
		#endregion

		#region fields
		private byte[] _byteArray;
		private int _length;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new zeroed-out <see cref="Code"/> of the specified length.
		/// </summary>
		/// <param name="length">The length of code, in bits.</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public Code(int length)
		{
			if(length <= 0)
				throw new ArgumentOutOfRangeException(nameof(length), "A code must be at least 1 bit long.");

			if(length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(length), "A code cannot exceed 1 gigabit in length.");

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
			: this(bits.Length)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			// TODO: replace this with a bit stream writer when we have one; will be faster
			for (int i = 0; i < bits.Length; ++i)
				SetBit(i, bits[i]);
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
		/// <param name="length">The length of code, in bits.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Code(byte[] byteArray, int length)
			: this(length)
		{
			if (byteArray == null)
				throw new ArgumentNullException(nameof(byteArray));

			int requiredByteCount = (length + 7) / 8;
			if (byteArray.Length < requiredByteCount)
			{
				throw new ArgumentOutOfRangeException(
					nameof(length),
					"The specified length is too small to accomodate the byte array."
				);
			}

			Debug.Assert(_byteArray.Length <= byteArray.Length);
			Array.Copy(byteArray, 0, _byteArray, 0, _byteArray.Length);
			ZeroUnusedBitsInLastByte();
		}

		/// <summary>
		/// Creates a new <see cref="Code"/> from the specified array of bytes.
		/// </summary>
		/// <param name="byteArray">The byte array that comprises the code.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Code(byte[] byteArray)
			: this(byteArray, byteArray.Length * 8)
		{
		}

		/// <summary>
		/// Creates a new <see cref="Code"/> from the specified array of bytes.
		/// </summary>
		/// <param name="byteArray">The byte array that comprises the code.</param>
		/// <param name="length">The length of code, in bits.</param>
		/// <param name="lastByte">The last byte which will be partly used based on length.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public Code(byte[] byteArray, int length, byte lastByte)
			: this(byteArray, length)
		{
			_byteArray[_byteArray.Length - 1] = lastByte;
			ZeroUnusedBitsInLastByte();
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
		/// Gets the value of a specific <see cref="Bit"/> within the code.
		/// </summary>
		/// <remarks>
		/// Read-only. A code is a value, so it cannot be changed once it exists; build one with
		/// <see cref="CodeBuilder"/> instead.
		/// </remarks>
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
		}

		/// <summary>
		/// Gets the value of a specific range of bits within the code.
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

				CodeBuilder builder = new CodeBuilder(length);

				// do what we can quickly with byte-by-byte copies
				int currentOffset = offset;
				if ((offset % 8) == 0 && length >= 8)
				{
					int offsetIntoByteArray = GetByteOffsetAt(offset);
					int byteLengthOfCode = (length / 8);
					byte[] bytes = new byte[byteLengthOfCode];
					for (int i = 0; i < byteLengthOfCode; ++i)
						bytes[i] = _byteArray[offsetIntoByteArray + i];

					Code head = new Code(bytes, byteLengthOfCode * 8);
					currentOffset += byteLengthOfCode * 8;
					if (currentOffset == offset + length)
						return head;

					for (int i = 0; i < head.Length; ++i)
						builder[i] = head[i];
				}

				// do the remainder bit-by-bit
				while (currentOffset < offset + length)
				{
					builder[currentOffset - offset] = this[currentOffset];
					++currentOffset;
				}

				return builder.Build();
			}
		}

		#endregion

		#region methods
		/// <summary>
		/// The number of bytes the code occupies.
		/// </summary>
		public int ByteLength => _byteArray.Length;

		/// <summary>
		/// Returns a read-only view over the code's bytes.
		/// </summary>
		/// <remarks>
		/// No copy. A code is immutable, so a read-only view over its storage is safe to hand out,
		/// and a code can be a gigabit long -- <see cref="ToByteArray"/> is not always a reasonable
		/// way to read one.
		///
		/// The view covers whole bytes. When <see cref="Length"/> is not a multiple of eight, the
		/// unused high bits of the final byte are zero, so two codes of equal length always have
		/// identical views.
		/// </remarks>
		public ReadOnlySpan<byte> AsSpan()
		{
			return new ReadOnlySpan<byte>(_byteArray);
		}

		/// <summary>
		/// Returns a read-only view over the code's bytes that can be stored and passed around.
		/// </summary>
		/// <remarks>
		/// As <see cref="AsSpan"/>, for callers that need a view outliving a single stack frame --
		/// a field, an async method, or an iterator.
		/// </remarks>
		public ReadOnlyMemory<byte> AsMemory()
		{
			return new ReadOnlyMemory<byte>(_byteArray);
		}

		/// <summary>
		/// Returns a single byte of the code.
		/// </summary>
		/// <remarks>
		/// For readers that want the bytes without a copy of the whole code. A code can be a
		/// gigabit long, so <see cref="ToByteArray"/> is not always a reasonable way to read one.
		/// </remarks>
		/// <param name="index">The byte index.</param>
		public byte GetByte(int index)
		{
			if (index < 0 || index >= _byteArray.Length)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _byteArray[index];
		}

		/// <summary>
		/// Copies part of the code into an existing buffer.
		/// </summary>
		/// <remarks>
		/// The way to move a large code somewhere without allocating a second copy of it first.
		/// </remarks>
		/// <param name="destination">The buffer to copy into.</param>
		/// <param name="destinationIndex">Where in the buffer to start writing.</param>
		/// <param name="sourceByteIndex">Which byte of the code to start from.</param>
		/// <param name="byteCount">How many bytes to copy.</param>
		public void CopyTo(byte[] destination, int destinationIndex, int sourceByteIndex, int byteCount)
		{
			if (destination == null)
				throw new ArgumentNullException(nameof(destination));

			if (sourceByteIndex < 0 || sourceByteIndex + byteCount > _byteArray.Length)
				throw new ArgumentOutOfRangeException(nameof(sourceByteIndex));

			if (destinationIndex < 0 || destinationIndex + byteCount > destination.Length)
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			Array.Copy(_byteArray, sourceByteIndex, destination, destinationIndex, byteCount);
		}

		/// <summary>
		/// Returns the code's bits as a byte array.
		/// </summary>
		/// <remarks>
		/// A copy. A code is immutable, so handing out its backing array would let any caller
		/// change a value that other code has already compared, fingerprinted, or used as a
		/// dictionary key. Callers inside this assembly that only read can use
		/// <see cref="ByteArray"/> and skip the copy.
		/// </remarks>
		public byte[] ToByteArray()
		{
			byte[] byteArray = new byte[_byteArray.Length];
			Array.Copy(_byteArray, 0, byteArray, 0, _byteArray.Length);

			return byteArray;
		}
		#endregion

		#region private methods
		/// <summary>
		/// Zeroes out the unused high bits of the last byte so that codes of the same bit
		/// content are always byte-for-byte identical, regardless of any garbage bits in
		/// the source byte array. This canonical form is required for <see cref="Equals(object)"/>
		/// and <see cref="Multihash.FromCode(Code, MultihashAlgorithm)"/> to behave correctly.
		/// </summary>
		private void ZeroUnusedBitsInLastByte()
		{
			int usedBitsInLastByte = _length % 8;
			if (usedBitsInLastByte != 0)
				_byteArray[_byteArray.Length - 1] &= (byte)((1 << usedBitsInLastByte) - 1);
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
			// The byte array is canonical (constructors zero the unused trailing bits), so
			// hashing the bytes plus the length is correct and allocation-free.
			unchecked
			{
				int hashCode = _length;
				for (int i = 0; i < _byteArray.Length; ++i)
					hashCode = (hashCode * 31) + _byteArray[i];

				return hashCode;
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(Length);
			for (int i = 0; i < Length; ++i)
			{
				if ((i % 8 == 0) && i != 0)
					stringBuilder.Append(" ");

				stringBuilder.Append(this[i].ToString());
			}

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
		/// <summary>
		/// Sets a bit. Callable only while a code is being constructed -- a code is a value and
		/// does not change afterwards.
		/// </summary>
		private void SetBit(int offset, Bit bit)
		{
			int offsetIntoCurrentByte = GetByteOffsetAt(offset);
			if (bit == 1)
				_byteArray[offsetIntoCurrentByte] |= GetMaskForByteAt(offset);
			else
				_byteArray[offsetIntoCurrentByte] &= (byte) ~GetMaskForByteAt(offset);
		}

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
