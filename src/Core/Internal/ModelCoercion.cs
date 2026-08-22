using System;
using System.Diagnostics.CodeAnalysis;

namespace BigRedProf.Data.Core.Internal
{
	/// <summary>
	/// Coerces a weakly unpacked model to the type a caller asked for.
	/// </summary>
	/// <remarks>
	/// Anything that holds a code and interprets it on demand -- a <see cref="Datum"/>, a trait
	/// value inside a <see cref="FlexDatum"/> -- has to unpack weakly, because the model type is
	/// the caller's question rather than the datum's. Unpacking weakly is the only way a schema
	/// this reader does not recognise can stay skippable instead of fatal.
	///
	/// What the caller then asks for does not always match what came back, and one mismatch is
	/// legitimate rather than an error: see below.
	/// </remarks>
	internal static class ModelCoercion
	{
		#region functions
		/// <summary>
		/// Tries to present a weakly unpacked model as the requested type.
		/// </summary>
		/// <remarks>
		/// A model packed through an integral schema comes back boxed as that integral type, so a
		/// boxed int is not `is M` against an enum M even when the enum's underlying type is int.
		/// That is the one coercion worth doing, and it is why this exists rather than a cast at
		/// each call site.
		/// </remarks>
		/// <typeparam name="M">The type the caller asked for.</typeparam>
		/// <param name="model">The weakly unpacked model.</param>
		/// <param name="coerced">The model as the requested type.</param>
		/// <returns>True if the model can be presented as that type, otherwise false.</returns>
		public static bool TryCoerce<M>(object model, [MaybeNullWhen(false)] out M coerced)
		{
			if (model is M typedModel)
			{
				coerced = typedModel;
				return true;
			}

			Type modelType = typeof(M);
			if (model != null && modelType.IsEnum && model.GetType() == Enum.GetUnderlyingType(modelType))
			{
				coerced = (M) Enum.ToObject(modelType, model);
				return true;
			}

			coerced = default;
			return false;
		}

		/// <summary>
		/// Describes a model's actual type, for the message when a coercion fails.
		/// </summary>
		public static string DescribeType(object model)
		{
			return model != null ? model.GetType().Name : "null";
		}
		#endregion
	}
}
