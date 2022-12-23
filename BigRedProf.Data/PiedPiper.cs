using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BigRedProf.Data
{
	public class PiedPiper : IPiedPiper
	{
		#region fields
		private IDictionary<Guid, object> _dictionary;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new <see cref="PiedPiper"/> instance.
		/// </summary>
		public PiedPiper()
		{
			_dictionary = new Dictionary<Guid, object>();
		}
		#endregion

		#region IPiedPiper methods
		/// <inheritdoc/>
		public void RegisterDefaultPackRats()
		{
			RegisterPackRat<bool>(new BooleanPackRat(this), SchemaId.Boolean);
			RegisterPackRat<int>(new EfficientWholeNumber31PackRat(this), SchemaId.EfficientWholeNumber31);
			RegisterPackRat<int>(new Int32PackRat(this), SchemaId.Int32);
			RegisterPackRat<string>(new StringPackRat(this), SchemaId.StringUtf8);
		}

		/// <inheritdoc/>
		public void RegisterPackRats(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			foreach(Type type in assembly.GetTypes())
			{
				AssemblyPackRatAttribute attribute = type.GetCustomAttributes<AssemblyPackRatAttribute>().FirstOrDefault();
				if(attribute != null)
				{
					object packRat = Activator.CreateInstance(type);
					AddPackRatToDictionary(packRat, attribute.SchemaId);
				}
			}
		}

		/// <inheritdoc/>
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
					$"The registered PackRat is of type {packRatAsObject.GetType().FullName} instead of type {typeof(PackRat<T>).FullName}."
				);
			}

			return packRat;
		}

		/// <inheritdoc/>
		public void RegisterPackRat<T>(PackRat<T> packRat, string schemaId)
		{
			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			AddPackRatToDictionary(packRat, schemaId);
		}

		/// <inheritdoc/>
		public void PackNullableModel<M>(
			CodeWriter writer,
			M model,
			PackRat<M> packRat,
			ByteAligned byteAligned
		)
			where M : new()
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			writer.WriteCode(model == null ? "0" : "1");
			if (byteAligned == ByteAligned.Yes)
				writer.AlignToNextByteBoundary();

			if (model != null)
				packRat.PackModel(writer, model);
		}

		/// <inheritdoc/>
		public M UnpackNullableModel<M>(CodeReader reader, PackRat<M> packRat, ByteAligned byteAligned)
			where M : new()
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			M model = default;

			bool isNull = reader.Read(1) == "0";
			if (byteAligned == ByteAligned.Yes)
				reader.AlignToNextByteBoundary();

			if (!isNull)
				model = packRat.UnpackModel(reader);

			return model;
		}

		/// <inheritdoc/>
		public void PackList<M>(
			CodeWriter writer,
			IList<M> list,
			string elementSchemaId,
			bool allowNullLists,
			bool allowNullElements,
			ByteAligned byteAligned
		)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (!allowNullLists && list == null)
			{
				throw new ArgumentNullException(
					nameof(list),
					"The list parameter cannot be null when allowNullList is false."
				);
			}

			if (elementSchemaId == null)
				throw new ArgumentNullException(nameof(elementSchemaId));

			if (allowNullLists)
				writer.WriteCode(list == null ? "0" : "1");

			if (list != null)
			{
				PackRat<int> efficientWholeNumber31PackRat = GetPackRat<int>(SchemaId.EfficientWholeNumber31);
				efficientWholeNumber31PackRat.PackModel(writer, list.Count);

				PackRat<M> elementPackRat = GetPackRat<M>(elementSchemaId);
				if (allowNullElements)
				{
					bool[] elementNullArray = new bool[list.Count];
					for (int i = 0; i < list.Count; ++i)
					{
						elementNullArray[i] = (list[i] != null);
						writer.WriteCode(list[i] != null ? "1" : "0");
					}

					if (byteAligned == ByteAligned.Yes)
						writer.AlignToNextByteBoundary();

					for (int i = 0; i < list.Count; ++i)
					{
						if (elementNullArray[i])
							elementPackRat.PackModel(writer, list[i]);

						if (byteAligned == ByteAligned.Yes)
							writer.AlignToNextByteBoundary();
					}
				}
				else
				{
					if (byteAligned == ByteAligned.Yes)
						writer.AlignToNextByteBoundary();

					foreach (M element in list)
					{
						if (element == null && !allowNullElements)
							throw new ArgumentException("Null element found in list.", nameof(list));

						elementPackRat.PackModel(writer, element);

						if (byteAligned == ByteAligned.Yes)
							writer.AlignToNextByteBoundary();
					}
				}
			}
		}

		/// <inheritdoc/>
		public IList<M> UnpackList<M>(
			CodeReader reader,
			string elementSchemaId,
			bool allowNullLists,
			bool allowNullElements,
			ByteAligned byteAligned
		)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			if (elementSchemaId == null)
				throw new ArgumentNullException(nameof(elementSchemaId));

			if (allowNullLists)
			{
				bool isNull = reader.Read(1) == "0";
				if (isNull)
					return null;
			}

			PackRat<int> efficientWholeNumber31PackRat = GetPackRat<int>(SchemaId.EfficientWholeNumber31);
			int elementCount = efficientWholeNumber31PackRat.UnpackModel(reader);

			PackRat<M> elementPackRat = GetPackRat<M>(elementSchemaId);
			IList<M> list = new List<M>(elementCount);

			if (allowNullElements)
			{
				bool[] nullElementList = new bool[elementCount];
				for (int i = 0; i < elementCount; ++i)
					nullElementList[i] = reader.Read(1) == "1";

				if (byteAligned == ByteAligned.Yes)
					reader.AlignToNextByteBoundary();

				for (int i = 0; i < elementCount; ++i)
				{
					if (nullElementList[i])
					{
						M element = elementPackRat.UnpackModel(reader);
						list.Add(element);
					}
					else
					{
						list.Add(default);
					}

					if (byteAligned == ByteAligned.Yes)
						reader.AlignToNextByteBoundary();
				}
			}
			else
			{
				if (byteAligned == ByteAligned.Yes)
					reader.AlignToNextByteBoundary();

				for (int i = 0; i < elementCount; ++i)
				{
					M element = elementPackRat.UnpackModel(reader);
					list.Add(element);

					if (byteAligned == ByteAligned.Yes)
						reader.AlignToNextByteBoundary();
				}
			}

			return list;
		}
		#endregion

		#region private methods
		private void AddPackRatToDictionary(object packRat, string schemaId)
		{
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
