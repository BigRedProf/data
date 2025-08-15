using System;
using System.Diagnostics;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Represents a compact code (KCode) intended for use as tokens, digests, or keys, where 1,000 bits or fewer is sufficient.
	/// Uses the same schema and serialization as <see cref="Code"/> (CodePackRat).
	/// </summary>
	public class KCode
	{
		#region constants
		/// <summary>
		/// The maximum length of a KCode, 1,000 bits.
		/// </summary>
		public const int MaxLength = 1000;
		#endregion

		#region fields
		private readonly Code _code;
		#endregion

		#region properties
		public int Length
		{
			get { return _code.Length; }
		}
		#endregion

		#region constructors
		public KCode(int length)
		{
			if (length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(length), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(length);
		}

		public KCode(params Bit[] bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			if (bits.Length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(bits), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(bits);
		}

		public KCode(string bits)
		{
			if (bits == null)
				throw new ArgumentNullException(nameof(bits));

			int bitCount = 0;
			foreach (char c in bits)
			{
				if (c == '0' || c == '1')
					++bitCount;
			}
			if (bitCount > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(bits), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(bits);
		}

		public KCode(byte[] byteArray, int length)
		{
			if (length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(length), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(byteArray, length);
		}

		public KCode(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException(nameof(byteArray));

			int length = byteArray.Length * 8;
			if (length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(byteArray), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(byteArray);
		}

		public KCode(byte[] byteArray, int length, byte lastByte)
		{
			if (length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(length), $"A KCode cannot exceed {MaxLength} bits in length.");

			_code = new Code(byteArray, length, lastByte);
		}
		#endregion

		#region Indexers
		public Bit this[int offset]
		{
			get { return _code[offset]; }
			set { _code[offset] = value; }
		}

		public Code this[int offset, int length]
		{
			get { return _code[offset, length]; }
			set { _code[offset, length] = value; }
		}
		#endregion

		#region methods
		public byte[] ToByteArray()
		{
			return _code.ToByteArray();
		}
		#endregion

		#region object methods
		public override string ToString()
		{
			return _code.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj is KCode other)
				return _code.Equals(other._code);

			if (obj is Code code)
				return _code.Equals(code);

			return false;
		}
		public override int GetHashCode()
		{
			return _code.GetHashCode();
		}
		#endregion

		#region operator overloads
		public static bool operator ==(KCode left, KCode right)
		{
			if (ReferenceEquals(left, right))
				return true;

			if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
				return false;

			return left._code == right._code;
		}

		public static bool operator !=(KCode left, KCode right)
		{
			return !(left == right);
		}
		#endregion

		#region Casts
		public static implicit operator Code(KCode kcode)
		{
			return kcode?._code;
		}

		public static explicit operator KCode(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			if (code.Length > MaxLength)
				throw new ArgumentOutOfRangeException(nameof(code), $"A KCode cannot exceed {MaxLength} bits in length.");

			return new KCode(code.ToString());
		}
		#endregion
	}
}
