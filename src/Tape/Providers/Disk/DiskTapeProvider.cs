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

			// Also ensure the labels directory exists
			bool createdLabelsDir = false;
			if (!Directory.Exists(LabelDirectoryPath))
			{
				try
				{
					Directory.CreateDirectory(LabelDirectoryPath);
					createdLabelsDir = true;
				}
				catch (Exception ex)
				{
					throw new IOException($"Failed to create label directory '{LabelDirectoryPath}'.", ex);
				}
			}

			// Make the .labels directory hidden
			FileAttributes attributes = File.GetAttributes(LabelDirectoryPath);
			if ((attributes & FileAttributes.Hidden) == 0)
				File.SetAttributes(LabelDirectoryPath, attributes | FileAttributes.Hidden);
		}
		#endregion

		#region private properties
		private string LabelDirectoryPath
		{
			get
			{
				return Path.Combine(_directoryPath, ".labels");
			}
		}
		#endregion

		#region TapeProvider methods
		public override bool TryFetchTapeInternal(Guid tapeId, out Tape? tape)
		{
			string filePath = GetFilePath(tapeId);
			if (!File.Exists(filePath))
			{
				tape = null;
				return false;
			}

			tape = new Tape(this, tapeId);
			return true;
		}

		public override IEnumerable<Tape> FetchAllTapesInternal()
		{
			// Enumerate all .tape files in the directory
			var tapeFiles = Directory.GetFiles(_directoryPath, "*.tape");
			foreach (var file in tapeFiles)
			{
				var fileName = Path.GetFileNameWithoutExtension(file);
				if (Guid.TryParse(fileName, out Guid tapeId))
				{
					yield return new Tape(this, tapeId);
				}
			}
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
			string labelFilePath = GetLabelFilePath(tapeId);
			if (!File.Exists(labelFilePath))
				throw new FileNotFoundException($"Label file for tape ID '{tapeId}' not found.", labelFilePath);

			return File.ReadAllBytes(labelFilePath);
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
			if(data == null)
				throw new ArgumentNullException(nameof(data));

			string labelFilePath = GetLabelFilePath(tapeId);
			File.WriteAllBytes(labelFilePath, data);
		}

		public override void AddTapeInternal(Tape tape)
		{
			if(tape == null)
				throw new ArgumentNullException(nameof(tape));
			
			string filePath = GetFilePath(tape.Id);
			EnsureFileExists(filePath);

			// Initialize an empty label
			string labelFilePath = GetLabelFilePath(tape.Id);
			if (!File.Exists(labelFilePath))
			{
				File.WriteAllBytes(labelFilePath, new byte[0]); // Start with an empty label
			}
		}
		#endregion

		#region private methods
		private string GetFilePath(Guid tapeId)
		{
			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));
		
			return Path.Combine(_directoryPath, $"{tapeId}.tape");
		}

		private string GetLabelFilePath(Guid tapeId)
		{
			if (tapeId == Guid.Empty)
				throw new ArgumentException("Tape ID cannot be empty.", nameof(tapeId));
		
			return Path.Combine(LabelDirectoryPath, $"{tapeId}.label");
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
