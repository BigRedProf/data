using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BigRedProf.Data.Internal
{
	internal class ReflectionHelper
	{
		#region functions
		public static bool TryCreateTypeInAssemblyWithAttribute<MType, MAttribute>(Assembly assembly, out MType result)
			where MAttribute : Attribute
		{
			Type[] typesInAssembly = assembly.GetTypes();
			foreach (Type type in typesInAssembly)
			{
				MAttribute attribute = type.GetCustomAttributes<MAttribute>().FirstOrDefault();
                if (attribute != null)
                {
					result = (MType)Activator.CreateInstance(type);
					return true;
				}
			}

			result = default;
			return false;
		}
		#endregion
	}
}
