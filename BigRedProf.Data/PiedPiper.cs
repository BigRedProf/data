using BigRedProf.Data.Internal;
using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BigRedProf.Data
{
	public class PiedPiper : IPiedPiper
	{
		#region fields
		private IDictionary<Guid, IWeaklyTypedPackRat> _dictionary;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new <see cref="PiedPiper"/> instance.
		/// </summary>
		public PiedPiper()
		{
			_dictionary = new Dictionary<Guid, IWeaklyTypedPackRat>();
		}
		#endregion

		#region IPiedPiper methods
		/// <inheritdoc/>
		public void RegisterDefaultPackRats()
		{
			RegisterPackRat<bool>(new BooleanPackRat(this), SchemaId.Boolean);
			RegisterPackRat<Code>(new CodePackRat(this), SchemaId.Code);
			RegisterPackRat<int>(new EfficientWholeNumber31PackRat(this), SchemaId.EfficientWholeNumber31);
			RegisterPackRat<int>(new Int32PackRat(this), SchemaId.Int32);
			RegisterPackRat<Guid>(new GuidPackRat(this), SchemaId.Guid);
			RegisterPackRat<float>(new SinglePackRat(this), SchemaId.Single);
			RegisterPackRat<string>(new StringPackRat(this), SchemaId.TextUtf8);
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
					object packRat = Activator.CreateInstance(type, this);
					AddPackRatToDictionary((IWeaklyTypedPackRat) packRat, attribute.SchemaId);
				}
			}
		}

		/// <inheritdoc/>
		public PackRat<T> GetPackRat<T>(string schemaId)
		{
			if(schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

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

		/// <inheritdoc/>
        public Code EncodeModel<M>(M model, string schemaId)
        {
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

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
		public Code EncodeModelWithSchema(object model, string schemaId)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			PackRat<Guid> guidPackRat = GetPackRat<Guid>(SchemaId.Guid);
			IWeaklyTypedPackRat modelPackRat = GetPackRat(schemaId);
			CodeStream codeStream = new CodeStream();
			using (CodeWriter codeWriter = new CodeWriter(codeStream))
			{
				// encode the schema identifier
				guidPackRat.PackModel(codeWriter, new Guid(schemaId));

				// then encode the model
				modelPackRat.PackModel(codeWriter, model);
			}
			Code code = codeStream.ToCode();

			return code;
		}

		/// <inheritdoc/>
		public ModelWithSchema DecodeModelWithSchema(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			ModelWithSchema modelWithSchema = new ModelWithSchema();
			PackRat<Guid> guidPackRat = GetPackRat<Guid>(SchemaId.Guid);
			MemoryStream memoryStream = new MemoryStream(code.ToByteArray());
			using (CodeReader codeReader = new CodeReader(memoryStream))
			{
				// decode the schema identifier
				modelWithSchema.SchemaId = guidPackRat.UnpackModel(codeReader).ToString();

				// then decode the model
				IWeaklyTypedPackRat modelPackRat = GetPackRat(modelWithSchema.SchemaId);
				modelWithSchema.Model = modelPackRat.UnpackModel(codeReader);
			}

			return modelWithSchema;
		}

		public byte[] SaveCodeToByteArray(Code code)
		{
			if (code == null)
				throw new ArgumentNullException(nameof(code));

			MemoryStream memoryStream = new MemoryStream((code.Length + 7) / 8);
			PackRat<Code> packRat = GetPackRat<Code>(SchemaId.Code);
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
			PackRat<Code> packRat = GetPackRat<Code>(SchemaId.Code);
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
	}
}
