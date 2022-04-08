using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using System.Collections.Generic;
using strange.extensions.pool.api;

public class ShowInfoFbCommand : Command {

	[Inject]
	public List<InfoUserFBData> listUser { get; set;}

	[Inject]
	public InfoUserService infoManager { get; set;}

	private ClickDungeonView[] listDungeon;
	private Dictionary<int, ClickDungeonView> dungeons;

	public override void Execute ()
	{
		infoManager.SetListUser (listUser);
	}

}
