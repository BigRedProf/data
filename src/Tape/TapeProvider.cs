using System;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeProvider
	{
		#region constants
		/// <summary>
		/// The maximum length, in bits, for a tape's content.
		/// </summary>
		public const int MaxContentLength = 1_000_000_000; // 1 billion bits
		#endregion 

		#region public methods
		/// <summary>
		/// Reads a portion of the tape starting at the specified offset.
		/// </summary>
		/// <param name="length">The number of bits to read.</param>
		/// <param name="offset">The starting position in bits.</param>
		/// <returns>The <see cref="Code"/> read from the tape.</returns>
		public Code Read(int length, int offset)
		{
			ValidateRange(offset, length);
			return ReadInternal(length, offset);
		}

		/// <summary>
		/// Writes content to the tape starting at the specified offset.
		/// </summary>
		/// <param name="content">The content to write.</param>
		/// <param name="offset">The starting position in bits.</param>
		public void Write(Code content, int offset)
		{
			ValidateRange(offset, content.Length);
			WriteInternal(content, offset);
		}
		#endregion

		#region protected methods
		/// <summary>
		/// Validates the requested offset and length.
		/// </summary>
		/// <param name="offset">The starting position in bits.</param>
		/// <param name="length">The number of bits to process.</param>
		protected void ValidateRange(int offset, int length)
		{
			if (offset < 0 || offset >= MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "Offset is out of bounds.");
			}

			if (length <= 0 || offset + length > MaxContentLength)
			{
				throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds tape bounds.");
			}
		}
		#endregion

		#region abstract methods
		/// <summary>
		/// Reads a portion of the tape. Must be implemented by subclasses.
		/// </summary>
		protected abstract Code ReadInternal(int length, int offset);

		/// <summary>
		/// Writes content to the tape. Must be implemented by subclasses.
		/// </summary>
		protected abstract void WriteInternal(Code content, int offset);
		#endregion
	}
}
