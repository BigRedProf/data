using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Memory;
using BigRedProf.Data.Tape.Providers.Disk;
using System;
using System.IO;

namespace BigRedProf.Data.Tape._TestHelpers
{
	internal class TapeProviderHelper
	{
		#region static data
		public static IEnumerable<object[]> TapeProviders()
		{
			yield return new object[] { TapeProviderHelper.CreateMemoryTapeProvider() };
			yield return new object[] { TapeProviderHelper.CreateDiskTapeProvider() };
		}
		#endregion

		#region static fields
		public static readonly string TestDirectory = Path.Combine(Path.GetTempPath(), "TapeProviderTestTapes");
		#endregion

		#region functions
		public static IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineCoreTraits();
			return piedPiper;
		}

		public static TapeProvider CreateMemoryTapeProvider()
		{
			return new MemoryTapeProvider();
		}

		public static TapeProvider CreateDiskTapeProvider()
		{
			Directory.CreateDirectory(TestDirectory);
			return new DiskTapeProvider(TestDirectory);
		}

		public static void DestroyDiskTapeProvider()
		{
			if (Directory.Exists(TestDirectory))
			{
				Directory.Delete(TestDirectory, true);
			}
		}

		public static void TestWriteAndReadRoundTrip(
			TapeProvider tapeProvider,
			Guid tapeId,
			byte[] content,
			int offset)
		{
			tapeProvider.WriteTapeInternal(tapeId, content, offset, content.Length);
			var result = tapeProvider.ReadTapeInternal(tapeId, offset, content.Length);
			Assert.Equal(content, result);
		}
		#endregion
	}
}