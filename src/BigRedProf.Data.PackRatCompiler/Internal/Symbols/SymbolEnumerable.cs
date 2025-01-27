using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal.Symbols
{
	internal class SymbolEnumerable : IEnumerable<ISymbol>
	{
		#region fields
		private INamespaceSymbol? _rootNamespaceSymbol;
		#endregion

		#region constructors
		public SymbolEnumerable(INamespaceSymbol? rootNamespaceSymbol)
		{
			_rootNamespaceSymbol = rootNamespaceSymbol;
		}
		#endregion

		#region IEnumerable methods
		public IEnumerator<ISymbol> GetEnumerator()
		{
			return new SymbolEnumerator(_rootNamespaceSymbol);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion	
	}
}
