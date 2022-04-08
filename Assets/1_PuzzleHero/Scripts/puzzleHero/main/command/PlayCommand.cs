using common.service;
using strange.examples.strangerocks;
using strange.extensions.command.impl;
using UnityEngine;

public class PlayCommand : Command
{
	[Inject]
	public ServerService service { get; set; }

	[Inject]
	public IRoutineRunner Runner { get; set; }

	[Inject]
	public ConfigManager config { get; set; }

	[Inject]
	public ChestService chestService { get; set; }

	[Inject]
	public ItemService itemService { get; set; }

	[Inject]
	public LoadingManager loadManager { get; set; }

	[Inject]
	public GoToDungeonTutorialSignal goDungeonSignal { get; set; }

	[Inject]
	public ApplicationDispatcherService appService { get; set; }

	[Inject]
	public NotificationService notifService { get; set; }

	[Inject]
	public DungeonService dungeonService { get; set; }

	[Inject]
	public ShopService shopService { get; set;}

	[Inject]
	public GoPlayFlushData goPlayFlushData { get; set;}

	public override void Execute()
	{
		shopService.CheckPayment ();
		goPlayFlushData.isInGame = true;
		if (config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.START)
		{
			config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.BEGIN;
			goDungeonSignal.Dispatch(0);
			config.UserData.AddEnergy(10);
		}
		else if (config.UserData.restoreDungeonState)
		{
			//Logger.Trace(config.UserData.dungeonStateData);
			int dunId = config.UserData.dungeonStateData.battleState.dungeonId;
			int energyToRestore = dungeonService.GetEnergy(dunId);
			config.UserData.AddEnergy(energyToRestore);
			goDungeonSignal.Dispatch(dunId);
		}
		else
		{
			loadManager.SetScreen("WorldMap", false);
			Application.LoadLevel("Loading");
		}
		goPlayFlushData.AddSubjectOs ();
		// cheat seed for random
		UnityEngine.Random.seed = (int) Time.realtimeSinceStartup;
		service.connection.Server = config.general.AppStore_Validate_Url;
	}
}