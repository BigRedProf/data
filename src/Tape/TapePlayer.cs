using BigRedProf.Data.Tape;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;

public class TapePlayer : TapeMover
{
	#region methods
	public Code Play(int length)
	{
		VerifyTapeIsInserted();

		Code result = TapeHelper.ReadContent(Tape, Tape.Position, length);
		RewindOrFastForwardTo(Tape.Position + length);
		return result;
	} 
	#endregion
}
