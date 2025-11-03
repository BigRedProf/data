using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BigRedProf.Data.Core
{
	public enum MultibaseEncoding
	{
		Base32Lower,
		HexLower
	}

	[DebuggerDisplay("{_algorithm}:{DigestLength}b")]
	public sealed class Multihash : IEquatable<Multihash>
	{
		#region static fields
		private static readonly Dictionary<MultihashAlgorithm, uint> _algoToCode = new Dictionary<MultihashAlgorithm, uint>
		{
			{ MultihashAlgorithm.Sha256, Sha256Code }
		};

		private static readonly Dictionary<uint, MultihashAlgorithm> _codeToAlgo = new Dictionary<uint, MultihashAlgorithm>
		{
			{ Sha256Code, MultihashAlgorithm.Sha256 }
		};
		#endregion

		#region fields
		private readonly byte[] _digest;
		private readonly MultihashAlgorithm _algorithm;
		#endregion

		#region constructors
		internal Multihash(byte[] digest, MultihashAlgorithm algorithm)
		{
			if (digest == null)
				throw new ArgumentNullException(nameof(digest));

			// defensive copy so callers can't mutate our internal state
			_digest = (byte[])digest.Clone();

			_algorithm = algorithm;
		}
		#endregion

		#region properties
		public byte[] Digest => (byte[])_digest.Clone();
		public int DigestLength => _digest.Length;
		public MultihashAlgorithm Algorithm => _algorithm;
		#endregion

		#region methods
		public string ToMultibaseString(MultibaseEncoding encoding = MultibaseEncoding.Base32Lower)
		{
			byte[] binary = BuildBinaryMultihash(_algorithm, _digest);
			string payload;
			string result;

			switch (encoding)
			{
				case MultibaseEncoding.Base32Lower:
					payload = Base32EncodeLower(binary);
					result = "b" + payload;
					break;
				case MultibaseEncoding.HexLower:
					payload = ToHex(binary);
					result = "f" + payload;
					break;
				default:
					throw new NotSupportedException(string.Format("Encoding '{0}' is not supported.", encoding));
			}

			return result;
		}
		#endregion

		#region object methods
		public override bool Equals(object obj)
		{
			return Equals(obj as Multihash);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + _algorithm.GetHashCode();
				foreach (byte b in _digest)
					hash = hash * 31 + b;
				return hash;
			}
		}

		public override string ToString()
		{
			return $"{_algorithm}:{ToHex(_digest)}";
		}

		private static string ToHex(byte[] bytes)
		{
			char[] c = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				byte b = bytes[i];
				c[i * 2] = GetHexChar(b >> 4);
				c[i * 2 + 1] = GetHexChar(b & 0xF);
			}
			return new string(c);
		}
		#endregion

		#region IEquatable<Multihash> methods
		public bool Equals(Multihash other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _algorithm == other._algorithm && _digest.SequenceEqual(other._digest);
		}
		#endregion

		#region operator overloads
		public static bool operator ==(Multihash a, Multihash b)
		{
			if (ReferenceEquals(a, b))
				return true;

			if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
				return false;

			return a.Equals(b);
		}

		public static bool operator !=(Multihash a, Multihash b)
		{
			return !(a == b);
		}
		#endregion

		#region functions
		public static Multihash FromCode(Code code, MultihashAlgorithm algorithm)
		{
			// For byte-aligned codes, consider the hash of the raw bytes to be the hash. This is
			// both elegant and efficient.
			//
			// For non-byte-aligned codes, we need to make sure to disambiguate codes with
			// the same raw bytes by including the length but also make sure collisions aren't
			// easy to manufacture. One way to do this is to hash the hash of the raw bytes
			// with the hash of the length.

			if (code == null)
				throw new ArgumentNullException(nameof(code));

			// Hash of raw packed bytes
			byte[] rawBytes = code.ToByteArray();
			byte[] digest = HashBytes(rawBytes, algorithm);

			// Byte-aligned: H(rawBytes)
			if (code.Length % 8 == 0)
				return new Multihash(digest, algorithm);

			// Non-byte-aligned: H( H(rawBytes) || H(length_be_64) )
			byte[] lengthBe = UInt64ToBigEndian((ulong)code.Length);
			byte[] lengthDigest = HashBytes(lengthBe, algorithm);

			return Combine(digest, lengthDigest, algorithm);
		}

		public static Multihash FromBytes(byte[] bytes, MultihashAlgorithm algorithm)
		{
			if (bytes == null)
				throw new ArgumentNullException(nameof(bytes));

			byte[] digest = HashBytes(bytes, algorithm);
			Multihash result = new Multihash(digest, algorithm);
			return result;
		}

		public static Multihash Combine(byte[] bytes1, byte[] bytes2, MultihashAlgorithm algorithm)
		{
			if (bytes1 == null)
				throw new ArgumentNullException(nameof(bytes1));

			if (bytes2 == null)
				throw new ArgumentNullException(nameof(bytes2));

			byte[] combined = new byte[bytes1.Length + bytes2.Length];
			Buffer.BlockCopy(bytes1, 0, combined, 0, bytes1.Length);
			Buffer.BlockCopy(bytes2, 0, combined, bytes1.Length, bytes2.Length);

			byte[] digest = HashBytes(combined, algorithm);
			Multihash result = new Multihash(digest, algorithm);

			return result;
		}

		public static Multihash Parse(string text)
		{
			if (text == null)
				throw new ArgumentNullException(nameof(text));

			if (text.Length < 2)
				throw new FormatException("Multibase text must include a prefix and payload.");

			char prefix = text[0];
			string payload = text.Substring(1);
			byte[] binary;

			switch (prefix)
			{
				case 'b':
					binary = Base32DecodeLower(payload);
					break;
				case 'f':
					binary = FromHex(payload);
					break;
				default:
					throw new FormatException(string.Format("Unsupported multibase prefix '{0}'.", prefix));
			}

			if (binary.Length == 0)
				throw new FormatException("Binary multihash payload is empty.");

			int offset = 0;
			uint codeValue;
			bool codeRead = TryReadUVarInt(binary, ref offset, out codeValue);
			if (!codeRead)
				throw new FormatException("Unable to read multihash algorithm code.");

			uint digestLengthValue;
			bool lengthRead = TryReadUVarInt(binary, ref offset, out digestLengthValue);
			if (!lengthRead)
				throw new FormatException("Unable to read multihash digest length.");

			MultihashAlgorithm algorithm;
			if (!_codeToAlgo.TryGetValue(codeValue, out algorithm))
				throw new FormatException(string.Format("Unsupported multihash algorithm code '0x{0:X}'.", codeValue));

			if (digestLengthValue > int.MaxValue)
				throw new FormatException("Digest length exceeds maximum supported size.");

			int digestLength = (int)digestLengthValue;
			if (algorithm == MultihashAlgorithm.Sha256 && digestLength != Sha256DigestLength)
				throw new FormatException("Digest length does not match SHA-256 requirements.");

			if (binary.Length - offset != digestLength)
				throw new FormatException("Digest length does not match payload size.");

			byte[] digest = new byte[digestLength];
			Buffer.BlockCopy(binary, offset, digest, 0, digestLength);

			Multihash result = new Multihash(digest, algorithm);
			return result;
		}

		public static bool TryParse(string text, out Multihash value)
		{
			if (text == null)
			{
				value = null;
				return false;
			}

			try
			{
				value = Parse(text);
				return true;
			}
			catch (FormatException)
			{
				value = null;
				return false;
			}
			catch (NotSupportedException)
			{
				value = null;
				return false;
			}
		}

		private static byte[] HashBytes(byte[] bytes, MultihashAlgorithm algorithm)
		{
			switch (algorithm)
			{
				case MultihashAlgorithm.Sha256:
					using (SHA256 sha256 = SHA256.Create())
						return sha256.ComputeHash(bytes);

				default:
					throw new NotImplementedException($"Algorithm '{algorithm}' is not implemented.");
			}
		}

		private static byte[] UInt64ToBigEndian(ulong value)
		{
			// This method is a handy alternative to BitConverter.GetBytes because BitConverter
			// returns little-endian byte arrays on little-endian systems but big-endian on 
			// big-endian systems. We need a consistent representation across all systems.

			byte[] bytes = new byte[8];
			for (int i = 7; i >= 0; i--)
			{
				bytes[i] = (byte)(value & 0xFF);
				value >>= 8;
			}
			return bytes;
		}
		#endregion

		#region private functions
		private static char GetHexChar(int val)
		{
			return (char)(val < 10 ? '0' + val : 'a' + (val - 10));
		}

		private static byte[] BuildBinaryMultihash(MultihashAlgorithm algorithm, byte[] digest)
		{
			Debug.Assert(digest != null, "digest must not be null.");

			uint code;
			if (!_algoToCode.TryGetValue(algorithm, out code))
				throw new NotSupportedException(string.Format("Algorithm '{0}' is not supported for multibase encoding.", algorithm));

			if (algorithm == MultihashAlgorithm.Sha256 && digest.Length != Sha256DigestLength)
				throw new InvalidOperationException("Digest length does not match SHA-256 requirements.");

			byte[] codeBytes = WriteUVarInt(code);
			byte[] lengthBytes = WriteUVarInt((uint)digest.Length);
			byte[] result = new byte[codeBytes.Length + lengthBytes.Length + digest.Length];

			Buffer.BlockCopy(codeBytes, 0, result, 0, codeBytes.Length);
			Buffer.BlockCopy(lengthBytes, 0, result, codeBytes.Length, lengthBytes.Length);
			Buffer.BlockCopy(digest, 0, result, codeBytes.Length + lengthBytes.Length, digest.Length);

			return result;
		}

		private static byte[] WriteUVarInt(uint value)
		{
			byte[] buffer = new byte[5];
			int index = 0;

			do
			{
				byte next = (byte)(value & 0x7FU);
				value >>= 7;
				if (value != 0)
					next |= 0x80;

				buffer[index] = next;
				index++;
			}
			while (value != 0 && index < buffer.Length);

			byte[] result = new byte[index];
			Buffer.BlockCopy(buffer, 0, result, 0, index);
			return result;
		}

		private static bool TryReadUVarInt(byte[] data, ref int offset, out uint value)
		{
			Debug.Assert(data != null, "data must not be null.");

			value = 0;
			int shift = 0;

			for (int i = 0; i < 5; i++)
			{
				if (offset >= data.Length)
					return false;

				byte current = data[offset];
				offset++;

				value |= (uint)(current & 0x7F) << shift;

				if ((current & 0x80) == 0)
					return true;

				shift += 7;
			}

			return false;
		}

		private static string Base32EncodeLower(byte[] bytes)
		{
			Debug.Assert(bytes != null, "bytes must not be null.");

			if (bytes.Length == 0)
				return string.Empty;

			StringBuilder builder = new StringBuilder((bytes.Length * 8 + 4) / 5);
			int buffer = bytes[0];
			int next = 1;
			int bitsLeft = 8;

			while (bitsLeft > 0 || next < bytes.Length)
			{
				if (bitsLeft < 5)
				{
					if (next < bytes.Length)
					{
						buffer = (buffer << 8) | bytes[next];
						next++;
						bitsLeft += 8;
					}
					else
					{
						int indexValue = (buffer << (5 - bitsLeft)) & 0x1F;
						builder.Append(Base32AlphabetLower[indexValue]);
						bitsLeft = 0;
						continue;
					}
				}

				int outputIndex = (buffer >> (bitsLeft - 5)) & 0x1F;
				builder.Append(Base32AlphabetLower[outputIndex]);
				bitsLeft -= 5;
			}

			return builder.ToString();
		}

		private static byte[] Base32DecodeLower(string text)
		{
			if (text == null)
				throw new FormatException("Base32 text cannot be null.");

			if (text.Length == 0)
				return Array.Empty<byte>();

			List<byte> result = new List<byte>((text.Length * 5) / 8);
			int buffer = 0;
			int bitsLeft = 0;

			for (int i = 0; i < text.Length; i++)
			{
				char character = text[i];
				int value = Base32AlphabetLower.IndexOf(character);
				if (value < 0)
					throw new FormatException(string.Format("Invalid base32 character '{0}'.", character));

				buffer = (buffer << 5) | value;
				bitsLeft += 5;

				if (bitsLeft >= 8)
				{
					bitsLeft -= 8;
					byte nextByte = (byte)((buffer >> bitsLeft) & 0xFF);
					result.Add(nextByte);
				}
			}

			if (bitsLeft > 0)
			{
				int mask = (1 << bitsLeft) - 1;
				if ((buffer & mask) != 0)
					throw new FormatException("Invalid base32 padding.");
			}

			return result.ToArray();
		}

		private static byte[] FromHex(string text)
		{
			if (text == null)
				throw new FormatException("Hex text cannot be null.");

			if (text.Length == 0)
				return Array.Empty<byte>();

			if ((text.Length & 1) != 0)
				throw new FormatException("Hex text must have an even length.");

			byte[] result = new byte[text.Length / 2];

			for (int i = 0; i < result.Length; i++)
			{
				int high = GetHexValue(text[i * 2]);
				int low = GetHexValue(text[i * 2 + 1]);
				result[i] = (byte)((high << 4) | low);
			}

			return result;
		}

		private static int GetHexValue(char c)
		{
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			throw new FormatException(string.Format("Invalid hex character '{0}'.", c));
		}
		#endregion

		#region constants
		private const uint Sha256Code = 0x12;
		private const int Sha256DigestLength = 32;
		private const string Base32AlphabetLower = "abcdefghijklmnopqrstuvwxyz234567";
		#endregion
	}
}
