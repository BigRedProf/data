﻿using BigRedProf.Data.Core.Internal;
using System;
using System.Reflection;

namespace BigRedProf.Data.Core
{
	/// <summary>
	/// Allows custom code to be executed when the host calls 
	/// <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/>.
	/// </summary>
	abstract public class AssemblyRegistrar
	{
		#region methods
		public void RegisterAssemblies(IPiedPiper piedPiper, Assembly assembly)
		{
			// find the assembly's registration helper (usually generated by the pack rat compiler)
			if (!ReflectionHelper.TryCreateTypeInAssemblyWithAttribute<AssemblyRegistrationHelper, AssemblyRegistrationHelperAttribute>(
				assembly, 
				out AssemblyRegistrationHelper assemblyRegistrationHelper)
			)
			{
				throw new ArgumentException("The assembly has no [AssemblyRegistrationHelper] class.", nameof(assembly));
			}

			assemblyRegistrationHelper.RegisterPackRats(piedPiper);
			this.OnAfterAssemblyPackRatsRegistered(piedPiper);
		}
		#endregion

		#region virtual methods
		/// <summary>
		/// This method will be called when the host calls
		/// <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/>
		/// right after the assembly pack rats are registered. Additional custom
		/// code such as initializing token dictionaries can be performed here.
		/// </summary>
		/// <param name="piedPiper">The pied piper.</param>
		public virtual void OnAfterAssemblyPackRatsRegistered(IPiedPiper piedPiper)
		{
		}
		#endregion
	}
}
