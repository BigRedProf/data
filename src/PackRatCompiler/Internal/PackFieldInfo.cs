using BigRedProf.Data.Core;

namespace BigRedProf.Data.PackRatCompiler.Internal
{
	internal class PackFieldInfo
	{
		#region fields
		public string? Name { get; set; }
		public string? Type { get; set; }
		public bool IsEnum { get; set; }
		public bool IsNullable { get; set; }
		public ByteAligned ByteAligned { get; set; }
		public int Position { get; set; }
		public string? SchemaId { get; set; }
		public int SourceLineNumber { get; set; }
		public int SourceColumn { get; set; }
		#endregion
	}
}
