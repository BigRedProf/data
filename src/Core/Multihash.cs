using System;
using System.Linq;
using System.Security.Cryptography;

namespace BigRedProf.Data.Core
{
	public class Multihash
	{
		#region static fields
		private static readonly IPiedPiper _piedPiper = CreatePiedPiper();
		#endregion

		#region fields
		private readonly byte[] _digest;
		private readonly MultihashAlgorithm _algorithm;
		#endregion

		#region constructors
		internal Multihash(byte[] digest, MultihashAlgorithm algorithm)
		{
			_digest = digest ?? throw new ArgumentNullException(nameof(digest));
			_algorithm = algorithm;
		}
		#endregion

		#region properties
		public byte[] Digest => (byte[])_digest.Clone();
		public MultihashAlgorithm Algorithm => _algorithm;
		#endregion

		#region object methdso
		public override bool Equals(object obj) =>
			obj is Multihash other &&
			Algorithm == other.Algorithm &&
			_digest.SequenceEqual(other._digest);

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

		#region functions
		public static Multihash FromCode(Code code, MultihashAlgorithm algorithm)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			byte[] packedBytes = _piedPiper.SaveCodeToByteArray(code);

			byte[] digest = HashBytes(packedBytes, algorithm);
			return new Multihash(digest, algorithm);
		}

		private static byte[] HashBytes(byte[] bytes, MultihashAlgorithm algorithm)
		{
			switch (algorithm)
			{
				case MultihashAlgorithm.SHA2_256:
					// SHA256.HashData is not available in .NET Standard 2.0 or older;
					// fall back to instance method.
					using (var sha256 = SHA256.Create())
						return sha256.ComputeHash(bytes);

				default:
					throw new NotImplementedException($"Algorithm '{algorithm}' is not implemented.");
			}
		}
		#endregion

		#region private functions
		private static IPiedPiper CreatePiedPiper()
		{
			var piper = new PiedPiper();
			piper.RegisterCorePackRats();
			return piper;
		}

		private static char GetHexChar(int val)
		{
			return (char)(val < 10 ? '0' + val : 'a' + (val - 10));
		}
		#endregion
	}
}
