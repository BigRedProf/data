using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;
using System;

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
				private readonly Multihash _baselineDigest;
				#endregion

				#region constructors
				public SeriesDigestEngine(MultihashAlgorithm algorithm)
				{
						_algorithm = algorithm;

						byte[] zeroBytes = new byte[1];
						Code zeroCode = new Code(zeroBytes, 8);
						_baselineDigest = Multihash.FromCode(zeroCode, _algorithm);
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
						return _baselineDigest;
				}

				public Multihash ComputeContentDigest(Tape tape)
				{
						if (tape == null)
								throw new ArgumentNullException(nameof(tape));

						int contentLengthBits = tape.Position;
						if (contentLengthBits <= 0)
								return _baselineDigest;

						Code content = TapeHelper.ReadContent(tape, 0, contentLengthBits);
						return FromCode(content);
				}

				public Multihash ComputeSeriesHeadDigest(Multihash parent, Multihash content)
				{
						if (parent == null)
								throw new ArgumentNullException(nameof(parent));

						if (content == null)
								throw new ArgumentNullException(nameof(content));

						if (parent.Algorithm != _algorithm || content.Algorithm != _algorithm)
								throw new InvalidOperationException("Digest algorithms must match to compute the series head digest.");

						byte[] parentBytes = parent.Digest;
						byte[] contentBytes = content.Digest;
						byte[] combined = new byte[parentBytes.Length + contentBytes.Length];
						Array.Copy(parentBytes, 0, combined, 0, parentBytes.Length);
						Array.Copy(contentBytes, 0, combined, parentBytes.Length, contentBytes.Length);

						Code combinedCode = new Code(combined);
						return FromCode(combinedCode);
				}

				public Multihash FromCode(Code code)
				{
						if (code == null)
								throw new ArgumentNullException(nameof(code));

						return Multihash.FromCode(code, _algorithm);
				}
				#endregion
		}
}

