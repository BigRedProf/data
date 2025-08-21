using System;
using System.Collections.Generic;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeProvider
	{
		#region abstract methods
		/// <summary>
		/// Retrieves all tapes from the underlying data source.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{T}"/> containing all tapes.</returns>
		abstract public IEnumerable<Tape> FetchAllTapesInternal();

		/// <summary>
		/// Attempts to retrieve a tape with the specified identifier.
		/// </summary>
		/// <param name="tapeId">The unique identifier of the tape to retrieve.</param>
		/// <param name="tape">
		/// When this method returns, contains the tape associated with the specified identifier, if the
		/// operation is successful; otherwise, <see langword="null"/>. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the tape was successfully retrieved; otherwise, <see langword="false"/>.
		/// </returns>
		abstract public bool TryFetchTapeInternal(Guid tapeId, out Tape tape);

		/// <summary>
		/// Reads a portion of the tape or its label. Must be implemented by subclasses.
		/// </summary>
		abstract public byte[] ReadInternal(Guid tapeId, int byteOffset, int byteLength);

		/// <summary>
		/// Writes content to the tape or its label. Must be implemented by subclasses.
		/// </summary>
		abstract public void WriteInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength);
		#endregion
	}
}
