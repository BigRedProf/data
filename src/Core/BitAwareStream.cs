using System.IO;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Represents a stream that provides access to individual bits within bytes. Streams intended for use
	/// with <see cref="CodeReader"/> and <see cref="CodeWriter"/> should implement this interface."
	/// </summary>
	public abstract class BitAwareStream : Stream 
	{
		#region properties
		/// <summary>
		/// Gets the current byte being read from or written to. It's essentially irrelevant when
		/// <see cref="OffsetIntoCurrentByte"/> is zero. Otherwise, its low-order bits represent
		/// the last few bits at the current position in the stream.
		/// </summary>
		public byte CurrentByte { get; set; }

		/// <summary>
		/// Gets the offset into <see cref="CurrentByte"/>, in bits. Valid values are 0 through 7.
		/// </summary>
		public int OffsetIntoCurrentByte { get; set; }
		#endregion
	}
}
