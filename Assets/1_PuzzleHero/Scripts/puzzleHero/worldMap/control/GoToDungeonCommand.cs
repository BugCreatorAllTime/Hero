using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;

public class GoToDungeonCommand : Command {

	[Inject]
	public int dungeonId{ get; set; }

	[Inject]
	public string nameDungeon { get; set;}

	[Inject]
	public bool check { get; set;}

	[Inject]
	public CrossContextData data {get; set;}

	[Inject]
	public ConfigManager configMgr {get;set;}

	[Inject]
	public LoadInfoTab loadTab { get; set; }

	[Inject]
	public DungeonService dungeonService {get; set;}

	public override void Execute ()
	{
		data.nameDungeon = nameDungeon;
		data.dungeonChoose = dungeonId;
		switch(dungeonService.GoToDungeon(dungeonId, data, configMgr.UserData, check))
		{
		case ErrorCode.OK:
			loadTab.CheckTypeTutorial(nameDungeon);
			break;
		case ErrorCode.NOT_ENOUGH_ENERGY:
			loadTab.ShowNoticePurchaseEnergy();
			break;
		case ErrorCode.NOT_ENOUGH_SLOT:
			loadTab.ShowNoticePurchaseSlot();
			break;
		case ErrorCode.NOT_WEAR_WEAPON:
			loadTab.ShowNoticeGoDungeon(configMgr.text.NotWearWeapon);
			break;
		case ErrorCode.NOT_WEAR_ARMOR:
			loadTab.ShowNoticeGoDungeon(configMgr.text.NotWearArmor);
			break;
		case ErrorCode.NOT_WEAR_SHIELD:
			loadTab.ShowNoticeGoDungeon(configMgr.text.NotWearShield);
			break;
		default:
			loadTab.ShowNotice(dungeonService.GoToDungeon(dungeonId, data, configMgr.UserData, false).ToString());
			break;
		}
	}

}

