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
		#region protected fields
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
