using System;

namespace BigRedProf.Data.Core.Internal
{
	/// <summary>
	/// This class is useful because it allows FlexModel's internal dictionary to store
	/// the real objects (and take no dependencies on the pied piper) while the FlexModelPackRat
	/// can use reflection magic to pack and unpack the models.
	/// </summary>
	internal class UntypedTrait
	{
		#region properties
		public Guid TraitId
		{
			get;
			set;
		}

		public object Model
		{
			get;
			set;
		}
		#endregion
	}
}
