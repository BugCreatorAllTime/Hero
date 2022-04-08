using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;

public class StartGameCommand : Command
{
	[Inject]
	public CreateObjModel createObjModel { get; set; }

	[Inject]
	public GameLoop gameLoop { get; set; }

	[Inject]
	public ConfigManager config { get; set; }

	[Inject]
	public DungeonStateDataSyncHandler dataSyncHandler { get; set; }

	[Inject]
	public MiniMapManager miniMapManager { get; set; }

	[Inject]
	public SoundManager soundManager { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	public override void Execute()
	{
		if (createObjModel.createTeam1Complete && createObjModel.createTeam2Complete)
		{
			createObjModel.createTeam1Complete = false;
			gameLoop.battleLogic.Init();
			gameLoop.battleLogic.Start();
			gameLoop.matchLogic.Init();

			if (config.UserData.restoreDungeonState)
			{
				dataSyncHandler.RestoreMonsterSkills();
				config.UserData.restoreDungeonState = false;
			}

			miniMapManager.CreatMiniMap();
			soundManager.PlayMusic(SoundName.MUSIC_DUNGEON);
			dungeonState.Start();
		}

	}
}
