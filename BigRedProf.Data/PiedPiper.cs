using BigRedProf.Data.Internal;
using BigRedProf.Data.Internal.PackRats;
using BigRedProf.Data.PackRats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BigRedProf.Data
{
	public class PiedPiper : IPiedPiper
	{
		#region fields
		private IDictionary<Guid, IWeaklyTypedPackRat> _packRats;
		private IDictionary<Guid, object> _tokenizers;
		private IDictionary<Guid, TraitDefinition> _traitDefinitions;
		#endregion

		#region constructors
		/// <summary>
		/// Creates a new <see cref="PiedPiper"/> instance.
		/// </summary>
		public PiedPiper()
		{
			_packRats = new Dictionary<Guid, IWeaklyTypedPackRat>();
			_tokenizers = new Dictionary<Guid, object>();
			_traitDefinitions = new Dictionary<Guid, TraitDefinition>();
		}
		#endregion

		#region IPiedPiper methods
		/// <inheritdoc/>
		public void RegisterCorePackRats()
		{
			RegisterPackRat<bool>(new BooleanPackRat(this), CoreSchema.Boolean);
			RegisterPackRat<Code>(new CodePackRat(this), CoreSchema.Code);
			RegisterPackRat<DateTime>(new DateTimePackRat(this), CoreSchema.DateTime);
			RegisterPackRat<double>(new DoublePackRat(this), CoreSchema.Double);
			RegisterPackRat<int>(new EfficientWholeNumber31PackRat(this), CoreSchema.EfficientWholeNumber31);
			RegisterPackRat<FlexModel>(new FlexModelPackRat(this), CoreSchema.FlexModel);
			RegisterPackRat<Guid>(new GuidPackRat(this), CoreSchema.Guid);
			RegisterPackRat<int>(new Int32PackRat(this), CoreSchema.Int32);
			RegisterPackRat<long>(new Int64PackRat(this), CoreSchema.Int64);
			RegisterPackRat<ModelWithSchemaAndLength>(new ModelWithSchemaAndLengthPackRat(this), CoreSchema.ModelWithSchemaAndLength);
			RegisterPackRat<ModelWithSchema>(new ModelWithSchemaPackRat(this), CoreSchema.ModelWithSchema);
			RegisterPackRat<float>(new SinglePackRat(this), CoreSchema.Single);
			RegisterPackRat<string>(new StringPackRat(this), CoreSchema.TextUtf8);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 1), CoreSchema.WholeNumber1);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 2), CoreSchema.WholeNumber2);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 3), CoreSchema.WholeNumber3);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 4), CoreSchema.WholeNumber4);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 5), CoreSchema.WholeNumber5);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 6), CoreSchema.WholeNumber6);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 7), CoreSchema.WholeNumber7);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 8), CoreSchema.WholeNumber8);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 9), CoreSchema.WholeNumber9);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 10), CoreSchema.WholeNumber10);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 11), CoreSchema.WholeNumber11);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 12), CoreSchema.WholeNumber12);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 13), CoreSchema.WholeNumber13);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 14), CoreSchema.WholeNumber14);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 15), CoreSchema.WholeNumber15);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 16), CoreSchema.WholeNumber16);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 17), CoreSchema.WholeNumber17);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 18), CoreSchema.WholeNumber18);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 19), CoreSchema.WholeNumber19);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 20), CoreSchema.WholeNumber20);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 21), CoreSchema.WholeNumber21);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 22), CoreSchema.WholeNumber22);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 23), CoreSchema.WholeNumber23);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 24), CoreSchema.WholeNumber24);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 25), CoreSchema.WholeNumber25);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 26), CoreSchema.WholeNumber26);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 27), CoreSchema.WholeNumber27);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 28), CoreSchema.WholeNumber28);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 29), CoreSchema.WholeNumber29);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 30), CoreSchema.WholeNumber30);
			RegisterPackRat<int>(new WholeNumberPackRat(this, 31), CoreSchema.WholeNumber31);
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
		public PackRat<T> GetPackRat<T>(AttributeFriendlyGuid schemaId)
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
		public void RegisterPackRat<T>(PackRat<T> packRat, AttributeFriendlyGuid schemaId)
		{
			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

			AddPackRatToDictionary(packRat, schemaId);
		}

		/// <inheritdoc/>
		public void RegisterTokenizer<TModel>(Tokenizer<TModel> tokenizer, AttributeFriendlyGuid tokenizerId)
		{
			if(tokenizer == null)
				throw new ArgumentNullException(nameof(tokenizer));

			if (_tokenizers.ContainsKey(tokenizerId))
			{
				throw new InvalidOperationException(
					$"A tokenizer with identifier '{tokenizerId}' has already been registered."
				);
			}

			_tokenizers.Add(tokenizerId, tokenizer);
		}

		/// <inheritdoc/>
		public Tokenizer<TModel> GetTokenizer<TModel>(AttributeFriendlyGuid tokenizerId)
		{
			if (!_tokenizers.TryGetValue(tokenizerId, out object tokenizerAsObject))
				throw new InvalidOperationException($"Tokenizer '{tokenizerId}' is not registered.");

			Tokenizer<TModel> tokenizer = tokenizerAsObject as Tokenizer<TModel>;
			if (tokenizer == null)
			{
				throw new InvalidOperationException(
					$"Invalid model type. Tokenizer '{tokenizerId}' is registered as type '{typeof(TModel).FullName}'."
				);
			}

			return tokenizer;
		}

		/// <inheritdoc/>
		public void RegisterTokenizers(Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException(nameof(assembly));

			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				object[] attributes = type.GetCustomAttributes(typeof(AssemblyTokenizerAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTokenizerAttribute attribute = (AssemblyTokenizerAttribute)attributes[0];

					object tokenizerInstance = Activator.CreateInstance(type);
					if (tokenizerInstance == null)
					{
						throw new InvalidOperationException($"Cannot create instance of tokenizer '{type.FullName}'.");
					}

					AttributeFriendlyGuid tokenizerId = new AttributeFriendlyGuid(attribute.TokenizerId);
					Type tokenizerModelType = type.BaseType.GetGenericArguments()[0];

					// register the tokenizer
					MethodInfo registerTokenizerMethod = typeof(PiedPiper).GetMethod(nameof(RegisterTokenizer)).MakeGenericMethod(tokenizerModelType);
					registerTokenizerMethod.Invoke(this, new object[] { tokenizerInstance, tokenizerId });

					// create the pack rat
					Type packRatType = typeof(TokenizedModelPackRat<>).MakeGenericType(tokenizerModelType);
					object packRatInstance = Activator.CreateInstance(packRatType, new object[] { this, tokenizerInstance });

					// register the pack rat
					MethodInfo registerPackRatMethod = typeof(PiedPiper).GetMethod(nameof(RegisterPackRat)).MakeGenericMethod(typeof(TokenizedModel<>).MakeGenericType(tokenizerModelType));
					registerPackRatMethod.Invoke(this, new object[] { packRatInstance, tokenizerId });
				}
			}
		}

		/// <inheritdoc/>
		public void DefineTrait(TraitDefinition traitDefintion)
		{
			if(traitDefintion == null)
				throw new ArgumentNullException(nameof(traitDefintion));

			Guid traitId = traitDefintion.TraitId;

			if (_traitDefinitions.ContainsKey(traitId))
				throw new ArgumentException($"Trait '{traitId}' already defined.");

			_traitDefinitions.Add(traitId, traitDefintion);
		}

		/// <inheritdoc/>
		public TraitDefinition GetTraitDefinition(AttributeFriendlyGuid traitId)
		{
			if (!_traitDefinitions.TryGetValue(traitId, out TraitDefinition traitDefinition))
				throw new ArgumentException($"Trait '{traitId}' not defined.", nameof(traitId));

			return traitDefinition;
		}

		/// <inheritdoc/>
		public void PackModel<M>(CodeWriter writer, M model, AttributeFriendlyGuid schemaId)
		{
			PackRat<M> modelPackRat = GetPackRat<M>(schemaId);
			modelPackRat.PackModel(writer, model);
		}

		/// <inheritdoc/>
		public M UnpackModel<M>(CodeReader reader, AttributeFriendlyGuid schemaId)
		{
			PackRat<M> modelPackRat = GetPackRat<M>(schemaId);
			M model = modelPackRat.UnpackModel(reader);

			return model;
		}

		/// <inheritdoc/>
		public void PackModel(CodeWriter writer, object model, AttributeFriendlyGuid schemaId)
		{
			IWeaklyTypedPackRat modelPackRat = GetPackRat(schemaId);
			modelPackRat.PackModel(writer, model);
		}

		/// <inheritdoc/>
		public object UnpackModel(CodeReader reader, AttributeFriendlyGuid schemaId)
		{
			IWeaklyTypedPackRat packRat = GetPackRat(schemaId);
			object model = packRat.UnpackModel(reader);
			
			return model;
		}

		/// <inheritdoc/>
		public void PackNullableModel<M>(
			CodeWriter writer,
			M model,
			AttributeFriendlyGuid schemaId,
			ByteAligned byteAligned
		)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

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
		public M UnpackNullableModel<M>(CodeReader reader, AttributeFriendlyGuid schemaId, ByteAligned byteAligned)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			if (schemaId == null)
				throw new ArgumentNullException(nameof(schemaId));

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
			AttributeFriendlyGuid elementSchemaId,
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
			AttributeFriendlyGuid elementSchemaId,
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
        public Code EncodeModel<M>(M model, AttributeFriendlyGuid schemaId)
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
        public M DecodeModel<M>(Code code, AttributeFriendlyGuid schemaId)
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
		private void AddPackRatToDictionary(IWeaklyTypedPackRat packRat, AttributeFriendlyGuid schemaId)
		{
			if (_packRats.ContainsKey(schemaId))
			{
				throw new InvalidOperationException(
					$"A PackRat has already been registered for schema identifier {schemaId}."
				);
			}

			_packRats.Add(schemaId, packRat);
		}

		private IWeaklyTypedPackRat GetPackRat(AttributeFriendlyGuid schemaId)
		{
			Debug.Assert(schemaId != null);

			IWeaklyTypedPackRat packRat;
			if (!_packRats.TryGetValue(schemaId, out packRat))
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
