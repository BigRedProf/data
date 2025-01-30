using BigRedProf.Data.Tape;
using BigRedProf.Data;

public class TapePlayer : TapeMover
{
	#region constructors
	public TapePlayer(TapeProvider provider) : base(provider)
	{
	}
	#endregion

	#region methods
	public Code Play(int length)
	{
		Code result = _provider.Read(length, Position);
		RewindOrFastForwardTo(Position + length);
		return result;
	} 
	#endregion
}
