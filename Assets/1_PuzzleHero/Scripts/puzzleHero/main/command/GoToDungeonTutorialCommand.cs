using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;

public class GoToDungeonTutorialCommand : Command {

	[Inject]
	public int dungeonId{ get; set; }

	[Inject]
	public CrossContextData data {get; set;}
	
	[Inject]
	public ConfigManager configMgr {get;set;}

	[Inject]
	public DungeonService dungeonService {get; set;}

	public override void Execute ()
	{
		dungeonService.GoToDungeon (dungeonId, data, configMgr.UserData, false);
	}
}
