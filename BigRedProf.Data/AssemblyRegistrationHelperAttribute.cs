using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// Identifies an assembly registration helper as the official one for pack rats to use
	/// when <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/>
	/// is called for a given assembly. The pack rat compiler will generate the assembly
	/// registration helper.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class AssemblyRegistrationHelperAttribute : Attribute
	{
	}
}
