using BigRedProf.Data.Internal;
using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace BigRedProf.Data
{
	public class PiedPiper : IPiedPiper
	{
		#region fields
		private IDictionary<Guid, IWeaklyTypedPackRat> _dictionary;
		private IDictionary<Guid, TraitDefinition> _traitDefinitions;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new <see cref="PiedPiper"/> instance.
		/// </summary>
		public PiedPiper()
		{
			_dictionary = new Dictionary<Guid, IWeaklyTypedPackRat>();
			_traitDefinitions = new Dictionary<Guid, TraitDefinition>();
		}
		#endregion

		#region IPiedPiper methods
		/// <inheritdoc/>
		public void RegisterCorePackRats()
		{
			RegisterPackRat<bool>(new BooleanPackRat(this), CoreSchema.Boolean);
			RegisterPackRat<Code>(new CodePackRat(this), CoreSchema.Code);
			RegisterPackRat<int>(new EfficientWholeNumber31PackRat(this), CoreSchema.EfficientWholeNumber31);
			RegisterPackRat<int>(new Int32PackRat(this), CoreSchema.Int32);
			RegisterPackRat<long>(new Int64PackRat(this), CoreSchema.Int64);
			RegisterPackRat<Guid>(new GuidPackRat(this), CoreSchema.Guid);
			RegisterPackRat<ModelWithSchema>(new ModelWithSchemaPackRat(this), CoreSchema.ModelWithSchema);
			RegisterPackRat<float>(new SinglePackRat(this), CoreSchema.Single);
			RegisterPackRat<string>(new StringPackRat(this), CoreSchema.TextUtf8);
		}

		/// <inheritdoc/>
		public void RegisterPackRats(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			if (!ReflectionHelper.TryCreateTypeInAssemblyWithAttribute<AssemblyRegistrar, AssemblyRegistrarAttribute>(
				assembly,
				out AssemblyRegistrar assemblyRegistrar)
			)
			{
				throw new ArgumentException("The assembly has no [AssemblyRegistrar] class.", nameof(assembly));
			}

			assemblyRegistrar.RegisterAssemblies(this, assembly);
		}

		/// <inheritdoc/>
		public PackRat<T> GetPackRat<T>(string schemaId)
		{
			if(schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			IWeaklyTypedPackRat nonGenericPackRat = GetPackRat(schemaId);
			PackRat<T> packRat = nonGenericPackRat as PackRat<T>;
			if (packRat == null)
			{
				throw new InvalidOperationException(
					$"The registered PackRat is of type {nonGenericPackRat.GetType().FullName} instead of {typeof(PackRat<T>).FullName}."
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

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			AddPackRatToDictionary(packRat, schemaId);
		}

		/// <inheritdoc/>
		public void DefineTrait(TraitDefinition traitDefintion)
		{
			if(traitDefintion == null)
				throw new ArgumentNullException(nameof(traitDefintion));

			if (!IsValidSchemaId(traitDefintion.TraitId))
				throw new ArgumentException("Invalid trait identifier.", nameof(traitDefintion));

			Guid traitId = new Guid(traitDefintion.TraitId);

			if (_traitDefinitions.ContainsKey(traitId))
				throw new ArgumentException($"Trait '{traitId}' already defined.");

			_traitDefinitions.Add(traitId, traitDefintion);
		}

		/// <inheritdoc/>
		public TraitDefinition GetTraitDefinition(string traitId)
		{
			if (!IsValidSchemaId(traitId))
				throw new ArgumentException("Invalid trait identifier.", nameof(traitId));

			if (!_traitDefinitions.TryGetValue(new Guid(traitId), out TraitDefinition traitDefinition))
				throw new ArgumentException($"Trait '{traitId}' not defined.", nameof(traitId));

			return traitDefinition;
		}

		/// <inheritdoc/>
		public void PackModel<M>(CodeWriter writer, M model, string schemaId)
		{
			PackRat<M> modelPackRat = GetPackRat<M>(schemaId);
			modelPackRat.PackModel(writer, model);
		}

		/// <inheritdoc/>
		public M UnpackModel<M>(CodeReader reader, string schemaId)
		{
			PackRat<M> modelPackRat = GetPackRat<M>(schemaId);
			M model = modelPackRat.UnpackModel(reader);

			return model;
		}

		/// <inheritdoc/>
		public void PackModel(CodeWriter writer, object model, string schemaId)
		{
			IWeaklyTypedPackRat modelPackRat = GetPackRat(schemaId);
			modelPackRat.PackModel(writer, model);
		}

		/// <inheritdoc/>
		public object UnpackModel(CodeReader reader, string schemaId)
		{
			IWeaklyTypedPackRat packRat = GetPackRat(schemaId);
			object model = packRat.UnpackModel(reader);
			
			return model;
		}

		/// <inheritdoc/>
		public void PackNullableModel<M>(
			CodeWriter writer,
			M model,
			string schemaId,
			ByteAligned byteAligned
		)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			writer.WriteCode(model == null ? "0" : "1");
			if (byteAligned == ByteAligned.Yes)
				writer.AlignToNextByteBoundary();

			if (model != null)
			{
				PackRat<M> packRat = GetPackRat<M>(schemaId);
				packRat.PackModel(writer, model);
			}
		}

		/// <inheritdoc/>
		public M UnpackNullableModel<M>(CodeReader reader, string schemaId, ByteAligned byteAligned)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			M model = default;

			bool isNull = reader.Read(1) == "0";
			if (byteAligned == ByteAligned.Yes)
				reader.AlignToNextByteBoundary();

			if (!isNull)
			{
				PackRat<M> packRat = GetPackRat<M>(schemaId);
				model = packRat.UnpackModel(reader);
			}

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

			if (!IsValidSchemaId(elementSchemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(elementSchemaId));

			if (allowNullLists)
				writer.WriteCode(list == null ? "0" : "1");

			if (list != null)
			{
				PackRat<int> efficientWholeNumber31PackRat = GetPackRat<int>(CoreSchema.EfficientWholeNumber31);
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

			if (!IsValidSchemaId(elementSchemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(elementSchemaId));

			if (allowNullLists)
			{
				bool isNull = reader.Read(1) == "0";
				if (isNull)
					return null;
			}

			PackRat<int> efficientWholeNumber31PackRat = GetPackRat<int>(CoreSchema.EfficientWholeNumber31);
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

		/// <inheritdoc/>
        public Code EncodeModel<M>(M model, string schemaId)
        {
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			PackRat<M> packRat = GetPackRat<M>(schemaId);
			CodeStream codeStream = new CodeStream();
			using(CodeWriter codeWriter = new CodeWriter(codeStream))
			{
				packRat.PackModel(codeWriter, model);
			}
			Code code = codeStream.ToCode();
									
			return code;
        }

        /// <inheritdoc/>
        public M DecodeModel<M>(Code code, string schemaId)
        {
            if(code == null)
				throw new ArgumentNullException(nameof(code));

			if(schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			if (!IsValidSchemaId(schemaId))
				throw CreateInvalidSchemaIdArgumentException(nameof(schemaId));

			M model;
			PackRat<M> packRat = GetPackRat<M>(schemaId);
			MemoryStream memoryStream = new MemoryStream(code.ToByteArray());
			using (CodeReader codeReader = new CodeReader(memoryStream))
			{
				model = packRat.UnpackModel(codeReader);
			}

			return model;
		}

		/// <inheritdoc/>
		public byte[] SaveCodeToByteArray(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			MemoryStream memoryStream = new MemoryStream((code.Length + 7) / 8);
			PackRat<Code> packRat = GetPackRat<Code>(CoreSchema.Code);
			using (CodeWriter writer = new CodeWriter(memoryStream))
			{
				packRat.PackModel(writer, code);
			}

			return memoryStream.ToArray();
		}

		public Code LoadCodeFromByteArray(byte[] byteArray)
		{
			if (byteArray == null)
				throw new ArgumentNullException(nameof(byteArray));

			Code code;
			MemoryStream memoryStream = new MemoryStream(byteArray);
			PackRat<Code> packRat = GetPackRat<Code>(CoreSchema.Code);
			using (CodeReader reader = new CodeReader(memoryStream))
			{
				code = packRat.UnpackModel(reader);
			}

			return code;
		}
		#endregion

		#region private methods
		private void AddPackRatToDictionary(IWeaklyTypedPackRat packRat, string schemaId)
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

		private IWeaklyTypedPackRat GetPackRat(string schemaId)
		{
			Debug.Assert(schemaId != null);

			Guid schemaIdAsGuid;
			if (!Guid.TryParse(schemaId, out schemaIdAsGuid))
				throw new ArgumentException("The schema identifier must be a GUID.", nameof(schemaId));

			IWeaklyTypedPackRat packRat;
			if (!_dictionary.TryGetValue(schemaIdAsGuid, out packRat))
			{
				throw new ArgumentException(
					$"No PackRat is registered for schema identifier {schemaId}.",
					nameof(schemaId)
				);
			}

			return packRat;
		}
		#endregion

		#region private functions
		private static bool IsValidSchemaId(string schemaId)
		{
			if (schemaId == null)
				return false;

			if (schemaId.Length != 36)
				return false;

			for (int i = 0; i < 36; ++i)
			{
				char character = schemaId[i];

				if (i == 8 || i == 13 || i == 18 || i == 23)
				{
					if (character != '-')
						return false;
				}
				else
				{
					if (!((character >= '0' && character <= '9') || (character >= 'a' && character <= 'f')))
						return false;
				}
			}

			return true;
		}

		private static Exception CreateInvalidSchemaIdArgumentException(string argumentName)
		{
			return new ArgumentException(
				"Invalid schema identifier. Please use only lowercase hexadecimal digits and hyphens. " +
				"\"Eg: 01234567-89ab-cdef-0123-456789abcdef\"",
				argumentName
			);
		}
		#endregion
	}
}
