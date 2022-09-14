using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class CompilerError : Exception
	{
		#region constants
		public const int CSharpCompilation = 101;
		public const int InvalidFieldPosition = 102;
		#endregion
	}
}
