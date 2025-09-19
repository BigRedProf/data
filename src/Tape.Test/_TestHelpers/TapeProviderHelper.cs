using BigRedProf.Data.Core;

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