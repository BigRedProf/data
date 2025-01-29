using Xunit;

namespace BigRedProf.Data.Tape._TestHelpers
{
	internal class TapeProviderHelper
	{
		#region functions
		public static void TestWriteAndReadRoundTrip(TapeProvider tapeProvider, Code content, int offset)
		{
			// Act
			tapeProvider.Write(content, offset);
			var result = tapeProvider.Read(offset, content.Length);

			// Assert
			Assert.Equal(content, result);
			#endregion
		}
	}
}