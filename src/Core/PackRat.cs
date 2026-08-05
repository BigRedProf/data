using System;
using System.Globalization;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// This class allows models of a given type to be packed and unpacked
	/// to and from codes.
	/// </summary>
	/// <typeparam name="T">The type of model to pack and unpack.</typeparam>
	public abstract class PackRat<T> : IWeaklyTypedPackRat
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

		#region INonGenericPackRat methods
		void IWeaklyTypedPackRat.PackModel(CodeWriter writer, object model)
		{
			T typedModel;
			if (model is T)
			{
				typedModel = (T) model;
			}
			else if (IsEnumOverUnderlyingType(model, typeof(T)))
			{
				// A boxed enum is never `is T` against its own underlying type, so an
				// enum-valued model would otherwise be rejected by the integral pack rat
				// that is perfectly capable of packing it.
				typedModel = (T) Convert.ChangeType(model, typeof(T), CultureInfo.InvariantCulture);
			}
			else
			{
				throw new ArgumentException("Invalid model type", nameof(model));
			}

			PackModel(writer, typedModel);
		}

		object IWeaklyTypedPackRat.UnpackModel(CodeReader reader)
		{
			return UnpackModel(reader);
		}
		#endregion

		#region private functions
		private static bool IsEnumOverUnderlyingType(object model, Type underlyingType)
		{
			bool isMatch = false;

			if (model != null)
			{
				Type modelType = model.GetType();
				if (modelType.IsEnum)
					isMatch = Enum.GetUnderlyingType(modelType) == underlyingType;
			}

			return isMatch;
		}
		#endregion
	}
}
