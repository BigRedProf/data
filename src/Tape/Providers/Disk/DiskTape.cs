using System;
using System.IO;

namespace BigRedProf.Data.Tape.Providers.Disk
{
	public class DiskTape : Tape
	{
		public DiskTape(string filePath, Guid tapeId)
			: base(new DiskTapeProvider(filePath), tapeId)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (!File.Exists(filePath))
				File.Create(filePath).Dispose();
		}
	}
}
