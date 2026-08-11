using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BigRedProf.Data.Tape.Providers.Memory
{
	public class MemoryTapeProvider : TapeProvider
	{
		#region constants
			/// <summary>
			/// The largest a tape's backing array can ever be. A tape holds at most
			/// <see cref="Tape.MaxContentLength"/> BITS, so its bytes are one eighth of that.
			/// </summary>
			private const int MaxTapeByteLength = Tape.MaxContentLength / 8;

			/// <summary>
			/// The size a tape's backing array starts at, before any content is written.
			/// </summary>
			private const int InitialTapeByteLength = 4096;
		#endregion

		#region fields
			private readonly Dictionary<Guid, byte[]> _tapes;
			private readonly Dictionary<Guid, byte[]> _labels;
		#endregion

		#region constructors
			public MemoryTapeProvider()
					: this(new PiedPiper())
			{
			}

			public MemoryTapeProvider(IPiedPiper piedPiper)
					: base(piedPiper)
			{
				_tapes = new Dictionary<Guid, byte[]>();
				_labels = new Dictionary<Guid, byte[]>();
			}
		#endregion

		#region TapeProvider methods
			public override bool TryFetchTapeInternal(Guid tapeId, out Tape? tape)
			{
				if (tapeId == Guid.Empty)
					throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if (!_tapes.ContainsKey(tapeId))
				{
					tape = null;
					return false;
				}

				tape = new Tape(this, tapeId);
				return true;
			}

			public override IEnumerable<Tape> FetchAllTapesInternal()
			{
				// Return a Tape for each tapeId in _tapes
				foreach (var tapeId in _tapes.Keys)
				{
					yield return new Tape(this, tapeId);
				}
			}

			public override byte[] ReadTapeInternal(Guid tapeId, int byteOffset, int byteLength)
			{
				if (tapeId == Guid.Empty)
					throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if (byteOffset < 0)
					throw new ArgumentOutOfRangeException(nameof(byteOffset), "Byte offset cannot be negative.");

				if (byteLength < 0)
					throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length cannot be negative.");

				byte[] tapeData = GetTapeData(tapeId);
				byte[] resultBytes = new byte[byteLength];

				// The backing array only covers the region written so far, so anything past it
				// is still blank tape and reads back as zeros. DiskTapeProvider does the same
				// thing when a read runs past the end of the tape file.
				int availableByteLength = tapeData.Length - byteOffset;
				if (availableByteLength > 0)
					Array.Copy(tapeData, byteOffset, resultBytes, 0, Math.Min(availableByteLength, byteLength));

				return resultBytes;
			}

			public override byte[] ReadLabelInternal(Guid tapeId)
			{
				if (tapeId == Guid.Empty)
					throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if(!_labels.TryGetValue(tapeId, out byte[] labelData))
					throw new KeyNotFoundException($"Label for tape ID '{tapeId}' not found.");

				return labelData;
			}

			public override void WriteTapeInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength)
			{
				if (tapeId == Guid.Empty)
					throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if (data == null)
					throw new ArgumentNullException(nameof(data), "Data cannot be null.");

				if (byteOffset < 0)
					throw new ArgumentOutOfRangeException(nameof(byteOffset), "Byte offset cannot be negative.");

				if (byteLength < 0)
					throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length cannot be negative.");

				if (byteLength > data.Length)
					throw new ArgumentException("Byte length exceeds source data length.", nameof(byteLength));

				if ((long) byteOffset + byteLength > MaxTapeByteLength)
					throw new ArgumentOutOfRangeException(nameof(byteLength), "Invalid byte length specified.");

				byte[] tapeData = EnsureTapeCapacity(GetTapeData(tapeId), byteOffset + byteLength);

				Array.Copy(data, 0, tapeData, byteOffset, byteLength);
				SetTapeData(tapeId, tapeData);
			}

			public override void WriteLabelInternal(Guid tapeId, byte[] data)
			{
				if (tapeId == Guid.Empty)
					throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

				if(data == null)
					throw new ArgumentNullException(nameof(data), "Data cannot be null.");

				_labels[tapeId] = data;
			}

			public override void AddTapeInternal(Tape tape)
			{
				if (tape == null)
					throw new ArgumentNullException(nameof(tape), "Tape cannot be null.");

				// A tape starts small and grows as it is written to. Reserving MaxTapeByteLength
				// up front charged every new tape a 125MB allocation regardless of how little
				// was ever recorded on it.
				_tapes[tape.Id] = new byte[InitialTapeByteLength];
			}
		#endregion

		#region private methods
			private byte[] GetTapeData(Guid tapeId)
			{
				return _tapes[tapeId];
			}

			/// <summary>
			/// Returns a backing array at least <paramref name="requiredByteLength"/> long,
			/// doubling when it has to grow so a stream of small sequential writes costs
			/// amortized linear time rather than quadratic.
			/// </summary>
			private static byte[] EnsureTapeCapacity(byte[] tapeData, int requiredByteLength)
			{
				Debug.Assert(tapeData != null);
				Debug.Assert(requiredByteLength >= 0 && requiredByteLength <= MaxTapeByteLength);

				byte[] result = tapeData;
				if (tapeData.Length < requiredByteLength)
				{
					long doubledByteLength = (long) tapeData.Length * 2;
					int newByteLength = (int) Math.Min(
						MaxTapeByteLength,
						Math.Max(requiredByteLength, doubledByteLength)
					);

					result = new byte[newByteLength];
					Array.Copy(tapeData, 0, result, 0, tapeData.Length);
				}

				return result;
			}

			private byte[] SetTapeData(Guid tapeId, byte[] data)
			{
				_tapes[tapeId] = data;
				return data;
			}
		#endregion
	}
}
