
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using common.service;
using System.Collections.Generic;

public class StartMainCommand : Command
{
	[Inject]
	public ServerService service { get; set; }

	[Inject]
	public IRoutineRunner Runner{ get; set; }

	[Inject]
	public ConfigManager config{get;set;}

	[Inject]
	public ChestService chestService {get;set;}

	[Inject]
	public ItemService itemService {get;set;}
	

	[Inject]
	public LoadingManager loadManager { get; set; }

	[Inject]
	public GoToDungeonTutorialSignal goDungeonSignal { get; set;}

	[Inject]
	public ApplicationDispatcherService appService{get;set;}

	[Inject]
	public NotificationService notifService {get; set;}

	[Inject]
	public FbHandler fbHandler { get; set; }

	[Inject]
	public ShopService shopService { get; set; }

	[Inject]
	public GA ga { get; set; }


	public override void Execute ()
	{
		// cheat seed for random
		UnityEngine.Random.seed = (int)Time.realtimeSinceStartup;
		service.connection.Server = config.general.AppStore_Validate_Url;
		
		fbHandler.Init();
		shopService.BuyGemHandler += HandleGemBuying;
	}

	private void HandleGemBuying(int gemId, PaymentErrorCode error) {
		if (error == PaymentErrorCode.OK) {
			ga.TrackGemBuy(gemId);
		}
	}
}
