using System;
using System.Collections.Generic;
using System.IO;

namespace BigRedProf.Data.Tape.Providers.Disk
{
	public class DiskTapeProvider : TapeProvider
	{
		#region fields
		private readonly string _directoryPath;
		#endregion

		#region constructors
		public DiskTapeProvider(string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(directoryPath))
				throw new ArgumentNullException(nameof(directoryPath));

			_directoryPath = directoryPath;

			// Ensure the directory exists
			if (!Directory.Exists(_directoryPath))
			{
				try
				{
					Directory.CreateDirectory(_directoryPath);
				}
				catch (Exception ex)
				{
					throw new IOException($"Failed to create directory '{_directoryPath}'.", ex);
				}
			}
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

		public override byte[] ReadTapeInternal(Guid tapeId, int byteOffset, int byteLength)
		{
			byte[] resultBytes = new byte[byteLength];

			string filePath = GetFilePath(tapeId);
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				fs.Seek(byteOffset, SeekOrigin.Begin);
				int bytesRead = fs.Read(resultBytes, 0, byteLength);

				// Zero-fill any unread bytes (e.g., reading past EOF)
				if (bytesRead < byteLength)
					Array.Clear(resultBytes, bytesRead, byteLength - bytesRead);
			}

			return resultBytes;
		}

		public override byte[] ReadLabelInternal(Guid tapeId)
		{
			throw new NotImplementedException();
		}

		public override void WriteTapeInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength)
		{
			if (data == null)
				throw new ArgumentNullException(nameof(data));
			
			if (byteOffset < 0 || byteLength <= 0 || byteOffset + byteLength > data.Length)
				throw new ArgumentOutOfRangeException("Invalid byte offset or length.");

			string filePath = GetFilePath(tapeId);
			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
			{
				fs.Seek(byteOffset, SeekOrigin.Begin);
				fs.Write(data, 0, byteLength);
				fs.Flush();
			}
		}

		public override void WriteLabelInternal(Guid tapeId, byte[] data)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region private methods
		// TODO: Add methods for the label files, too. They should be in a hidden
		// .labels directory.

		private string GetFilePath(Guid tapeId)
		{
			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));
		
			return Path.Combine(_directoryPath, $"{tapeId}.tape");
		}

		private void EnsureFileExists(string filePath)
		{
			// Ensure the file exists and is the correct size
			if (!File.Exists(filePath))
			{
				using (FileStream fs = File.Create(filePath))
				{
					fs.SetLength(Tape.MaxContentLength / 8); // Allocate full tape size (125MB)
				}
			}
		}
		#endregion
	}
}
