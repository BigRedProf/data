using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BigRedProf.Data.Tape.Providers.Memory
{
	public class MemoryTapeProvider : TapeProvider
	{
		#region fields
		private readonly Dictionary<Guid, byte[]> _tapes;
		#endregion

		#region constructors
		public MemoryTapeProvider()
		{
			_tapes = new Dictionary<Guid, byte[]>();
		}
		#endregion

		#region TapeProvider methods
		public override bool TryFetchTapeInternal(Guid tapeId, out Tape tape)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<Tape> FetchAllTapesInternal()
		{
			throw new NotImplementedException();
		}

		public override byte[] ReadInternal(Guid tapeId, int byteOffset, int byteLength)
		{
			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));

			byte[] tapeData = GetTapeData(tapeId);
			var resultBytes = new byte[byteLength];
			Array.Copy(tapeData, byteOffset, resultBytes, 0, byteLength);
			return resultBytes;
		}

		public override void WriteInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength)
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
