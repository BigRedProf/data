using System;
using System.IO;

namespace BigRedProf.Data.Tape.Providers.Disk
{
	public class DiskTapeProvider : TapeProvider
	{
		#region fields
		private readonly string _filePath;
		#endregion

		#region constructors
		public DiskTapeProvider(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath));

			_filePath = filePath;

			// Ensure the file exists and is the correct size
			if (!File.Exists(_filePath))
			{
				using (FileStream fs = File.Create(_filePath))
				{
					fs.SetLength(MaxContentLength / 8); // Allocate full tape size (125MB)
				}
			}
		}
		#endregion

		#region TapeProvider methods
		protected override byte[] ReadInternal(int byteOffset, int byteLength)
		{
			byte[] resultBytes = new byte[byteLength];

			using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				fs.Seek(byteOffset, SeekOrigin.Begin);
				int bytesRead = fs.Read(resultBytes, 0, byteLength);

				// Zero-fill any unread bytes (e.g., reading past EOF)
				if (bytesRead < byteLength)
					Array.Clear(resultBytes, bytesRead, byteLength - bytesRead);
			}

			return resultBytes;
		}

		protected override void WriteInternal(byte[] data, int byteOffset, int byteLength)
		{
			using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
			{
				fs.Seek(byteOffset, SeekOrigin.Begin);
				fs.Write(data, 0, byteLength);
				fs.Flush();
			}
		}
		#endregion
	}
}
