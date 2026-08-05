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

		#region functions
			public static IPiedPiper CreatePiedPiper()
			{
				IPiedPiper piedPiper = new PiedPiper();
				piedPiper.RegisterCorePackRats();
				piedPiper.DefineCoreTraits();
				piedPiper.DefineTrait(new TraitDefinition(TapeTrait.ClientCheckpointCode, CoreSchema.Code));
				piedPiper.DefineTrait(new TraitDefinition(TapeTrait.SeriesDescription, CoreSchema.TextUtf8));
				piedPiper.DefineTrait(new TraitDefinition(TapeTrait.TapePosition, CoreSchema.Int32));
				return piedPiper;
			}

			public static TapeProvider CreateMemoryTapeProvider()
			{
				return new MemoryTapeProvider();
			}

			/// <summary>
			/// Creates a temp directory for a disk tape provider. The caller owns it and must
			/// dispose it. It is deliberately NOT held in a static field: xUnit runs test
			/// classes in parallel, so a shared static is overwritten by whichever class
			/// constructs last, and the first class to dispose then deletes a directory another
			/// class is still using.
			/// </summary>
			public static TempDir CreateDiskTempDir()
			{
				return new TempDir(Path.Combine(Path.GetTempPath(), "UnitTests.DiskTapeProvider"));
			}

			public static TapeProvider CreateDiskTapeProvider(TempDir tempDir)
			{
				if (tempDir == null)
					throw new ArgumentNullException(nameof(tempDir));

				return new DiskTapeProvider(tempDir.Path);
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
