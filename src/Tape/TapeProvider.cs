using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BigRedProf.Data.Tape
{
	public abstract class TapeProvider
	{
		#region constructors
		protected TapeProvider()
		{
			PiedPiper = new PiedPiper();
			PiedPiper.RegisterCorePackRats();
			PiedPiper.DefineCoreTraits();
			PiedPiper.DefineTrait(new TraitDefinition(TapeTrait.ClientCheckpointCode, CoreSchema.Code));
			PiedPiper.DefineTrait(new TraitDefinition(TapeTrait.TapePosition, CoreSchema.Int32));
		}
		#endregion

		#region properties
		public IPiedPiper PiedPiper 
		{
			get; 
			private set; 
		}
		#endregion

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
		abstract public bool TryFetchTapeInternal(Guid tapeId, out Tape? tape);

		/// <summary>
		/// Reads a portion of the tape. Must be implemented by subclasses.
		/// </summary>
		abstract public byte[] ReadTapeInternal(Guid tapeId, int byteOffset, int byteLength);

		/// <summary>
		/// Reads a tape's label. Must be implemented by subclasses.
		/// </summary>
		/// <param name="tapeId">The tape identifier.</param>
		/// <returns></returns>
		abstract public byte[] ReadLabelInternal(Guid tapeId);

		/// <summary>
		/// Writes content to the tape. Must be implemented by subclasses.
		/// </summary>
		abstract public void WriteTapeInternal(Guid tapeId, byte[] data, int byteOffset, int byteLength);

		/// <summary>
		/// Writes a tape's label. Must be implemented by subclasses.
		/// </summary>
		/// <param name="tapeId">The tape identifier.</param>
		/// <param name="data">The label, as packed bytes.</param>
		abstract public void WriteLabelInternal(Guid tapeId, byte[] data);

		/// <summary>
		/// Adds a tape to the underlying data source.
		/// </summary>
		/// <param name="tape">The tape to add.</param>
		abstract public void AddTapeInternal(Tape tape);
		#endregion
	}
}
