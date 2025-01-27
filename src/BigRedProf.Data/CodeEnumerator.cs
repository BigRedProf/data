using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BigRedProf.Data
{
	public class CodeEnumerator : IEnumerator<Bit>
	{
		#region fields
		private Code _code;
		private int _offset;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new object to enumerate the bits in a given code.
		/// </summary>
		/// <param name="code">The code to enumerate.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public CodeEnumerator(Code code)
		{
			if(code == null)
				throw new ArgumentNullException(nameof(code));

			_code = code;
			_offset = -1;
		}
		#endregion

		#region IEnumerator properties
		/// <inheritdoc/>
		public Bit Current
		{
			get
			{
				Debug.Assert(_offset >= -1);

				if (_offset == -1)
					throw new InvalidOperationException("Attempted to read before calling MoveNext.");

				if (_offset >= _code.Length)
					throw new InvalidOperationException("Attempted to read past end of code.");

				return _code[_offset];
			}
		}

		/// <inheritdoc/>
		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}
		#endregion

		#region IEnumerator methods
		/// <inheritdoc/>
		public bool MoveNext()
		{
			if (_offset >= _code.Length)
				throw new InvalidOperationException("Attempted to read past end of code.");

			++_offset;
			return (_offset < _code.Length);
		}

		/// <inheritdoc/>
		public void Reset()
		{
			_offset = -1;
		}
		#endregion

		#region IDisposable methods
		/// <inheritdoc/>
		public void Dispose()
		{
		}
		#endregion
	}
}
