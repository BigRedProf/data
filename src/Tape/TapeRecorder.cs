﻿using BigRedProf.Data.Tape;
using BigRedProf.Data;
using System;

public class TapeRecorder : TapeMover
{
	#region constructors
	public TapeRecorder(TapeProvider provider) : base(provider)
	{
	}
	#endregion

	#region methods
	public void Record(Code content)
	{
		if(content == null) 
			throw new ArgumentNullException(nameof(content));

		_provider.Write(content, Position);
		RewindOrFastForwardTo(Position + content.Length);
	}
	#endregion
}
