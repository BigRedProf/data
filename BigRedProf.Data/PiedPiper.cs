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
		public PackRat<T> GetPackRat<T>(string schemaId)
		{
			if(schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			Guid schemaIdAsGuid;
			if (!Guid.TryParse(schemaId, out schemaIdAsGuid))
				throw new ArgumentException("The schema identifier must be a GUID.", nameof(schemaId));

			object packRatAsObject;
			if (!_dictionary.TryGetValue(schemaIdAsGuid, out packRatAsObject))
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

		public void RegisterPackRat<T>(PackRat<T> packRat, string schemaId)
		{
			if(packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			Guid schemaIdAsGuid;
			if (!Guid.TryParse(schemaId, out schemaIdAsGuid))
				throw new ArgumentException("The schema identifier must be a GUID.", nameof(schemaId));

			if (_dictionary.ContainsKey(schemaIdAsGuid))
			{
				throw new InvalidOperationException(
					$"A PackRat has already been registered for schema identifier {schemaId}."
				);
			}

			_dictionary.Add(schemaIdAsGuid, packRat);
		}
		#endregion
	}
}
