using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BigRedProf.Data.Tape.Providers.Memory
{
	public class MemoryTapeProvider : TapeProvider
	{
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

				byte[] tapeData = GetTapeData(tapeId);
				var resultBytes = new byte[byteLength];
				Array.Copy(tapeData, byteOffset, resultBytes, 0, byteLength);
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

				byte[] tapeData = GetTapeData(tapeId);

				if (byteOffset + byteLength > tapeData.Length)
					throw new ArgumentOutOfRangeException(nameof(byteLength), "Invalid byte length specified.");

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

				// TODO: Consider making tape length grow dynamically so it's not always 125MB long.
				_tapes[tape.Id] = new byte[Code.MaxLength];
			}
		#endregion

		#region private methods
			private byte[] GetTapeData(Guid tapeId)
			{
				return _tapes[tapeId];
			}

			private byte[] SetTapeData(Guid tapeId, byte[] data)
			{
				_tapes[tapeId] = data;
				return data;
			}
		#endregion
	}
}
