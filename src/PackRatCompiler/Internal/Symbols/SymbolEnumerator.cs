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
	internal class SymbolEnumerator : SymbolVisitor, IEnumerator<ISymbol>
	{
		#region fields
		private INamespaceSymbol? _rootNamespaceSymbol;
		private ISymbol? _currentSymbol;
		private INamespaceSymbol? _currentNamespaceSymbol;
		private IEnumerator<INamespaceOrTypeSymbol>? _currentNamespaceChildSymbolEnumerator;
		private INamedTypeSymbol? _currentNamedTypeSymbol;
		private IEnumerator<INamedTypeSymbol>? _currentNamedTypeChildSymbolEnumerator;
		#endregion

		#region constructors
		public SymbolEnumerator(INamespaceSymbol? namespaceSymbol)
		{
			_rootNamespaceSymbol = namespaceSymbol;
		}
		#endregion

		#region IEnumerator properties
		public ISymbol Current
		{
			get
			{
				if (_rootNamespaceSymbol == null)
					throw new InvalidOperationException("Attempted to read past the end of the enumerator.");

				Debug.Assert(_currentSymbol != null);
				return _currentSymbol;
			}
		}

		object IEnumerator.Current => Current;

		#endregion

		#region SymbolVisitor methods
		public override void VisitNamespace(INamespaceSymbol symbol)
		{
			_currentSymbol = symbol;
			_currentNamespaceSymbol = symbol;
			
			// nulling this tells MoveNext we're currently on a namespace node
			_currentNamedTypeSymbol = null;

			_currentNamespaceChildSymbolEnumerator = symbol.GetMembers().GetEnumerator();
		}

		public override void VisitNamedType(INamedTypeSymbol symbol)
		{
			_currentSymbol = symbol;
			_currentNamedTypeSymbol = symbol;

			_currentNamedTypeChildSymbolEnumerator = symbol.GetTypeMembers().ToList().GetEnumerator();
		}
		#endregion

		#region IEnumerator methods
		public void Dispose()
		{
			// TODO: implement
		}

		public bool MoveNext()
		{
			if (_currentSymbol == null)
			{
				if (_rootNamespaceSymbol != null)
				{
					// for the very first symbol, visit the root namespace
					Visit(_rootNamespaceSymbol);
					return true;
				}
				else
				{
					// unless there isn't a root namespace in which case we're already done
					return false;
				}
			}

			if(_currentNamedTypeSymbol == null)
			{
				// for namespace symbols, visit its first namespace or type child next
				Debug.Assert(_currentNamespaceChildSymbolEnumerator != null);
				if (_currentNamespaceChildSymbolEnumerator.MoveNext())
				{
					_currentNamespaceChildSymbolEnumerator.Current.Accept(this);
					return true;
				}
				else
				{
					// when the last namespace has no children, we're all done
					_rootNamespaceSymbol = null;
					_currentNamespaceSymbol = null;
					_currentNamedTypeSymbol = null;
					return false;
				}
			}

			// for named type symbols, 
			Debug.Assert(_currentNamedTypeChildSymbolEnumerator != null);
			if(_currentNamedTypeChildSymbolEnumerator.MoveNext())
			{
				// first visit their children
				_currentNamedTypeChildSymbolEnumerator.Current.Accept(this);
				return true;
			}
			else
			{
				// then the next named type symbol
				Debug.Assert(_currentNamespaceChildSymbolEnumerator != null);
				if(_currentNamespaceChildSymbolEnumerator.MoveNext())
				{
					_currentNamespaceChildSymbolEnumerator.Current.Accept(this);
					return true;
				}
				else
				{
					// when the last namespace has no children, we're all done
					_rootNamespaceSymbol = null;
					_currentNamespaceSymbol = null;
					_currentNamedTypeSymbol = null;
					return false;
				}
			}
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		#endregion	
	}
}
