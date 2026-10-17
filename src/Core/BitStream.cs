using System;
using System.IO;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// A stream of bits. Pack rats sit on top of this the way they used to sit on
	/// <c>CodeReader</c> and <c>CodeWriter</c>.
	/// </summary>
	/// <remarks>
	/// Within each byte the least significant bit comes first. Bit <c>i</c> of the stream is bit
	/// <c>i % 8</c> (mask <c>1 &lt;&lt; (i % 8)</c>) of byte <c>i / 8</c>, and bit 0 of a
	/// <see cref="Code"/> is written first. Writing the 3-bit code <c>[1,0,1]</c>, aligning, then
	/// writing the 8-bit code <c>[1,1,1,1,0,0,0,0]</c> produces the bytes <c>0x05 0x0F</c>.
	///
	/// <see cref="Position"/> counts bits read or written through this wrapper, including bits
	/// skipped or padded by alignment. It starts at 0, whatever the underlying stream's position
	/// is. Getting it is always supported until disposal. Setting it throws
	/// <see cref="NotSupportedException"/> unless <see cref="CanSeek"/>, and no v1 implementation
	/// can seek.
	///
	/// A writer writes forward from wherever the underlying stream already is. It never reads back
	/// or rewrites bytes it has written. Callers must not reposition, read, or write that stream
	/// while the wrapper is active.
	///
	/// Alignment discards the rest of the current byte when reading, and zero-pads it when writing.
	/// Aligning when already aligned does nothing. <see cref="Flush"/> never pads: a partial byte
	/// stays buffered.
	///
	/// <see cref="ReadBits"/> throws <see cref="EndOfStreamException"/> when the underlying stream
	/// ends before the requested bits arrive. The reader does not guess how many trailing bits are
	/// padding. The caller supplies the count, from a length prefix, a schema, or a known size, and
	/// trailing pad bits in the final byte are simply never requested.
	///
	/// If the underlying stream throws during <see cref="ReadBits"/>, <see cref="WriteBits"/>,
	/// <see cref="AlignToNextByteBoundary"/>, or <see cref="Flush"/>, the exception passes through
	/// unwrapped and the wrapper is faulted. <see cref="EndOfStreamException"/> faults a reader too.
	/// Once faulted, every operational call throws <see cref="InvalidOperationException"/> with the
	/// original exception as <see cref="Exception.InnerException"/>, but only after the disposed,
	/// unsupported, and argument checks. A faulted writer never writes again, including during
	/// <see cref="Dispose"/>. After a failure, <see cref="Position"/> stays readable and is
	/// diagnostic only. It is not a safe point to resume from.
	///
	/// When more than one rule applies, the first match wins:
	/// <see cref="ObjectDisposedException"/>, then <see cref="NotSupportedException"/>, then
	/// argument exceptions, then the faulted <see cref="InvalidOperationException"/>, then an
	/// exception from the underlying stream.
	///
	/// Anything added later is <c>virtual</c>, with a default built on these members. It is never a
	/// new abstract member.
	/// </remarks>
	public abstract class BitStream : IDisposable
	{
		#region constants
		/// <summary>
		/// Key in <see cref="Exception.Data"/> for a failure while disposing an owned stream, when
		/// padding or flushing already failed. The original exception is the one rethrown.
		/// </summary>
		public const string DisposeExceptionDataKey = "BigRedProf.Data.BitStream.DisposeException";
		#endregion

		#region fields
		private bool _disposed;
		private Exception? _fault;
		private long _position;
		#endregion

		#region properties
		/// <summary>
		/// True when this wrapper reads bits. False after <see cref="Dispose"/>.
		/// </summary>
		public abstract bool CanRead { get; }

		/// <summary>
		/// True when this wrapper writes bits. False after <see cref="Dispose"/>.
		/// </summary>
		public abstract bool CanWrite { get; }

		/// <summary>
		/// True when <see cref="Position"/> can be set. No v1 implementation can seek, and this is
		/// false after <see cref="Dispose"/>.
		/// </summary>
		public abstract bool CanSeek { get; }

		/// <summary>
		/// Bits read or written through this wrapper, including alignment. Starts at 0.
		/// </summary>
		/// <remarks>
		/// Getting it throws <see cref="ObjectDisposedException"/> after disposal, and still works
		/// when the wrapper is faulted. Setting it throws <see cref="NotSupportedException"/> unless
		/// <see cref="CanSeek"/>.
		/// </remarks>
		public virtual long Position
		{
			get
			{
				ThrowIfDisposed();
				return _position;
			}
			set
			{
				ThrowIfDisposed();
				if (!CanSeek)
					throw new NotSupportedException("This stream does not support seeking.");

				ThrowIfFaulted();
				throw new NotSupportedException("This stream does not support seeking.");
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Reads the next <paramref name="bitCount"/> bits.
		/// </summary>
		/// <param name="bitCount">How many bits to read. Zero reads the empty code and consumes nothing.</param>
		/// <returns>The bits.</returns>
		public abstract Code ReadBits(int bitCount);

		/// <summary>
		/// Writes <paramref name="code"/>, least significant bit first.
		/// </summary>
		/// <param name="code">The bits to write. A zero-length code writes nothing.</param>
		public abstract void WriteBits(Code code);

		/// <summary>
		/// Moves to the next byte boundary. Reading discards the rest of the current byte. Writing
		/// zero-pads it. Already aligned is a no-op.
		/// </summary>
		public abstract void AlignToNextByteBoundary();

		/// <summary>
		/// Pushes complete bytes and asks the underlying stream to flush. A partial byte stays
		/// buffered, because flushing never pads. The reader's flush does no I/O. Every override
		/// still enforces the disposed and faulted checks.
		/// </summary>
		public virtual void Flush()
		{
			ThrowIfDisposed();
			ThrowIfFaulted();
		}

		/// <summary>
		/// Releases this wrapper. The call is idempotent: the second and later calls do nothing
		/// and never throw. A writer that is not faulted zero-pads and writes any partial byte,
		/// then flushes. A faulted writer skips that, so a partly failed write is never repeated.
		/// An owned underlying stream is disposed either way.
		/// </summary>
		public void Dispose()
		{
			if (_disposed)
				return;

			try
			{
				Dispose(true);
			}
			finally
			{
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Releases managed resources.
		/// </summary>
		/// <param name="disposing">True when called from <see cref="Dispose"/>.</param>
		protected virtual void Dispose(bool disposing)
		{
		}
		#endregion

		#region protected methods
		/// <summary>
		/// True after <see cref="Dispose"/> has begun its cleanup.
		/// </summary>
		protected bool IsDisposed
		{
			get
			{
				return _disposed;
			}
		}

		/// <summary>
		/// True after an I/O or end-of-stream failure.
		/// </summary>
		protected bool IsFaulted
		{
			get
			{
				return _fault != null;
			}
		}

		/// <summary>
		/// Throws <see cref="ObjectDisposedException"/> naming the runtime type.
		/// </summary>
		protected void ThrowIfDisposed()
		{
			if (_disposed)
				throw new ObjectDisposedException(GetType().Name);
		}

		/// <summary>
		/// Throws the faulted <see cref="InvalidOperationException"/>, with the original failure
		/// as its inner exception.
		/// </summary>
		protected void ThrowIfFaulted()
		{
			if (_fault != null)
				throw new InvalidOperationException("The bit stream is faulted and cannot be used.", _fault);
		}

		/// <summary>
		/// Records the first failure. Later failures do not replace it.
		/// </summary>
		/// <param name="exception">The failure from the underlying stream, or end of stream.</param>
		protected void Fault(Exception exception)
		{
			if (_fault == null)
				_fault = exception;
		}

		/// <summary>
		/// Applies the read-side precedence checks.
		/// </summary>
		/// <param name="bitCount">The requested length.</param>
		protected void PrepareRead(int bitCount)
		{
			ThrowIfDisposed();
			if (!CanRead)
				throw new NotSupportedException("This stream does not support reading.");

			if (bitCount < 0)
				throw new ArgumentOutOfRangeException(nameof(bitCount), "The parameter cannot be negative.");

			ThrowIfFaulted();
		}

		/// <summary>
		/// Applies the write-side precedence checks.
		/// </summary>
		/// <param name="code">The code about to be written.</param>
		protected void PrepareWrite(Code code)
		{
			ThrowIfDisposed();
			if (!CanWrite)
				throw new NotSupportedException("This stream does not support writing.");

			if (code == null)
				throw new ArgumentNullException(nameof(code));

			ThrowIfFaulted();
		}

		/// <summary>
		/// Applies the precedence checks shared by alignment and any direction-agnostic operation
		/// that has no arguments.
		/// </summary>
		protected void PrepareOperation()
		{
			ThrowIfDisposed();
			ThrowIfFaulted();
		}

		/// <summary>
		/// Counts bits that were successfully read, written, or skipped by alignment.
		/// </summary>
		/// <param name="bitCount">How many bits to add.</param>
		protected void AdvancePosition(long bitCount)
		{
			_position += bitCount;
		}

		/// <summary>
		/// Faults on failure and lets the original exception propagate.
		/// </summary>
		/// <param name="action">The I/O.</param>
		protected void RunIo(Action action)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Fault(exception);
				throw;
			}
		}
		#endregion
	}
}
