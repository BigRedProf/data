using BigRedProf.Data.Tape;
using System;
using BigRedProf.Data.Core;
using BigRedProf.Data.Tape.Internal;

public class TapeRecorder : TapeMover
{
	#region methods
	public void Record(Code content)
	{
		if(content == null) 
			throw new ArgumentNullException(nameof(content));

		VerifyTapeIsInserted();

		TapeHelper.WriteContent(Tape.TapeProvider, Tape.Id, content, Tape.Position);
		RewindOrFastForwardTo(Tape.Position + content.Length);
	}
	#endregion
}
