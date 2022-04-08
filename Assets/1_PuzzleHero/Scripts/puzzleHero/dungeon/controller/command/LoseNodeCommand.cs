using Nfury.Base;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;

public class LoseNodeCommand :Command
{
	[Inject]
	public CrossContextData CrossContextData{ get; set; }
	[Inject]
	public ConfigManager config{ get; set; }
	[Inject]
	public CreateTeam2Signal createTeam2Signal{ get; set; }

	[Inject(GuiObjectName.defeatedPopup)]
	public GameObject defeatedPopup { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }
	
	public override void Execute ()
	{
		dungeonState.state = DungeonState.State.Lose;
		guiEventHandler.ShowDefeatedPopup ();
	}
}
