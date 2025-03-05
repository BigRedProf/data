using System;
using System.Diagnostics;

namespace BigRedProf.Data.Core
{
	public struct Bit
	{
		#region fields
		private byte _value;
		#endregion

		#region private constructors
		private Bit(byte value)
		{
			Debug.Assert(value == 0 || value == 1, "Value must be 0 or 1.");

			_value = value;
		}
		#endregion

		#region object methods
		public override bool Equals(object obj)
		{
			if (!(obj is Bit))
				return false;

			return (this._value == ((Bit)obj)._value);
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public override string ToString()
		{
			return _value == 1 ? "1" : "0";
		}
		#endregion

		#region operator overloads
		public static bool operator==(Bit left, Bit right)
		{
			return left.Equals(right);
		}

		public static bool operator!=(Bit left, Bit right)
		{
			return !(left == right);
		}
		#endregion

		#region casts
		public static implicit operator int(Bit bit)
		{
			return bit == Bit.Zero ? 0 : 1;
		}

		public static implicit operator Bit(int integer)
		{
			if (integer == 0)
				return Bit.Zero;
			else if (integer == 1)
				return Bit.One;
			else
				throw new ArgumentOutOfRangeException("bit", "The integer must be either 0 or 1.");
		}
		#endregion

		#region static fields
		public static readonly Bit Zero = new Bit(0);
		public static readonly Bit One = new Bit(1);
		#endregion
	}
}
