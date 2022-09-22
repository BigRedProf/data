using BigRedProf.Data.Internal.PackRats;
using System;
using System.Collections.Generic;
using System.Text;

namespace BigRedProf.Data
{
	/// <summary>
	/// This class allows models of a given type to be packed and unpacked
	/// to and from codes.
	/// </summary>
	/// <typeparam name="T">The type of model to pack and unpack.</typeparam>
	public abstract class PackRat<T>
	{
		#region protected constructors
		protected PackRat(IPiedPiper piedPiper)
		{
			if (piedPiper == null)
				throw new ArgumentNullException(nameof(piedPiper));
				
			PiedPiper = piedPiper;
		}
		#endregion

		#region protected properties
		protected IPiedPiper PiedPiper
		{
			get;
			private set;
		}
		#endregion

		#region protected methods
		/// <summary>
		/// Packs a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model.</param>
		/// <param name="packRat">The pack rat.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		protected void PackNullableModel<M>(
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

			if(model != null)
				packRat.PackModel(writer, model);
		}

		/// <summary>
		/// Unpacks a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="packRat">The pack rat.</param>
		/// <param name="byteAligned">Controls the packing size of the null marker.</param>
		/// <returns>The model.</returns>
		protected M UnpackNullableModel<M>(CodeReader reader, PackRat<M> packRat, ByteAligned byteAligned)
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

			if(!isNull)
				model = packRat.UnpackModel(reader);

			return model;
		}

		/// <summary>
		/// Packs a list using the specified list element pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="writer">The code writer.</param>
		/// <param name="list">The list.</param>
		/// <param name="elementSchemaId">The element schema identifier.</param>
		/// <param name="allowNullLists">Whether or not null lists are allowed.</param>
		/// <param name="allowNullElements">Whether or not null elements are allowed in the list.</param>
		/// <param name="byteAligned">Controls the packing size of the null markers.</param>
		protected void PackList<M>(
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

			if(list != null)
			{
				PackRat<int> efficientWholeNumber31PackRat = PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31);
				efficientWholeNumber31PackRat.PackModel(writer, list.Count);

				PackRat<M> elementPackRat = PiedPiper.GetPackRat<M>(elementSchemaId);
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
						elementPackRat.PackModel(writer, element);

						if (byteAligned == ByteAligned.Yes)
							writer.AlignToNextByteBoundary();
					}
				}
			}
		}

		/// <summary>
		/// Unpacks a list using the specified list element pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="elementSchemaId">The element schema identifier.</param>
		/// <param name="allowNullLists">Whether or not null lists are allowed.</param>
		/// <param name="allowNullElements">Whether or not null elements are allowed in the list.</param>
		/// <param name="byteAligned">Controls the packing size of the null markers.</param>
		/// <returns>The list.</returns>
		protected IList<M> UnpackList<M>(
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

			PackRat<int> efficientWholeNumber31PackRat = PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31);
			int elementCount = efficientWholeNumber31PackRat.UnpackModel(reader);

			PackRat<M> elementPackRat = PiedPiper.GetPackRat<M>(elementSchemaId);
			IList<M> list = new List<M>(elementCount);

			if (allowNullElements)
			{
				bool[] nullElementList = new bool[elementCount];
				for (int i = 0; i < elementCount; ++i)
					nullElementList[i] = reader.Read(1) == "1";

				if (byteAligned == ByteAligned.Yes)
					reader.AlignToNextByteBoundary();

				for(int i = 0; i < elementCount; ++i)
				{
					if(nullElementList[i])
					{
						M Element = elementPackRat.UnpackModel(reader);
						list.Add(Element);
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
				if(byteAligned == ByteAligned.Yes)
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

		#region abstract methods
		/// <summary>
		/// Packs a model into a <see cref="Code"/>.
		/// </summary>
		/// <param name="writer">The code writer.</param>
		/// <param name="model">The model to pack.</param>
		public abstract void PackModel(CodeWriter writer, T model);

		/// <summary>
		/// Unpacks a model from a <see cref="Code"/>.
		/// </summary>
		/// <param name="reader">The code reader.</param>
		/// <returns>The unpacked model.</returns>
		public abstract T UnpackModel(CodeReader reader);
		#endregion
	}
}
