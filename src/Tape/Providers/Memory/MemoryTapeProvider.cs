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
			throw new NotImplementedException();
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

			if (byteLength <= 0 || byteOffset + byteLength > data.Length)
				throw new ArgumentOutOfRangeException(nameof(byteLength), "Invalid byte length specified.");

			byte[] _data = GetTapeData(tapeId);
			Array.Copy(data, 0, _data, byteOffset, byteLength);
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
			
			_tapes[tape.Id] = new byte[0];
			_labels[tape.Id] = new byte[0];
		}
		#endregion

		#region private methods
		private byte[] GetTapeData(Guid tapeId)
		{
			byte[] tapeData;
			if (!_tapes.TryGetValue(tapeId, out tapeData))
			{
				// Allocate space for 1 billion bits (125 MB)
				tapeData = new byte[Tape.MaxContentLength / 8];
				_tapes[tapeId] = tapeData;
			}

			return tapeData;
		}
		#endregion
	}
}
