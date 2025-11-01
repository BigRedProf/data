using System;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;

namespace BigRedProf.Data.Tape
{
	public interface ISeriesDigestEngine
	{
		MultihashAlgorithm Algorithm
		{
			get;
		}

		Multihash ComputeBaseline();

		Multihash ComputeContentDigest(Tape tape);

		Multihash ComputeSeriesHeadDigest(Multihash parent, Multihash content);

		Multihash FromCode(Code code);
	}

	public sealed class SeriesDigestEngine : ISeriesDigestEngine
	{
		#region fields
		private readonly MultihashAlgorithm _algorithm;
		private readonly Multihash _baseline;
		#endregion

		#region constructors
		public SeriesDigestEngine(MultihashAlgorithm algorithm = MultihashAlgorithm.SHA2_256)
		{
			_algorithm = algorithm;
			_baseline = CreateBaseline(_algorithm);
		}
		#endregion

		#region properties
		public MultihashAlgorithm Algorithm
		{
			get
			{
				return _algorithm;
			}
		}
		#endregion

		#region methods
		public Multihash ComputeBaseline()
		{
			return _baseline;
		}

		public Multihash ComputeContentDigest(Tape tape)
		{
			if (tape == null)
				throw new ArgumentNullException(nameof(tape));

			Multihash result;
			int contentLengthBits = tape.Position;
			if (contentLengthBits <= 0)
			{
				result = _baseline;
			}
			else
			{
				Code content = TapeHelper.ReadContent(tape, 0, contentLengthBits);
				result = FromCode(content);
			}

			return result;
		}

		public Multihash ComputeSeriesHeadDigest(Multihash parent, Multihash content)
		{
			if (parent == null)
				throw new ArgumentNullException(nameof(parent));
			if (content == null)
				throw new ArgumentNullException(nameof(content));

			if (parent.Algorithm != _algorithm)
				throw new InvalidOperationException("Parent digest algorithm does not match the engine algorithm.");
			if (content.Algorithm != _algorithm)
				throw new InvalidOperationException("Content digest algorithm does not match the engine algorithm.");

			byte[] parentBytes = parent.Digest;
			byte[] contentBytes = content.Digest;
			byte[] combined = new byte[parentBytes.Length + contentBytes.Length];
			Array.Copy(parentBytes, 0, combined, 0, parentBytes.Length);
			Array.Copy(contentBytes, 0, combined, parentBytes.Length, contentBytes.Length);
			Code combinedCode = new Code(combined);
			Multihash result = FromCode(combinedCode);
			return result;
		}

		public Multihash FromCode(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			Multihash result = Multihash.FromCode(code, _algorithm);
			return result;
		}
		#endregion

		#region private methods
		private static Multihash CreateBaseline(MultihashAlgorithm algorithm)
		{
			Code zeroCode = new Code(new byte[1], 8);
			Multihash baseline = Multihash.FromCode(zeroCode, algorithm);
			return baseline;
		}
		#endregion
	}
}
