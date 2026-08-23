using BigRedProf.Data.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace BigRedProf.Data.Test
{
	public class SchemaIdTests
	{
		#region static fields
		[Fact]
		[Trait("Region", "static fields")]
		public void _NoSchemaIdsShouldCollide()
		{
			HashSet<Guid> schemaIds = new HashSet<Guid>();

			Type type = typeof(CoreSchema);
			IList<FieldInfo> fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach(FieldInfo field in fields)
			{
				Guid identifier = Guid.Parse((string)field.GetValue(null));

				bool schemaIdAlreadyExists = schemaIds.Contains(identifier);
				Assert.False(schemaIdAlreadyExists);

				schemaIds.Add(identifier);
			}
		}
		#endregion
	}
}
