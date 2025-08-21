using BigRedProf.Data.Tape;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;

public class TapePlayer : TapeMover
{
	#region constructors
	public TapePlayer(Tape tape) : base(tape)
	{
	}
	#endregion

	#region methods
	public Code Play(int length)
	{
		Code result = TapeHelper.ReadContent(Tape.TapeProvider, Tape.Id, Tape.Position, length);
		RewindOrFastForwardTo(Tape.Position + length);
		return result;
	} 
	#endregion
}
