// HACKHACK: This class is shared between multiple test projects:
// - BigRedProf.Data.Tape.Test
// - BigRedProf.Stories.StoriesCli.Test
// BE SURE TO KEEP IN SYNC!

namespace BigRedProf.Data.Tape.Test._TestHelpers
{
	internal sealed class TempDir : IDisposable
	{
		public string Path { get; }

		public TempDir(string root)
		{
			var baseRoot = root;
			Directory.CreateDirectory(baseRoot);

			// Optional: sweep old dirs first (older than, say, 7 days)
			SweepOld(baseRoot, TimeSpan.FromDays(7));

			string unique = System.IO.Path.GetRandomFileName(); // 12-char, filesystem-safe
			Path = System.IO.Path.Combine(baseRoot, unique);
			Directory.CreateDirectory(Path);
		}

		public void Dispose()
		{
			try
			{
				if (Directory.Exists(Path))
					Directory.Delete(Path, recursive: true);
			}
			catch
			{
				// Swallow: tests shouldn't fail because cleanup couldn't delete a locked file.
				// Stale dirs will be swept next run by SweepOld
			}
		}

		private static void SweepOld(string baseRoot, TimeSpan olderThan)
		{
			try
			{
				var cutoff = DateTime.UtcNow - olderThan;
				foreach (var dir in Directory.EnumerateDirectories(baseRoot))
				{
					var info = new DirectoryInfo(dir);
					if (info.LastWriteTimeUtc < cutoff)
					{
						try { Directory.Delete(dir, true); } catch { /* ignore */ }
					}
				}
			}
			catch { /* ignore */ }
		}
	}
}