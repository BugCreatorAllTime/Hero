
using System;
using strange.extensions.command.impl;

public class SaveGameCommand : Command
{
	[Inject]
	public DataService dataService {get; set;}
	public SaveGameCommand ()
	{
	}

	public override void Execute ()
	{
		dataService.OnPause(true);
	}
}


