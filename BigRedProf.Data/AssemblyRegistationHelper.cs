using System;

namespace BigRedProf.Data
{
	/// <summary>
	/// Allows the pack rat compiler to generate code to help with assembly registration.
	/// <see cref="IPiedPiper.RegisterPackRats(System.Reflection.Assembly)"/>.
	/// </summary>
	abstract public class AssemblyRegistrationHelper
	{
		#region abstract methods
		/// <summary>
		/// This method will register pack rats. The pack rat compiler generates code for this.
		public abstract void RegisterPackRats(IPiedPiper piedPiper);
		#endregion
	}
}
