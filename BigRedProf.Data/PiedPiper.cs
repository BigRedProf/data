using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	public class PiedPiper : IPiedPiper
	{
		#region fields
		private IDictionary<Guid, object> _dictionary;
		#endregion

		#region constructors
		public PiedPiper()
		{
			_dictionary = new Dictionary<Guid, object>();
		}
		#endregion

		#region IPiedPiper methods
		public PackRat<T> GetPackRat<T>(Guid schemaId)
		{
			object packRatAsObject;
			if (!_dictionary.TryGetValue(schemaId, out packRatAsObject))
			{
				throw new ArgumentException(
					$"No PackRat is registered for schema identifier {schemaId}.",
					nameof(schemaId)
				);
			}

			PackRat<T> packRat = packRatAsObject as PackRat<T>;
			if (packRat == null)
			{
				throw new InvalidOperationException(
					$"The registered PackRat is of type {packRatAsObject.GetType().FullName} instead of type {typeof(T)}."
				);
			}

			return packRat;
		}

		public void RegisterPackRat<T>(PackRat<T> packRat, Guid schemaId)
		{
			if(packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			if (_dictionary.ContainsKey(schemaId))
			{
				throw new InvalidOperationException(
					$"A PackRat has already been registered for schema identifier {schemaId}."
				);
			}

			_dictionary.Add(schemaId, packRat);
		}
		#endregion
	}
}
