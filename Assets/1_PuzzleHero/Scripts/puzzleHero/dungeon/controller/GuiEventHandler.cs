using System;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Nfury.Base;
using strange.extensions.pool.api;
using UnityEngine;
using System.Collections;
using strange.examples.strangerocks;
using LitJson;

public class GuiEventHandler
{
	public Dictionary<string, GameObject> cachedGameObjects;

	[Inject]
	public AssetMgr mgr{get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public CrossContextData contextData{ get; set;}

	[Inject]
	public EffectsManager effectMng{get; set;}

	[Inject(GuiObjectName.pausePopup)]
	public GameObject pausePopup { get; set; }

	[Inject(GuiObjectName.noticePopup)]
	public GameObject noticePopup { get; set; }

	[Inject(PrefabWorldMap.content)]
	public IPool<GameObject> contentPool { get; set; }

	[Inject]
	public EnableBoardInputSignal enableBoardInputSignal { get; set; }

	[Inject]
	public DisableBoardInputSignal disableBoardInputSignal { get; set; }

	[Inject(Prefabs.itemContent)]
	public IPool<GameObject> itemContentPool { get; set; }

	[Inject]
	public DungeonService dungeonService { get; set; }

	[Inject]
	public ItemService itemService { get; set; }

	[Inject]
	public ChestService chestService { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public CreateTeam1Signal createTeam1Signal { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public ShopService shopService { get; set; }

	[Inject]
	public LoadingManager loadManager{ get; set;}

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public TutorialStateSignal tutStateSignal { get; set;}

	[Inject]
	public TutorialFirstBattleLogic tutLogic { get; set;}

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleGameLogic { get; set; }

	[Inject]
	public ChestModeLogic chestLogic { get; set;}

	[Inject]
	public HudView hudView {get;set;}

	[Inject]
	public SoundManager soundMng {get;set;}

	[Inject]
	public GA ga { get; set; }

	[Inject]
	public CrossContextData crossContextData { get; set; }

	public delegate void OnGuiBeingDisplayed(bool displayed);
	public event OnGuiBeingDisplayed OnGuiBeingDisplayedEvent;

	private Stack<Dictionary<string, GameObject>> popupStack;
	private bool isGuiBeingDisplayed = false;
	private GameObject fx;
	private GameObject fxLevelUp;
	private Dictionary<string, string> ignoredLayerCheckingButtons;
	private bool clickTutorial = false;
	private HomeTownTutorialData tutoData;
	private int typeTut;
	private const int CREATE_NEW = 0;
	private const int TEXT_CONTINUE = 101;
	private const int RETURN_POOL = 103;
		
	[PostConstruct]
	public void InitObjects()
	{
		cachedGameObjects = new Dictionary<string, GameObject>();
		popupStack = new Stack<Dictionary<string, GameObject>>();
		ignoredLayerCheckingButtons = new Dictionary<string, string>();
		Component[] buttons = pausePopup.GetComponentsInChildren(typeof(UIButton));
		for (int i = 0; i < buttons.Length; i++) {
			ignoredLayerCheckingButtons.Add(buttons[i].gameObject.name, buttons[i].gameObject.name);
		}
		ignoredLayerCheckingButtons.Add(GuiObjectName.pauseButtonName, GuiObjectName.pauseButtonName);
	}

	public void HandleClick(string buttonName)
	{
//		Logger.Trace("pauseEventHandler handleClick ", buttonName);
//		Logger.Trace("onclick ", buttonName);
		string[] buttonNames;
		if (!IsLayerCheckingIgnoredFor(buttonName))
		{
			if (!IsOnTop(buttonName)) {
				Logger.Trace(buttonName, " is not on top, break;");
				return;
			}
		}
		switch (buttonName)
		{
			case GuiObjectName.pauseButtonName:
				if (dungeonState.state != DungeonState.State.Playing) break;
				if (!pausePopup.activeInHierarchy && configManager.UserData.currentStepTownTutorial >  TutorialFirstBattleLogic.START)
				{
					pausePopup.SetActive(true);
					SetGuiBeingDisplayed(true);
					FindGameObject(GuiObjectName.pauseBoard).SetActive(true);
					FindGameObject(GuiObjectName.pauseSurrender).SetActive(false);
				}
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.resume:
				pausePopup.SetActive(false);
				SetGuiBeingDisplayed(false);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.confirmResume:
				HideItems();
				pausePopup.SetActive(false);
				SetGuiBeingDisplayed(false);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.confirmSurrender:
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				soundMng.PauseMusic();
				loadManager.SetScreen("WorldMap");
				Application.LoadLevel("Loading");
				TrackSpendTime();
				break;
			case GuiObjectName.surrender:
				FindGameObject(GuiObjectName.pauseBoard).SetActive(false);
				FindGameObject(GuiObjectName.pauseSurrender).SetActive(true);
				ShowItems(FindGameObject(GuiObjectName.grid));
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.defeatedPopup_ReviveButton:
				ShowPopup(GuiObjectName.revivePopup);
				FindGameObject(GuiObjectName.reviveDisabel).GetComponent<UISprite>().alpha = 0.7f;
				UILabel dLabel = FindGameObject (GuiObjectName.revivePopup_ReviveLabel).GetComponent<UILabel>();
				dLabel.text = "REVIVE  "+shopService.CalculateRevivePrice(dungeonState.ReviveNumber);
				fx.SetActive(false);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.defeatedPopup_SurrenderButton:
				if(configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.START)
				{
					soundMng.PauseMusic();
					loadManager.SetScreen("WorldMap");
					Application.LoadLevel("Loading");
				} else {
					ShowPopup(GuiObjectName.surrenderConfirmPopup);
					FindGameObject(GuiObjectName.surrenderDisable).GetComponent<UISprite>().alpha = 0.7f;
					fx.SetActive(false);
				}
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.surrenderConfirmPopup_NoButton:
				HidePopup(GuiObjectName.surrenderConfirmPopup);
				fx.SetActive(true);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.surrenderConfirmPopup_SurrenderButton:
				if(configManager.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START)
					configManager.UserData.SetDefeat(true);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				soundMng.PauseMusic();
				loadManager.SetScreen("WorldMap");
				Application.LoadLevel("Loading");
				TrackSpendTime();
				break;
			case GuiObjectName.revivePopup_NoButton:
				fx.SetActive(true);
				HidePopup(GuiObjectName.revivePopup);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.revivePopup_ReviveButton:
				if (shopService.ProcessRevive(dungeonState.ReviveNumber))
				{
					dungeonState.Revive();
					HidePopup(GuiObjectName.revivePopup);
					HidePopup(GuiObjectName.defeatedPopup);
					SetGuiBeingDisplayed(false);
					ReviveCharacter();
					fx.SetActive(false);
				}
				else
				{
					HidePopup(GuiObjectName.revivePopup);
					ShowPopup(GuiObjectName.noticePopup);
					FindGameObject(GuiObjectName.noticeContent).GetComponent<UILabel>().text =
					configManager.text.NotGemAndBuy;
					FindGameObject(GuiObjectName.noticeButton).GetComponent<UIButton>().onClick.Clear();
					FindGameObject (GuiObjectName.noticeNo).gameObject.SetActive (true);
					FindGameObject (GuiObjectName.noticeYes).gameObject.SetActive (true);
				}
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.victoryPopup_TownButton:
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				soundMng.PauseMusic();
				loadManager.SetScreen("WorldMap");
				Application.LoadLevel("Loading");
				TrackSpendTime();
				break;
			case GuiObjectName.tutButton:
				CloseTutorial(FindGameObject (GuiObjectName.tutorialInfoUI));
				break;
			case GuiObjectName.tutChaButton:
				CloseTutFirst(FindGameObject (GuiObjectName.tutorialUI));
				break;
			case GuiObjectName.victoryPopup_NextButton:
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				NextDungeon();
				break;
			case GuiObjectName.noticeButton:
				HidePopup(GuiObjectName.noticePopup);
				if(fx != null)
					fx.SetActive(true);
				break;
			case GuiObjectName.noticeNo:
				noticePopup.GetComponent<NoticeView>().SetButton();
				HidePopup(GuiObjectName.noticePopup);
				if(fx != null)
					fx.SetActive(true);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.noticeYes:
				noticePopup.GetComponent<NoticeView>().SetButton();
				HidePopup(GuiObjectName.noticePopup);
				ShowPurchase();
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.purchaseQuit:
				HidePopup(GuiObjectName.popupPurchase);
				ShowPopup(GuiObjectName.defeatedPopup);
				if(fx != null)
					fx.SetActive(true);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.purchaseButtonFirst:
				BuyGem(configManager.shopCfg.gem.ElementAt(0).Value);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				ShowPopup(GuiObjectName.defeatedPopup);
				break;
			case GuiObjectName.purchaseButtonSecond:
				BuyGem(configManager.shopCfg.gem.ElementAt(1).Value);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				ShowPopup(GuiObjectName.defeatedPopup);
				break;
			case GuiObjectName.purchaseButtonThird:
				BuyGem(configManager.shopCfg.gem.ElementAt(2).Value);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				ShowPopup(GuiObjectName.defeatedPopup);
				break;
			case GuiObjectName.purchaseButtonFourth:
				BuyGem(configManager.shopCfg.gem.ElementAt(3).Value);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				ShowPopup(GuiObjectName.defeatedPopup);
				break;
			case GuiObjectName.purchaseButtonFifth:
				BuyGem(configManager.shopCfg.gem.ElementAt(4).Value);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				ShowPopup(GuiObjectName.defeatedPopup);
				break;
			case GuiObjectName.buyMovePre:
				AddMove(-1);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.buyMoveNext:
				AddMove(1);
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				break;
			case GuiObjectName.buyMoveBtNo:
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				SetGuiBeingDisplayed(false);
				HidePopup(GuiObjectName.buyMovePopup);
				((BattleGameLogic)battleGameLogic).Win();
				break;
			case GuiObjectName.buyMoveBtOk:
				soundMng.PlaySound(SoundName.BUTTON_CLICK);
				BuyMove();
				break;
		}
	}

	public void HandleValueChange(UISlider slider)
	{
		switch (slider.gameObject.name)
		{
		case GuiObjectName.musicSlider:
			configManager.UserData.musicValue = slider.value;
			soundMng.SetVolumeMusic((float)slider.value);
			break;
		case GuiObjectName.soundSlider:
			configManager.UserData.soundValue = slider.value;
			break;
		}
	}

	private void TrackSpendTime()
	{
		float duration = Time.time - dungeonState.startTime;
		ga.TrackTimeSpendInDungeon((int) duration);
	}

	private void NextDungeon()
	{
		int id = contextData.dungeonId;
		mgr.GetAsset<TextAsset>("Config/WorldMapCfg.json", delegate (TextAsset ta){
			string jsonText = ta.text;
			JsonData data = JsonMapper.ToObject(jsonText);
			int numberOfDungeon = (int)data["dungeon"].Count;
			for (int i = 1; i < numberOfDungeon; i++)
			{
				if( id == (int)data["dungeon"][i-1]["level"])
				{
					id = (int)data["dungeon"][i]["level"];
					break;
				}
			}
			ErrorCode error = dungeonService.GoToDungeon (id, contextData, configManager.UserData, true);
			switch(error)
			{
			case ErrorCode.OK:
				TrackSpendTime();
				break;
			case ErrorCode.NOT_ENOUGH_ENERGY:
				ShowNotice(configManager.text.NotEnoughEnegry);
				break;
			case ErrorCode.NOT_ENOUGH_SLOT:
				ShowNotice(configManager.text.NotEnoughSlot);
				break;
			default:
				ShowNotice(error.ToString());
				break;
			}
		});
	}

	private void ShowNotice(string text)
	{
		ShowPopup(GuiObjectName.noticePopup);
		FindGameObject (GuiObjectName.noticeNo).gameObject.SetActive (false);
		FindGameObject (GuiObjectName.noticeYes).gameObject.SetActive (false);
		if(fx != null)
			fx.SetActive(false);
		FindGameObject(GuiObjectName.noticeDisable).GetComponent<UISprite>().alpha = 0.7f;
		FindGameObject(GuiObjectName.noticeContent).GetComponent<UILabel>().text = text;
	}

	
	private void AddRewardToUser()
	{
		UserData mUser = configManager.UserData;
		mUser.AddGold (dungeonState.Gold());
		mUser.GainExp (dungeonState.GetExp());
		for(int i = 0; i < dungeonState.itemList.Count; i++)
		{
			if(dungeonState.itemList[i].item != null)
			{
				configManager.UserData.SetItemUpdate(true);
				mUser.Inventory.AddItem(dungeonState.itemList[i].item);
			}
		}
		if(dungeonState.chestID > 0)
		{
			chestService.AddChest(dungeonState.chestID.GetValue(), ChestSource.NONE_GEM, mUser);
		}
	}

	private void ReviveCharacter() {
		((MatchLogic)matchLogic).DisableBoardInput();
		createTeam1Signal.Dispatch(true);
		soundMng.PlayMusic (SoundName.MUSIC_DUNGEON);
		GameObject fxGameObject = effectsManager.CreateSpineEffect(Effects.resurrect, FxAnimationName.active, OnReviveAnimationEnd);
		fxGameObject.transform.localPosition = new Vector2(135, 590);
		soundMng.PlaySound (SoundName.CHARACTER_REVIVE);
		ga.TrackRevive();
	}

	private void OnReviveAnimationEnd() {
		dungeonState.state = DungeonState.State.Playing;
		battlePhaseManager.CharacterRevive();
//		createTeam1Signal.Dispatch(true);
	}

	private bool IsLayerCheckingIgnoredFor(string buttonName)
	{
		string button = null;
		ignoredLayerCheckingButtons.TryGetValue(buttonName, out button);
		return button != null;
	}

	public void ShowPopup(string popupName)
	{
		GameObject popup = FindGameObject(popupName);
		popup.SetActive(true);
		Component[] buttons = popup.GetComponentsInChildren(typeof (UIButton));
		string[] buttonNames = new string[buttons.Length];
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonNames[i] = buttons[i].gameObject.name;
		}
		PushToStack(buttonNames);
	}

	public void HidePopup(string popupName)
	{
		PopFromStack();
		FindGameObject(popupName).SetActive(false);
	}

	private void PushToStack(string[] buttonNames)
	{
		Dictionary<string, GameObject> item = new Dictionary<string, GameObject>();
		for (int i = 0; i < buttonNames.Length; i++)
		{
			GameObject go = FindGameObject(buttonNames[i]);
			item[go.name] = go;
		}
		popupStack.Push(item);
	}

	private bool IsOnTop(string gameObjectName)
	{
		GameObject go = null;
		if (popupStack.Count <= 0)
		{
			return false;
		}
		popupStack.Peek().TryGetValue(gameObjectName, out go);
		return go != null;
	}

	private Dictionary<string, GameObject> PopFromStack()
	{
		return popupStack.Pop();
	}

	private void ShowItems(GameObject grid)
	{
		List<KeyValuePair<int, int>> defeatedMonsters = dungeonState.GetDefeatedMonsters();
		for(int i = 0; i < grid.transform.childCount; i++)
		{
			GameObject go = grid.transform.GetChild(i).gameObject;
			go.SetActive (false);
			itemContentPool.ReturnInstance(go);
		}
		for(int i = 0; i < dungeonState.itemList.Count; i++)
		{
			FillDataToItem(dungeonState.itemList[i].item, grid);
		}
		int itemCount = 0;
		for (int i = dungeonState.IndexStop(); i < defeatedMonsters.Count; i++)
		{
			if(configManager.MonsterCfg.GetMonsterCfgData(defeatedMonsters[i].Key).Type >= 0)
			{
				Reward itemData;
				itemData = dungeonService.DropRewardFromMonster(i, defeatedMonsters[i].Value, contextData.dungeonId);
				dungeonState.NextItemGet();
				dungeonState.GainExp(itemData.exp);
				dungeonState.CollectGold(itemData.gold);
				FillDataToItem(itemData.item, grid);
				dungeonState.itemList.Add(itemData);
				if (itemData.item != null)
				{
					itemCount++;
					configManager.UserData.SetItemUpdate(true);
				}
			} else {
				dungeonState.chestID = defeatedMonsters[i].Key;
				ChestCfgImplement chestImp = configManager.chestCfg.GetChestCfg(defeatedMonsters[i].Key);
				FillDataToChest(grid, chestImp);
			}

		}
		int count = 2 - grid.transform.childCount;
		if(count > 0)
		{
			FillDataNull(grid);
		}
		grid.GetComponent<UIGrid> ().sorting = UIGrid.Sorting.Alphabetic;
		grid.GetComponent<UIGrid>().Reposition();
		ga.TrackEquipmentPick(crossContextData.dungeonId, itemCount);
	}

	private GameObject PoolObject(GameObject grid)
	{
		GameObject itemContent = CreatePoolGameObject(itemContentPool);
		itemContent.transform.parent = grid.transform;
		itemContent.transform.localScale = Vector3.one;
		itemContent.transform.localPosition = Vector3.zero;
		itemContent.transform.Find("Content").gameObject.AddComponent<UIDragScrollView>();
		return itemContent;
	}

	private void FillDataToChest(GameObject grid, ChestCfgImplement chest)
	{
		GameObject itemContent = PoolObject(grid);
		itemContent.name = "aChest";
		ItemContentManager content = itemContent.GetComponent<ItemContentManager>();
		content.icon.spriteName = chest.Icon;
		content.nameItem.text = chest.Name;
		content.nameItem.color = Color.white;
		content.nameItem.width = 200;
		content.contentIcon.gameObject.SetActive (true);
		content.nameItem.height = 30;
		content.nameItem.transform.localPosition = new Vector3(40, -10, 0);
		content.iconCoin.gameObject.SetActive(false);
		content.iconArmor.gameObject.SetActive(false);
		content.iconDame.gameObject.SetActive(false);
		content.iconHp.gameObject.SetActive(false);
		content.iconWeight.gameObject.SetActive(false);
		content.theme.spriteName = configManager.text.NormalItem;
		content.buttonSell.gameObject.SetActive(false);
		content.btInfo.gameObject.SetActive(false);
		content.iconWeaponSet.gameObject.SetActive (false);
		content.iconArmorSet.gameObject.SetActive (false);
		content.iconShieldSet.gameObject.SetActive (false);
		content.btContent.transform.GetComponent<BoxCollider>().enabled = true;
	}

	private void FillDataToItem(ItemBaseData item, GameObject grid)
	{
		if(item != null)
		{
			GameObject itemContent = PoolObject(grid);
			Utils.FillDataItem(itemContent, item, itemService, configManager);
			ItemContentManager content = itemContent.GetComponent<ItemContentManager>();
			content.btInfo.gameObject.SetActive(false);
			content.buttonSell.gameObject.SetActive(false);
			content.btContent.transform.GetComponent<BoxCollider>().enabled = true;
			itemContent.name = "bItem";
		}
	}

	private void FillDataNull(GameObject grid)
	{
		GameObject item = Utils.ButtonNullSlot(grid.gameObject,contentPool,"tabslot",0,-30,configManager.text.NullItem, false);
	}

	private void HideItems()
	{
		GameObject grid = FindGameObject(GuiObjectName.grid);
		for (int i = 0; i < grid.transform.childCount; i++)
		{
			GameObject child = grid.transform.GetChild(i).gameObject;
			itemContentPool.ReturnInstance(child);
		}
	}

	public void CacheObject(GameObject go) {
		if (go == null) {
			return;
		}
		GameObject cachedObject = null;
		cachedGameObjects.TryGetValue(go.name, out cachedObject);
		if (cachedObject == null) {
//			Logger.Trace("cache ", go.name);
			cachedGameObjects[go.name] = go;
		}
	}

	public bool IsGuiBeingDisplayed()
	{
		return isGuiBeingDisplayed;
	}

	public void SetGuiBeingDisplayed(bool isBeingDisplayed)
	{
		this.isGuiBeingDisplayed = isBeingDisplayed;
		if (OnGuiBeingDisplayedEvent != null)
		{
			OnGuiBeingDisplayedEvent(isBeingDisplayed);
		}
	}

	private GameObject FindGameObject(string name)
	{
		GameObject go = null;
		cachedGameObjects.TryGetValue(name, out go);
		if (go == null)
		{
			go = GameObject.Find(name);
			cachedGameObjects[name] = go;
		}
		return go;
	}

	private GameObject FindChild(GameObject parent, string childName)
	{
		GameObject go = null;
		cachedGameObjects.TryGetValue(childName, out go);
		if (go == null) {
			go = parent.transform.Find(childName).gameObject;
			cachedGameObjects[childName] = go;
		}
		return go;
	}

	public void ShowDefeatedPopup()
	{
		ShowPopup(GuiObjectName.defeatedPopup);
		SetGuiBeingDisplayed(true);
		FindGameObject (GuiObjectName.defeatedDisable).GetComponent<UISprite>().alpha = 0.7f;
		UILabel dLabel = FindGameObject (GuiObjectName.defeatedPopup_ReviveLabel).GetComponent<UILabel>();
		dLabel.text = "REVIVE  "+shopService.CalculateRevivePrice(dungeonState.ReviveNumber);
		ShowItems(FindGameObject(GuiObjectName.gridDefeated));
		UILabel goldLabel = FindGameObject (GuiObjectName.defeatedGold).GetComponent<UILabel>();
		goldLabel.text = " "+dungeonState.Gold();
		UIProgressBar progress = FindGameObject (GuiObjectName.defeatedExpProgress).GetComponent<UIProgressBar>();
		float expProgress = (float)configManager.UserData.levelPercent;
		progress.value = expProgress;
		UILabel expAdd = FindGameObject (GuiObjectName.defeatedExp).GetComponent<UILabel>();
		expAdd.text = " "+dungeonState.GetExp();
		CreateFX ("Animation/Fx/Defeat/Defeat.ske", o =>
		{
			fx = o;
			fx.name = "defeatFx";
			fx.transform.localPosition = new Vector3(0, 150, 0);
		});
		
		soundMng.PlayMusic (SoundName.LOSE);
		if(configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.START)
		{
			FindGameObject(GuiObjectName.defeatedPopup_ReviveButton).SetActive(false);
			FindGameObject(GuiObjectName.defeatedPopup_SurrenderButton).transform.localPosition = new Vector3(15,-440,0);
		}
	}

	public void ShowVictoryPopUp()
	{
		ShowPopup(GuiObjectName.victoryPopup);
		SetGuiBeingDisplayed(true);		
		ShowItems(FindGameObject(GuiObjectName.gridVictory));
		FindGameObject (GuiObjectName.victoryDisable).GetComponent<UISprite>().alpha = 0.7f;
		UILabel dLabel = FindGameObject (GuiObjectName.victoryGold).GetComponent<UILabel>();
		UIProgressBar progress = FindGameObject (GuiObjectName.expProgress).GetComponent<UIProgressBar>();
		float expBefore = (float)configManager.UserData.levelPercent;
		progress.value = expBefore;
		int levelBefore = configManager.UserData.currentLevel;
		UILabel expAdd = FindGameObject (GuiObjectName.expAdd).GetComponent<UILabel>();
		AddRewardToUser ();
		CreateFX ("Animation/Fx/Victory/Victory.ske", o =>
		{
			fx = o;
			fx.name = "VictoryFx";
			fx.transform.localPosition = new Vector3(0, 250, 0);
		});
		
		soundMng.PlayMusic (SoundName.WIN);
		routineRunner.StartCoroutine(UpdateExp(levelBefore,progress));
		routineRunner.StartCoroutine(UpdateLabel(dLabel,"+ ",0,dungeonState.Gold(),	3f));
		routineRunner.StartCoroutine(UpdateLabel(expAdd,"+ ",0,dungeonState.GetExp(),3f));
	}

	private IEnumerator UpdateLabel(UILabel label, string s, float start, float end, float duration)
	{
		float passedTime = 0;
		float numb = start;
		while (passedTime < duration)
		{
			passedTime += Time.deltaTime;
			label.text = s + (int)numb;
			if(numb < end)
			{
				numb += (end-start)/duration*Time.deltaTime;
			} else {
				label.text = s + end;
			}
			yield return new WaitForEndOfFrame();
		}
		label.text = s + end;
	}

	private IEnumerator UpdateExp(int levelBefore, UIProgressBar progress)
	{
		int subLevel = configManager.UserData.currentLevel - levelBefore;
		float addTime = 0;
		if(subLevel > 0) addTime = 0.5f;
		for(int i = 0; i < subLevel; i++)
		{
			TweenParms parms = new TweenParms();
			parms.Prop("value", 1).Ease(EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(SetValue), progress);
			HOTween.To(progress, 0.5f, parms);
			yield return new WaitForSeconds(0.5f);
		}
		TweenParms parms2 = new TweenParms();
		parms2.Prop("value", configManager.UserData.levelPercent).Ease(EaseType.EaseOutSine);
		HOTween.To(progress, 2.5f+addTime, parms2);
	}

	private void SetValue(TweenEvent tweenEvent)
	{
		((UIProgressBar)tweenEvent.parms[0]).value = 0;
		fx = effectMng.CreateSpineEffect("Animation/Fx/LevelUp/level up.ske", "Active", null, false, null, false);
		fx.name = "LevelUp";
		fx.transform.parent = FindGameObject(GuiObjectName.expAdd).transform;
		fx.transform.localScale = new Vector3(240, 336, 336);
		fx.transform.localPosition = new Vector3(92627, 2619, 0);
		soundMng.PlaySound (SoundName.LEVEL_UP);
//		fx.transform.localPosition = new Vector3 (415-MatchLogic.offsetX, 550, 0);

	}

	public GameObject CreateFX(string spin, Action<GameObject> onFxCreated)
	{
		bool[] active = {false, true};
		string[] nameAnimation = {"Begin","Idle"};
		GameObject o = effectMng.CreateLoopSpineEffct (spin, nameAnimation,active,null);
		onFxCreated(o);
		return null;
	}

	GameObject CreatePoolGameObject(IPool<GameObject> poolObject)
	{
		GameObject contentObject = poolObject.GetInstance();
		contentObject.SetActive(true);
		return contentObject;
	}

	private void ShowPurchase()
	{
		HidePopup (GuiObjectName.defeatedPopup);
		ShowPopup(GuiObjectName.popupPurchase);
		PopupPurchaseContent pContent = FindGameObject(GuiObjectName.popupPurchase).GetComponent<PopupPurchaseContent>();
		pContent.disable.alpha = 0.7f;
		bool check = true;
		ShopItemCfg shopItemTest = configManager.shopCfg.gem.ElementAt (0).Value;
		if(shopItemTest.RM > 0)
		{
			check = shopItemTest.RM < configManager.shopCfg.gem.ElementAt (configManager.shopCfg.gem.Count - 1).Value.RM;
		}
		for(int i = 0; i < configManager.shopCfg.gem.Count; i++)
		{
			ShopItemCfg shopItem = configManager.shopCfg.gem.ElementAt(Cal(i,configManager.shopCfg.gem.Count,check)).Value;
			pContent.name[i].text = shopItem.Name;
			pContent.description[i].text = shopItem.Description;
			pContent.itemShop[i] = shopItem;
			if(shopItem.RM > 0)
			{
				pContent.price[i].text = shopItem.RM.ToString();
				pContent.iconGem[i].spriteName = "GTokenLogo";
				pContent.iconGem[i].width = 25;
				pContent.iconGem[i].height = 25;
			}
		}
	}

	int Cal(int i, int count, bool check)
	{
		if(check)
		{
			return i;
		} else {
			return count - i - 1;
		}
	}

	public void ShowBuyMovePopup()
	{
		ShowPopup(GuiObjectName.buyMovePopup);
		SetGuiBeingDisplayed(true);		
		FindGameObject (GuiObjectName.buyMoveDisable).GetComponent<UISprite>().alpha = 0.7f;
		int numberMove = FindGameObject (GuiObjectName.buyMovePopup).GetComponent<BuyMovePopupView>().numberMove;
		FindGameObject (GuiObjectName.buyMoveCost).GetComponent<UILabel>().text = shopService.CalculateMovePrice(numberMove).ToString();
		FindGameObject (GuiObjectName.buyMoveNumber).GetComponent<UILabel>().text = numberMove.ToString();
	}

	private void AddMove(int number)
	{
		int numberMove = FindGameObject (GuiObjectName.buyMovePopup).GetComponent<BuyMovePopupView>().numberMove;
		if(numberMove + number > 0)
		{
			numberMove += number;
			FindGameObject (GuiObjectName.buyMovePopup).GetComponent<BuyMovePopupView>().numberMove = numberMove;
			FindGameObject (GuiObjectName.buyMoveCost).GetComponent<UILabel>().text = shopService.CalculateMovePrice(numberMove).ToString();
			FindGameObject (GuiObjectName.buyMoveNumber).GetComponent<UILabel>().text = numberMove.ToString();
		}
	}

	private void BuyMove()
	{
		int numberMove = FindGameObject (GuiObjectName.buyMovePopup).GetComponent<BuyMovePopupView>().numberMove;
		if (shopService.ProcessBuyMove(numberMove))
		{
			FindGameObject (GuiObjectName.buyMovePopup).GetComponent<BuyMovePopupView>().numberMove = 1;
			chestLogic.AddMove(numberMove);
			chestLogic.SetRemainingMoves();
			HidePopup(GuiObjectName.buyMovePopup);
			SetGuiBeingDisplayed(false);
		}
		else
		{
			ShowPopup(GuiObjectName.noticePopup);
			FindGameObject(GuiObjectName.noticeContent).GetComponent<UILabel>().text =
				configManager.text.NotGemAndBuy;
			FindGameObject(GuiObjectName.noticeButton).GetComponent<UIButton>().onClick.Clear();
			FindGameObject (GuiObjectName.noticeNo).gameObject.SetActive (true);
			FindGameObject (GuiObjectName.noticeYes).gameObject.SetActive (true);
		}
	}

	public void ShowTutorial(HomeTownTutorialData tutoData, int type)
	{
		this.typeTut = type;
		this.tutoData = tutoData;
		clickTutorial = false;
		ShowPopup(GuiObjectName.tutorialInfoUI);
		SetGuiBeingDisplayed (true);
		GameObject go = FindGameObject (GuiObjectName.tutorialInfoUI); 
		SetNameContent sContent = go.GetComponent<SetNameContent>();
		tutLogic.CreateBlackObject (sContent.show.transform);
		sContent.number.gameObject.SetActive(false);
		sContent.bg.pivot = UIWidget.Pivot.Top;
		sContent.name.pivot = UIWidget.Pivot.Top;
		sContent.name.text = tutoData.Description;
		sContent.bg.transform.localPosition = new Vector3 (tutoData.TalkPosition[0],tutoData.TalkPosition[1],0);
		sContent.bg.width = tutoData.TalkSize [0];
		sContent.bg.height = tutoData.TalkSize [1];
		sContent.name.width = tutoData.TextSize [0];
		sContent.name.height = tutoData.TextSize [1];
		sContent.bg.transform.localScale = Vector3.zero;
		sContent.bg.pivot = UIWidget.Pivot.Center;
		sContent.name.pivot = UIWidget.Pivot.Center;
		sContent.name.transform.localPosition = new Vector3 (tutoData.TextPosition[0],tutoData.TextPosition[1],0);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine).OnComplete(new TweenDelegate.TweenCallbackWParms(ClickTutorial),go);
		HOTween.To(sContent.bg.transform, 0.5f, parms);

		sContent.name.transform.localScale = Vector3.zero;
		TweenParms parms2 = new TweenParms();
		parms2.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
		HOTween.To(sContent.name.transform, 0.5f, parms2);
	}

	public void TutorialShow(HomeTownTutorialData tutoData)
	{
		if(!FindGameObject(GuiObjectName.tutorialUI).activeInHierarchy)
		{
			this.tutoData = tutoData;
			clickTutorial = false;
			ShowPopup(GuiObjectName.tutorialUI);
			SetGuiBeingDisplayed (true);
			GameObject go = FindGameObject (GuiObjectName.tutorialUI); 
			SetNameContent sContent = go.GetComponent<SetNameContent>();
			sContent.arrow.gameObject.SetActive(false);
			tutLogic.CreateBlackObject (sContent.list.transform, tutoData);
			sContent.character.gameObject.SetActive (true);
			if(tutoData.EventName == CREATE_NEW)
				sContent.character.transform.localScale = Vector3.zero;
			sContent.textTalk.text = tutoData.Description;
			sContent.talk.width = tutoData.TalkSize [0];
			sContent.talk.height = tutoData.TalkSize [1];
			sContent.talk.transform.localPosition = new Vector3 (tutoData.TalkPosition[0],tutoData.TalkPosition[1],0);
			if(tutoData.Rotation == 0)
			{
				sContent.talk.flip = UIBasicSprite.Flip.Nothing;
			} else {
				sContent.talk.flip = UIBasicSprite.Flip.Vertically;
			}
			sContent.textTalk.width = tutoData.TextSize [0];
			sContent.textTalk.height = tutoData.TextSize [1];
			sContent.textTalk.transform.localPosition = new Vector3 (tutoData.TextPosition[0],tutoData.TextPosition[1],0);
			sContent.character.transform.localPosition = new Vector3 (150,-350,0);
			sContent.PreTalk ();
			TweenParms parms = new TweenParms();
			parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine).OnComplete(new TweenDelegate.TweenCallbackWParms(ClickTutorial),go);
			HOTween.To(sContent.character.transform, 0.5f, parms);		
		}
	}

	private void ClickTutorial(TweenEvent tweenEvent)
	{
		GameObject go = (GameObject)tweenEvent.parms [0];
		SetNameContent sContent = go.GetComponent<SetNameContent>();
		if(!sContent.flagTalk)
			sContent.Talk ();
		if(configManager.UserData.currentStepTownTutorial ==  TutorialFirstBattleLogic.SHOW_TURN)
		{
			sContent.number.gameObject.SetActive(true);
			sContent.number.text = hudView.TurnsToAttackLabel.text;
		}
		clickTutorial = true;
	}

	private void CloseTutorial(GameObject go)
	{
		SetNameContent sContent = go.GetComponent<SetNameContent>();
		if(sContent.flagTalk)
		{
			sContent.TalkComplete();
		} else {
			if(clickTutorial)
			{
				clickTutorial = false;
				TweenParms parms = new TweenParms();
				parms.Prop ("localScale", new Vector3 (0f, 0f, 0f)).Ease (EaseType.EaseOutSine);
				HOTween.To(sContent.name.transform, 0.25f, parms);
				
				TweenParms parms2 = new TweenParms();
				parms2.Prop ("localScale", new Vector3 (0f, 0f, 0f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(HideTutorial),1);
				HOTween.To(sContent.bg.transform, 0.25f, parms2);
				GridTutorialData gridData;
				if(configManager.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START &&
				   configManager.UserData.GetTutCollectGold() == 1)
				{
					gridData = configManager.gridTutorialCfg.GetGridTutorialByIdTutorial (TutorialFirstBattleLogic.ID_TUT_COLLECT_GOLD);
				} else {
					gridData = configManager.gridTutorialCfg.GetGridTutorialByIdTutorial (tutoData.Id);
				}
				if(gridData != null)
					tutLogic.DisplayBlackTitleGrid (gridData);
			}	
		}
	}
	
	private void CloseTutFirst(GameObject go)
	{
		if(clickTutorial)
		{
			SetNameContent sContent = go.GetComponent<SetNameContent>();
			if(sContent.flagTalk)
			{
				sContent.TalkComplete();
			} else {
				sContent.character.gameObject.SetActive (true);
				float size = 1;
				if(tutoData.EventName == RETURN_POOL)
					size = 0;
				TweenParms parms = new TweenParms();
				parms.Prop ("localScale", Vector3.one*size).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(HideTutorial),2);
				HOTween.To(sContent.character.transform, 0.25f, parms);
				tutLogic.ReturnParent();
				configManager.UserData.currentStepTownTutorial++;
				if(configManager.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.FIRST_BRING &&
				   configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END_BRING
				   && configManager.UserData.GetTutCollectGold() == 0)
				{
					tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_FRIST_BRING,tutLogic.GetSkill());
				}
				if(configManager.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.END_BRING + 1)
				{
					configManager.UserData.SetFirstBringDown(0);
					tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_BRING_END,tutLogic.GetSkill());
				}
			}
		}		
	}

	void HideTutorial(TweenEvent tweenEvent)
	{
		int type = (int)tweenEvent.parms [0];
		if(type == 2)
			HidePopup(GuiObjectName.tutorialUI);
		else HidePopup(GuiObjectName.tutorialInfoUI);
		if(configManager.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.FIRST_BRING &&
		   configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END_BRING)
		{
			if(tutoData.EventName != CREATE_NEW && tutoData.EventName != TEXT_CONTINUE)
				tutLogic.ReturnBlackObject();
		} else {
			tutLogic.ReturnBlackObject();
		}
		SetGuiBeingDisplayed (false);
		if(typeTut != TutorialFirstBattleLogic.TUT_FIRST_SKILL_MONSTER)
		{
			if(configManager.UserData.currentStepTownTutorial == 2)
			{
				tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_SHOW,tutLogic.GetSkill());
			}
			if(configManager.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.FIRST_BRING &&
			   configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END_BRING
			   && configManager.UserData.GetTutCollectGold() == 0)
			{
				tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_FRIST_BRING,tutLogic.GetSkill());
			}
			if(configManager.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.END_BRING + 1)
			{
				configManager.UserData.SetFirstBringDown(0);
				tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_BRING_END,tutLogic.GetSkill());
			}
		}
	}

	private void BuyGem(ShopItemCfg item)
	{
		HidePopup(GuiObjectName.popupPurchase);
		if(fx != null)
			fx.SetActive(true);
		ErrorCode error = shopService.BuyItem (item);
		switch(error)
		{
			case ErrorCode.OK:	
				ShowNotice(configManager.text.BuySuccessfully);
				break;
			case ErrorCode.NOT_ENOUGH_GOLD:
				ShowNotice(configManager.text.NotEnoughGold);
				break;
			case ErrorCode.NOT_ENOUGH_GEM:
				ShowNotice(configManager.text.NotEnoughGem);
				break;
			case ErrorCode.NOT_ENOUGH_SLOT:
				ShowNotice(configManager.text.NotEnoughSlot);
				break;
			default:
				ShowNotice (error.ToString ());
				break;
		}
	}
}