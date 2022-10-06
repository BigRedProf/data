using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class PackListFieldInfo : PackFieldInfo
	{
		#region fields
		public string? ElementType { get; set; }
		public bool IsElementNullable { get; set; }
		public string? ElementSchemaId { get; set; }
		#endregion
	}
}
