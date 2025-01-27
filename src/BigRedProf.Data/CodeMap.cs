using System.Collections.Generic;

namespace BigRedProf.Data
{
	public class CodeMap
	{
		#region fields
		private IDictionary<Code, Code> _dictionary;
		#endregion

		#region constructors
		public CodeMap()
		{
			_dictionary = new Dictionary<Code, Code>();
		}

		public CodeMap(int initialCapacity)
		{
			_dictionary = new Dictionary<Code, Code>(initialCapacity);
		}
		#endregion

		#region properties
		public ICollection<Code> Keys
		{
			get
			{
				return _dictionary.Keys;
			}
		}

		public ICollection<Code> Values
		{
			get
			{
				return _dictionary.Values;
			}
		}

		public Code this[Code key]
		{
			get
			{
				return _dictionary[key];
			}
		}
		#endregion

		#region methods
		public bool ContainsKey(Code key)
		{
			return _dictionary.ContainsKey(key);
		}

		public bool TryGetValue(Code key, out Code value)
		{
			return _dictionary.TryGetValue(key, out value);
		}
		#endregion
	}
}
