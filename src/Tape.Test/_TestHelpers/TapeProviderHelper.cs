using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Memory;
using BigRedProf.Data.Tape.Providers.Disk;

namespace BigRedProf.Data.Tape._TestHelpers
{
	internal class TapeProviderHelper
	{
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

		public static TapeProvider CreateDiskTapeProvider(string directoryPath = "TestTapes")
		{
			return new DiskTapeProvider(directoryPath);
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