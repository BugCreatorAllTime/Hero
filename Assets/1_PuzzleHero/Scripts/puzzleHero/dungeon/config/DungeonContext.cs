using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.impl;
using Nfury.Base;
using strange.extensions.pool.api;
using strange.extensions.pool.impl;
using UnityEngine;
using strange.extensions.signal.impl;
using strange.examples.strangerocks;

public class DungeonContext : MVCSContext
{
	public const string BATTLE_LOGIC = "BattleGameLogic";
	public const string MATCH_LOGIC = "MatchLogic";

	public const string DUNGEON = "Dungeon";
	public const string TEXT = "TextView";
	public const string BOARD = "Board";
	public const string TESTVIEW = "View";
	public const string HUD = "Hud";
	public const string loadingProgressBar = "LoadingProgressBar";

	private const string pausePrefab = "Prefabs/GUI/Ingame/Pause";
	private const string defeatedPrefab = "Prefabs/GUI/Ingame/Defeated";
	private const string victoryPrefab = "Prefabs/GUI/Ingame/Victory";
	private const string surrenderConfirmPrefab = "Prefabs/GUI/Ingame/SurrenderConfirm";
	private const string noticePrefab = "Prefabs/GUI/Ingame/PopupNotice";
	private const string purchasePrefab = "Prefabs/GUI/Ingame/PopupPurchase";
	private const string revivePrefab = "Prefabs/GUI/Ingame/Revive";
	private const string tutorialInfoPrefab = "Prefabs/GUI/TutorialInfoUI";
	private const string tutorialUiPrefab = "Prefabs/GUI/TutorialUI";
	private const string buyMovePopupPrefab = "Prefabs/GUI/Ingame/BuyMovePopup";

	private int readyViewsCount = 0;
	private int viewToConstructCount = 0;
	private Dictionary<GameObject, Type> gameObjectsToAddView = new Dictionary<GameObject, Type>();

	public DungeonContext(MonoBehaviour contextView)
		: base(contextView)
	{
		
	}

	protected override void addCoreComponents() {
		base.addCoreComponents();
		injectionBinder.Unbind<ICommandBinder>();
		injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
	}

	protected override void mapBindings()
	{
		base.mapBindings();
		implicitBinder.ScanForAnnotatedClasses(new string[] { "strange.examples.strangerocks" });

		injectionBinder.Bind<AbstractGameLogic>().To<BattleGameLogic>().ToSingleton().ToName(BATTLE_LOGIC);
		injectionBinder.Bind<AbstractGameLogic>().To<MatchLogic>().ToSingleton().ToName(MATCH_LOGIC);
		injectionBinder.Bind<IRoutineRunner>().To<RoutineRunner>().ToSingleton();
		injectionBinder.Bind<GameLoop>().ToSingleton();
		injectionBinder.Bind<CreateObjModel>().ToSingleton();
		injectionBinder.Bind<DungeonState>().ToSingleton();
		injectionBinder.Bind<DungeonStateDataSyncHandler>().ToSingleton();

		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find("Dungeon")).ToName(DungeonContext.DUNGEON);
		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find("Board")).ToName(DungeonContext.BOARD);
		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find("TextView")).ToName(DungeonContext.TEXT);
		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find("Hud")).ToName(DungeonContext.HUD);
		injectionBinder.Bind<GameObject>().ToValue(GameObject.Find(loadingProgressBar)).ToName(loadingProgressBar);

		injectionBinder.Bind<SkillsManager>().ToSingleton();
		injectionBinder.Bind<EffectsManager>().ToSingleton();
		injectionBinder.Bind<TextFxManager>().ToSingleton();
		injectionBinder.Bind<MiniMapManager>().ToSingleton();
		injectionBinder.Bind<CharacterHpChangedSignal>().ToSingleton();
		injectionBinder.Bind<MonsterHpChangedSignal>().ToSingleton();
		injectionBinder.Bind<CharacterDefendChangedSignal>().ToSingleton();
		injectionBinder.Bind<CharacterSkillChangedSignal>().ToSingleton();
		injectionBinder.Bind<GuiEventHandler>().ToSingleton();
		injectionBinder.Bind<MonsterTurnChangedSignal>().ToSingleton();
		injectionBinder.Bind<BattlePhaseManager>().ToSingleton();
		injectionBinder.Bind<ChestModeLogic>().ToSingleton();
		injectionBinder.Bind<RemainingMovesChangedSignal>().ToSingleton();
		injectionBinder.Bind<TutorialFirstBattleLogic> ().ToSingleton ();
		injectionBinder.Bind<Handler>().ToSingleton();
		injectionBinder.Bind<DungeonAssetsRegister>().ToSingleton();
		injectionBinder.Bind<PreloadedAssets>().ToSingleton();

		commandBinder.Bind<StartSignal>().To<PrepareCommand>();
		commandBinder.Bind<CreateTeam1Signal>().To<CreateTeam1Command>();
		commandBinder.Bind<CreateTeam2Signal>().To<CreateTeam2Command>();
		commandBinder.Bind<StartGameSignal>().To<StartGameCommand>();
		commandBinder.Bind<AddActionSignal>().To<AddActionCommand>();
		commandBinder.Bind<Team1PrepareAttackSignal>().To<Team1PrepareAttackCommand>();
		commandBinder.Bind<Team1StartAttackSignal>().To<Team1StartAttackCommand>();
		commandBinder.Bind<Team1FinishAttackSignal>().To<Team1FinishActionCommand>();
		commandBinder.Bind<Team2StartAttackSignal>().To<Team2StartAttackCommand>();
		commandBinder.Bind<Team2FinishAttackSignal>().To<Team2FinishAttackCommand>();
		commandBinder.Bind<FinishBattleSignal>().To<FinishBattleCommand>();

		commandBinder.Bind<WinNodeSignal>().To<WinNodeCommand>();
		commandBinder.Bind<LoseNodeSignal>().To<LoseNodeCommand>();
		commandBinder.Bind<DisableBoardInputSignal>().To<DisableBoardInputCommand>();
		commandBinder.Bind<EnableBoardInputSignal>().To<EnableBoardInputCommand>();
		commandBinder.Bind<CharacterStartRunningSignal>().To<CharacterStartRunningCommand>();
		commandBinder.Bind<CharacterStopRunningSignal>().To<CharacterStopRunningCommand>();
		commandBinder.Bind<BoardSkillSignal>().To<BoardSkillComand>();
		commandBinder.Bind<CalculatorComboSignal>().To<CalculatorComboCommand>();
		commandBinder.Bind<TutorialStateSignal>().To<TutorialStateCommand>();
		commandBinder.Bind<UpgradeFxSignal> ().To<UpgradeFxCommand> ();

		mediationBinder.Bind<TestView>().To<TestMediator>();

		mediationBinder.Bind<HudView>().To<HudMediator>();
		mediationBinder.Bind<PauseView>().To<PauseMediator>();
		mediationBinder.Bind<DefeatedView>().To<DefeatedMediator>();
		mediationBinder.Bind<VictoryView>().To<VictoryMediator>();
		mediationBinder.Bind<NoticeView>().To<NoticeMediator>();
		mediationBinder.Bind<PurchaseView>().To<PurchaseMediator>();
		mediationBinder.Bind<SurrenderConfirmView>().To<SurrenderConfirmMediator>();
		mediationBinder.Bind<ReviveView>().To<ReviveMediator>();
		mediationBinder.Bind<TutorialInfoUIView>().To<TutorialInfoUIMediator>();
		mediationBinder.Bind<TutorialUIView>().To<TutorialUIMediator>();
		mediationBinder.Bind<BuyMovePopupView>().To<BuyMovePopupMediator>();

		Type type = typeof(Prefabs);
		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			object field = fieldInfo.GetValue(null);
			injectionBinder.Bind<IPool<GameObject>>().To<Pool<GameObject>>().ToSingleton().ToName(field.ToString());
		}

		GameObject gameObject = GameObject.FindWithTag(ObjectTag.gameInput);
		GameInput gameInput = gameObject.GetComponent<GameInput>();
		injectionBinder.Bind<GameInput>().ToValue(gameInput).ToSingleton();
	}

	protected override void postBindings()
	{
		AssetMgr assetMgr = injectionBinder.GetInstance<AssetMgr>();
		CrossContextData crossContextData = injectionBinder.GetInstance<CrossContextData>();
		crossContextData.OnNewDungeonContextConstruction();

		Type type = typeof(Prefabs);
		foreach (FieldInfo fieldInfo in type.GetFields())
		{
			object v = fieldInfo.GetValue(null);
			String name = v.ToString();
			IPool<GameObject> pool = injectionBinder.GetInstance<IPool<GameObject>>(name);
			pool.instanceProvider = new ResourceInstanceProvider(name, assetMgr);
			pool.inflationType = PoolInflationType.INCREMENT;
		}

		GameObject hudGameObject = GameObject.FindWithTag(ObjectTag.hud);

		List<ViewInstruction> instructions = new List<ViewInstruction>();
		instructions.Add(ViewInstruction.From(pausePrefab, GuiObjectName.pausePopup, typeof(PauseView)));
		instructions.Add(ViewInstruction.From(defeatedPrefab, GuiObjectName.defeatedPopup, typeof(DefeatedView)));
		instructions.Add(ViewInstruction.From(victoryPrefab, GuiObjectName.victoryPopup, typeof(VictoryView)));
		instructions.Add(ViewInstruction.From(surrenderConfirmPrefab, GuiObjectName.surrenderConfirmPopup, typeof(SurrenderConfirmView)));
		instructions.Add(ViewInstruction.From(noticePrefab, GuiObjectName.noticePopup, typeof(NoticeView)));
		instructions.Add(ViewInstruction.From(purchasePrefab, GuiObjectName.popupPurchase, typeof(PurchaseView)));
		instructions.Add(ViewInstruction.From(revivePrefab, GuiObjectName.revivePopup, typeof(ReviveView)));
		instructions.Add(ViewInstruction.From(tutorialInfoPrefab, GuiObjectName.tutorialInfoUI, typeof(TutorialInfoUIView)));
		instructions.Add(ViewInstruction.From(tutorialUiPrefab, GuiObjectName.tutorialUI, typeof(TutorialUIView)));
		instructions.Add(ViewInstruction.From(buyMovePopupPrefab, GuiObjectName.buyMovePopup, typeof(BuyMovePopupView)));

		viewToConstructCount = instructions.Count + 1;//including hudView

		gameObjectsToAddView.Add(hudGameObject, typeof(HudView));
		OnViewLoaded();
		for (int i = 0; i < instructions.Count; i++)
		{
			ConstructView(assetMgr, instructions[i]);
		}
	}

	private void LoadAdditionalAsset()
	{
		AssetMgr assetMgr = injectionBinder.GetInstance<AssetMgr>();
		DungeonAssetsRegister assetsRegister = injectionBinder.GetInstance<DungeonAssetsRegister>();
		assetsRegister.OnAllAssetLoadedEvent += OnAllAssetLoaded;
		PreloadedAssets preloadedAssets = injectionBinder.GetInstance<PreloadedAssets>();
		((DungeonAssetsRegister.IRegistration)preloadedAssets).RegisterAsset(assetsRegister);
		((DungeonAssetsRegister.IRegistration)preloadedAssets).LoadAsset(assetMgr, assetsRegister.OnAssetLoaded);
	}

	private void OnAllAssetLoaded()
	{
		base.postBindings();
		StartSignal startSignal = (StartSignal)injectionBinder.GetInstance<StartSignal>();
		startSignal.Dispatch();
	}

	private void ConstructView(AssetMgr assetMgr, ViewInstruction instruction)
	{
		assetMgr.GetAsset<GameObject>(instruction.prefabPath, delegate(GameObject go)
		{
			GameObject obj = GameObject.Instantiate(go) as GameObject;
			obj.transform.localScale = Vector3.zero;
			obj.name = instruction.nameToSet;
			injectionBinder.Bind<GameObject>().ToValue(obj).ToName(instruction.nameToSet);
			gameObjectsToAddView.Add(obj, instruction.viewType);
			OnViewLoaded();
		});
	}

	private void OnViewLoaded()
	{
		readyViewsCount++;
		if (readyViewsCount < viewToConstructCount) return;
		for (int i = 0; i < gameObjectsToAddView.Count; i++)
		{
			KeyValuePair<GameObject, Type> kvp = gameObjectsToAddView.ElementAt(i);
			GameObject go = kvp.Key;
			Type t = kvp.Value;
			go.AddComponent(t);
		}

		LoadAdditionalAsset();
	}
	
	private class ViewInstruction
	{
		public string prefabPath;
		public string nameToSet;
		public Type viewType;

		public static ViewInstruction From(string prefabPath, string nameToSet, Type viewType)
		{
			ViewInstruction instruction = new ViewInstruction();
			instruction.prefabPath = prefabPath;
			instruction.nameToSet = nameToSet;
			instruction.viewType = viewType;
			return instruction;
		}
	}
}
