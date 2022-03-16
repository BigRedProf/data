using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// A <see cref="Code"/> is an ordered set of bits. The caller is responsible for encoding models into codes, decoding codes into models
	/// and defining the meaning of codes it uses.
	/// </summary>
	public class Code
	{
		#region fields
		private byte[] _byteArray;
		private int _length;
		#endregion

		#region constructors
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

		public Code(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException(nameof(byteArray));

			_byteArray = byteArray;
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
		/// Gets or sets the value of a specific <see cref="Bit"/> within the code.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public Bit this[int offset]
		{
			get
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				int offsetIntoCurrentByte = GetOffsetIntoByteArray(offset);
				int mask = GetMaskForCurrentByte(offset);
				return (_byteArray[offsetIntoCurrentByte] & mask) == 0 ? 0 : 1;
			}
			set
			{
				if (offset < 0 || offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				int offsetIntoCurrentByte = GetOffsetIntoByteArray(offset);
				if(value == 1)
					_byteArray[offsetIntoCurrentByte] |= GetMaskForCurrentByte(offset);
				else
					_byteArray[offsetIntoCurrentByte] &= GetInvertedMaskForCurrentByte(offset);
			}
		}
		#endregion

		#region object methods
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(Length);
			for (int i = 0; i < Length; ++i)
				stringBuilder.Append(this[i].ToString());

			return stringBuilder.ToString();
		}
		#endregion

		#region private methods
		private int GetOffsetIntoByteArray(int bitOffset)
		{
			return bitOffset / 8;
		}

		private byte GetMaskForCurrentByte(int bitOffset)
		{
			return (byte) (1 << (bitOffset % 8));
		}

		private byte GetInvertedMaskForCurrentByte(int bitOffset)
		{
			return (byte)(~GetMaskForCurrentByte(bitOffset));
		}
		#endregion
	}
}
