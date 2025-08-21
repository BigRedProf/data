using BigRedProf.Data.Core;
using Xunit;
using System;

namespace BigRedProf.Data.Tape._TestHelpers
{
	internal class TapeProviderHelper
	{
		#region functions
		public static void TestWriteAndReadRoundTrip(
			TapeProvider tapeProvider,
			Guid tapeId,
			byte[] content,
			int offset)
		{
			tapeProvider.WriteInternal(tapeId, content, offset, content.Length);
			var result = tapeProvider.ReadInternal(tapeId, offset, content.Length);
			Assert.Equal(content, result);
		}
		#endregion
	}
}