using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Providers.Memory;
using BigRedProf.Data.Tape.Providers.Disk;
using System;
using System.IO;
using BigRedProf.Data.Tape.Test._TestHelpers;

namespace BigRedProf.Data.Tape._TestHelpers
{
	internal class TapeProviderHelper
	{
		#region static data
		public static IEnumerable<object[]> TapeProviders()
		{
			yield return new object[] { TapeProviderHelper.CreateMemoryTapeProvider() };
			//yield return new object[] { TapeProviderHelper.CreateDiskTapeProvider() };
		}
		#endregion

		#region static fields
		private static TempDir _tempDir;
		#endregion

		#region functions
		public static IPiedPiper CreatePiedPiper()
		{
			IPiedPiper piedPiper = new PiedPiper();
			piedPiper.RegisterCorePackRats();
			piedPiper.DefineCoreTraits();
			piedPiper.DefineTrait(new TraitDefinition(TapeTrait.TapePosition, CoreSchema.Int32));
			return piedPiper;
		}

		public static TapeProvider CreateMemoryTapeProvider()
		{
			return new MemoryTapeProvider();
		}

		public static TapeProvider CreateDiskTapeProvider()
		{
			_tempDir = new TempDir(Path.Combine(Path.GetTempPath(), "UnitTests.DiskTapeProvider"));
			return new DiskTapeProvider(_tempDir.Path);
		}

		public static void DestroyDiskTapeProvider()
		{
			_tempDir?.Dispose();
		}

		public static void TestWriteAndReadRoundTrip(
			TapeProvider tapeProvider,
			Guid tapeId,
			byte[] content,
			int offset)
		{
			Tape tape = Tape.CreateNew(tapeProvider, tapeId);
			tapeProvider.WriteTapeInternal(tapeId, content, offset, content.Length);
			var result = tapeProvider.ReadTapeInternal(tapeId, offset, content.Length);
			Assert.Equal(content, result);
		}
		#endregion
	}
}