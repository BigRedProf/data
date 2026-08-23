using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class PackListFieldAttributeTests
	{
		#region methods
		[Fact]
		[Trait("Region", "attribute usage")]
		public void PackListFieldAttribute_ShouldBeValidOnFieldsAndProperties()
		{
			AttributeUsageAttribute usage = GetAttributeUsage(typeof(PackListFieldAttribute));

			Assert.Equal(AttributeTargets.Field | AttributeTargets.Property, usage.ValidOn);
		}

		[Fact]
		[Trait("Region", "attribute usage")]
		public void PackFieldAttribute_ShouldBeValidOnFieldsAndProperties()
		{
			// PackField and PackListField must stay in step. PackListField was restricted to
			// fields alone for a long time, which made a list-valued property inexpressible.
			AttributeUsageAttribute usage = GetAttributeUsage(typeof(PackFieldAttribute));

			Assert.Equal(AttributeTargets.Field | AttributeTargets.Property, usage.ValidOn);
		}

		[Fact]
		[Trait("Region", "attribute usage")]
		public void PackListFieldAttribute_ShouldBeReadableFromBothAFieldAndAProperty()
		{
			Type probeType = typeof(ListAttributeTargetProbe);

			PropertyInfo property = probeType.GetProperty(nameof(ListAttributeTargetProbe.ListOnAProperty))!;
			FieldInfo field = probeType.GetField(nameof(ListAttributeTargetProbe.ListOnAField))!;

			PackListFieldAttribute propertyAttribute =
				property.GetCustomAttributes(typeof(PackListFieldAttribute), false)
				.Cast<PackListFieldAttribute>()
				.Single();
			PackListFieldAttribute fieldAttribute =
				field.GetCustomAttributes(typeof(PackListFieldAttribute), false)
				.Cast<PackListFieldAttribute>()
				.Single();

			Assert.Equal(1, propertyAttribute.Position);
			Assert.Equal(2, fieldAttribute.Position);
		}
		#endregion

		#region private functions
		private static AttributeUsageAttribute GetAttributeUsage(Type attributeType)
		{
			return attributeType
				.GetCustomAttributes(typeof(AttributeUsageAttribute), false)
				.Cast<AttributeUsageAttribute>()
				.Single();
		}
		#endregion
	}

	/// <summary>
	/// Compile-time coverage for <see cref="PackListFieldAttribute"/>'s attribute targets.
	/// If the attribute is ever restricted back to <see cref="AttributeTargets.Field"/>,
	/// this type stops compiling with CS0592 and Core.Test fails to build -- which is a
	/// louder signal than any assertion. The pack rat compiler's golden-file tests cannot
	/// catch that regression, because its model resources are excluded from compilation
	/// and only ever parsed as text.
	/// </summary>
	internal class ListAttributeTargetProbe
	{
		#region fields
		[PackListField(2, CoreSchema.TextUtf8, ByteAligned.Yes)]
		public IList<string> ListOnAField = new List<string>();
		#endregion

		#region properties
		[PackListField(1, CoreSchema.TextUtf8, ByteAligned.Yes)]
		public IList<string> ListOnAProperty { get; set; } = new List<string>();
		#endregion
	}
}
