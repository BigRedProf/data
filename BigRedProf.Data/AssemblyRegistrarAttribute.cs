using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// Identifies an assembly registrar as the official one for pack rats to use
	/// when <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/>
	/// is called for a given assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class AssemblyRegistrarAttribute : Attribute
	{
	}
}
