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
		protected void PackNullableModel<M>(CodeWriter writer, M model, PackRat<M> packRat)
			where M : new()
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			if(model == null)
			{
				writer.WriteCode("0");
			}
			else
			{
				writer.WriteCode("1");
				packRat.PackModel(writer, model);
			}
		}

		/// <summary>
		/// Unpacks a nullable model using the specified pack rat.
		/// </summary>
		/// <typeparam name="M"></typeparam>
		/// <param name="reader">The code reader.</param>
		/// <param name="packRat">The pack rat.</param>
		/// <returns></returns>
		protected M UnpackNullableModel<M>(CodeReader reader, PackRat<M> packRat)
			where M : new()
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			if (packRat == null)
				throw new ArgumentNullException(nameof(packRat));

			M model = default;
			bool isNull = reader.Read(1) == "0";
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
		protected void PackList<M>(
			CodeWriter writer, 
			IList<M> list, 
			string elementSchemaId, 
			bool allowNullLists, 
			bool allowNullElements
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
			{
				if (list == null)
					writer.WriteCode("0");
				else
					writer.WriteCode("1");
			}

			if(list != null)
			{
				PackRat<int> efficientWholeNumber31PackRat = PiedPiper.GetPackRat<int>(SchemaId.EfficientWholeNumber31);
				efficientWholeNumber31PackRat.PackModel(writer, list.Count);

				PackRat<M> elementPackRat = PiedPiper.GetPackRat<M>(elementSchemaId);
				writer.AlignToNextByteBoundary();
				foreach (M element in list)
				{
					if (allowNullElements)
						writer.WriteCode(element == null ? "0" : "1");

					if (element != null)
						elementPackRat.PackModel(writer, element);
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
		/// <returns>The list.</returns>
		protected IList<M> UnpackList<M>(
			CodeReader reader,
			string elementSchemaId, 
			bool allowNullLists,
			bool allowNullElements
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
			reader.AlignToNextByteBoundary();
			for(int i = 0; i < elementCount; ++i)
			{
				M element = default;
				bool isNullElement = false;
				if (allowNullElements)
					isNullElement = reader.Read(1) == "0";

				if (!isNullElement)
					element = elementPackRat.UnpackModel(reader);

				list.Add(element);
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
