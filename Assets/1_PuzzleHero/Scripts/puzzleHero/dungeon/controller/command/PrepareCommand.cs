using Nfury.Base;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
public class PrepareCommand : Command
{
	private const string pause = "Prefabs/GUI/Ingame/Pause";

	[Inject]
	public CreateTeam1Signal createTeam1Signal { get; set; }
	[Inject]
	public CreateTeam2Signal createTeam2Signal { get; set; }
	[Inject]
	public CrossContextData CrossContextData { get; set; }
	[Inject(DungeonContext.DUNGEON)]
	public GameObject DungeonObj { get; set; }

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject]
	public ConfigManager config { get; set; }

	[Inject]
	public DungeonStateDataSyncHandler dungeonStateDataSyncHandler { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject(DungeonContext.HUD)]
	public GameObject hudView { get; set; }

	public override void Execute()
	{
//		Logger.Trace(GetType(), "::SetupViews()");
//		SetupViews();

		hudView.gameObject.SetActive(true);
		GameObject.Find("Panel").SetActive(false);

		if (config.UserData.restoreDungeonState)
		{
			//Logger.Trace(GetType(), "::restoreDunState");
			dungeonStateDataSyncHandler.RestoreCrossContextData(CrossContextData);
			dungeonStateDataSyncHandler.RestoreDungeonState(dungeonState);
			dungeonStateDataSyncHandler.RestoreBoard((MatchLogic) matchLogic);
		}
		else
		{
			CrossContextData.monsterIndexInDungeon = 0;
		}
		createTeam1Signal.Dispatch(false);
		createTeam2Signal.Dispatch(0);

	}

	private void SetupViews()
	{
		GameObject obj = new GameObject();
		obj.transform.parent = DungeonObj.transform;
		obj.name = DungeonContext.TESTVIEW;
		obj.AddComponent<TestView>();

//		obj = GameObject.FindWithTag(ObjectTag.hud);
//		obj.AddComponent<HudView>();

//		obj = GameObject.Instantiate(assetMgr.GetAsset<GameObject>(pause)) as GameObject;
//		obj.name = GuiObjectName.pausePopup;
//		injectionBinder.Bind<GameObject>().ToValue(obj).ToName(GuiObjectName.pausePopup);
//		obj.AddComponent<PauseView>();
//		obj.SetActive(false);
	}
}
