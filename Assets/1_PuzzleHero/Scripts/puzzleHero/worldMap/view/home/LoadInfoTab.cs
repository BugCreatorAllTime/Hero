using UnityEngine;
using System.Collections;
using strange.extensions.pool.api;
using System.Collections.Generic;
using System.Linq;
using strange.examples.strangerocks;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using System;

public class LoadInfoTab : BaseView {

	[Inject(PrefabWorldMap.updateShop)]
	public IPool<GameObject> updateShopPool { get; set; }
	[Inject(PrefabWorldMap.loading)]
	public IPool<GameObject> loadingPool { get; set; }
	[Inject(PrefabWorldMap.detailFBPopup)]
	public IPool<GameObject> detailPool { get; set;}
	[Inject(PrefabWorldMap.noticePurchase)]
	public IPool<GameObject> noticePurchasePool { get; set; }
	[Inject(PrefabWorldMap.noticePurchaseGem)]
	public IPool<GameObject> noticePurchaseGemPool { get; set; }
	[Inject(PrefabWorldMap.banner)]
	public IPool<GameObject> bannerPool { get; set; }
	[Inject(PrefabWorldMap.black)]
	public IPool<GameObject> blackBgPool { get; set; }
	[Inject(PrefabWorldMap.purchase)]
	public IPool<GameObject> purchasePool { get; set; }
	[Inject(PrefabWorldMap.purchaseGem)]
	public IPool<GameObject> purchaseGemPool { get; set; }
	[Inject(PrefabWorldMap.popup)]
	public IPool<GameObject> popupPool { get; set; }
	[Inject(PrefabWorldMap.setUserName)]
	public IPool<GameObject> setNamePool { get; set; }
	[Inject(PrefabWorldMap.tutorialUI)]
	public IPool<GameObject> tutPool { get; set; }
	[Inject(PrefabWorldMap.positioningUI)]
	public IPool<GameObject> posPool { get; set; }
	[Inject]
	public ShopService shopService { get; set;}
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public GoToHouseManager houseManager { get; set;}
	[Inject]
	public TutorialService tutorialService{ get; set;}
	[Inject]
	public AssetMgr mgr{get; set;}
	[Inject]
	public ItemService itemService { get; set; }
	[Inject]
	public InfoFbManager infoFbManager { get; set;}
	[Inject]
	public SoundManager soundManager{ get; set;}
	[Inject]
	public GameInput gameInput { get; set; }
	[Inject]
	public ClickTutManager clickTutMng { get; set;}
	[Inject]
	public SoundManager soundMng { get; set;}
	[Inject]
	public WorldMapHander worldMapHander { get;set;}
	[Inject]
	public GoToDungeonSignal goToDungeonSignal {get; set;}
	[Inject]
	public CrossContextData data {get; set;}

	[NGUITag]
	public UILabel ExpNumber { get; set;}
	[NGUITag]
	public UIProgressBar ExpProgress { get; set;}
	[NGUITag]
	public UILabel Level { get; set;}
	[NGUITag]
	public UILabel NumberGold { get; set;}
	[NGUITag]
	public UILabel NumberDiamond { get; set;}
	[NGUITag]
	public UILabel Physical { get; set;}
	[NGUITag]
	public UILabel NameCharacter { get; set;}
	[NGUITag]
	public UILabel TimeAdd { get; set;}
	[NGUITag]
	public UIButton ButtonDiamond { get; set;}
	[NGUITag]
	public UIButton ButtonGold { get; set;}
	[NGUITag]
	public UIButton ButtonShoe { get; set;}
	[NGUITag]
	public UISprite EnergyProgress { get; set;}
	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public GA ga { get; set; }

	GameObject Panel;
	bool creat = false;
	private float energy;
	private GameObject posObject;
	private GameObject setPool = null;
	private GameObject popup;
	private Camera myCamera;
	private List<GameObject> blackList;
	private Vector2 fingerDown;
	private Rect rect;
	private GameObject tutObject;
	private UIPanel TutPanel ;
	private UIPanel BlackTut ;
	private UICamera uiCamera;
	private int current;
	private List<ImgDataShow> listParent = new List<ImgDataShow> ();
	private int key;
	private GameObject iconUpdateShopObj;
	private GameObject iconUpdateHomeObj;
	public bool fDown = false;
	private Dictionary<int, int> chestBanners;
	private bool touch = true;
	public bool checkNotice;
	private UIScrollView scrollWorldMap;

	private int ID_DUNGEON_MAP_2 = 13;
	private int ID_DUNGEON_MAP_3 = 25;
	private const string setBannerPath = "Textures/Banner/BannerSet";
	private const int NUMBER_LEGEND_SET = 3;
	private const int NUMBER_RARE_SET = 4;

	private const int CREATE_NEW = 0;
	private const int TEXT_CONTINUE = 101;
	private const int HIDE_TEXT = 102;
	private const int RETURN_POOL = 103;
	private const int SPECIAL = 104;
	private const int SHOW_TEXT = 105;
	private const int SPECIAL_RETURN = 106;

	protected override void OnStart ()
	{
		base.OnStart ();
		soundMng.PlayMusic (SoundName.MUSIC_HOMETOWN);
		chestBanners = new Dictionary<int, int> ();
		chestBanners.Add (1, 1);
		chestBanners.Add (2, 2);
		chestBanners.Add (3, 3);
		chestBanners.Add (4, 4);
		chestBanners.Add (5, 5);
		chestBanners.Add (6, 6);
		chestBanners.Add (7, 7);
		scrollWorldMap = GameObject.Find("ScrollWorldMap").GetComponent<UIScrollView>();
		Panel = GameObject.Find ("tabPanel");
		ShowLoading (true);
		TutPanel = GameObject.Find ("TutPanel").GetComponent<UIPanel>();
		BlackTut = GameObject.Find ("BlackTut").GetComponent<UIPanel>();
		myCamera = GameObject.Find ("Camera").GetComponent<Camera>();
		iconUpdateShopObj = GameObject.Find("IconUpdateShop");
		iconUpdateHomeObj = GameObject.Find ("IconUpdateHome");
		uiCamera = myCamera.GetComponent<UICamera>();
		energy = (float)config.UserData.energy / config.CharacterCfg.GetMaxEnergy () * 0.9f;
		EnergyProgress.fillAmount = energy;
		
		shopService.BuyGemHandler += HandleGemBuying;
		gameInput.FingerUpEvent += new GameInput.FingerUp(OnFingerUp);
		gameInput.FingerDown += new GameInput.FingerDowned(OnFingerDown);
	}

	private void OnMethodStart()
	{
		CheckShopUpdate ();
		LoadInfo ();
		routineRunner.StartCoroutine(UpdateInfo());
		CheckCurrentTutorial ();
		CheckTypeTutorial(null);
		infoFbManager.SetInfoUserToMap ();
		infoFbManager.AddCountToCheckUpdateInfoFriend ();
		routineRunner.StartCoroutine(CheckShowBanner ());
		CheckHomeUpdate ();
	}

	private IEnumerator UpdateInfo()
	{
		while(true)
		{
			if(config.UserData.IsFullEnergy())
			{
				if(TimeAdd != null)
				{
					TimeAdd.gameObject.SetActive(false);
				}
			}
			else{
				if(TimeAdd != null)
				{
					long totalTime = config.UserData.lastTimeUpdate + config.general.EnergyCooldown - Utils.GetCurrentTimeInSecond();
					int minute = (int)(totalTime/60);
					int second = (int)(totalTime%60);
					TimeAdd.gameObject.SetActive(true);
					if(second >= 10)
						TimeAdd.text = minute+":"+second;
					else TimeAdd.text = minute+":0"+second;
				}
			}
			RefreshTab();
			yield return new WaitForSeconds(1f);
		}
	}

	public void RefreshTab()
	{
		LoadInfo ();
	}

	void CreatPopup(GameObject purchase, string iconName,Dictionary<string, ShopItemCfg> dictionary, string text="")
	{
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		purchase.transform.parent = Panel.transform;
		purchase.transform.localScale = Vector3.one;
		purchase.transform.localPosition = new Vector3 (0,-440,0);
		PopupPurchaseContent pContent = purchase.GetComponent<PopupPurchaseContent>();
		pContent.disable.alpha = 0.7f;
		if(pContent.text != null)
		{
			pContent.text.text = text;
		}
		List<ShopItemCfg> listItemShop = GetListItemSort (dictionary);
		for(int i = 0; i < listItemShop.Count; i++)
		{
			ShopItemCfg shopItem = listItemShop[i];
			pContent.name[i].text = shopItem.Name;
			pContent.description[i].text = shopItem.Description;
			pContent.itemShop[i] = shopItem;
			if(shopItem.Gem > 0)
			{
				pContent.price[i].text = shopItem.Gem.ToString() ;
				pContent.iconGem[i].spriteName = "diamond";
				pContent.iconGem[i].width = 25;
				pContent.iconGem[i].height = 25;
			}
			if(shopItem.RM > 0)
			{
				pContent.price[i].text = shopItem.RM.ToString();
				pContent.iconGem[i].spriteName = "GTokenLogo";
				pContent.iconGem[i].width = 25;
				pContent.iconGem[i].height = 25;
			}
			AddEventToButton (pContent.button[i],"BuyPurchaseItem"+(i+1),pContent.gameObject);
			pContent.icon[i].spriteName = iconName+(i+1);
		}
		AddEventToButton (pContent.exit,"Exit",pContent.gameObject);
	}

	private List<ShopItemCfg> GetListItemSort(Dictionary<string, ShopItemCfg> dictionary)
	{
		List<ShopItemCfg> list = new List<ShopItemCfg> ();
		for(int i = 0; i < dictionary.Count; i++)
		{
			ShopItemCfg shopItem = dictionary.ElementAt(i).Value;
			list.Add(shopItem);
		}
		bool check = false;
		while(!check)
		{
			check = true;
			for(int i = 0; i < list.Count - 1; i++)
			{
				if(list[i].Id > list[i+1].Id)
				{
					check = false;
					ShopItemCfg tg = list[i].Clone();
					list[i] = list[i+1].Clone();
					list[i+1] = tg.Clone();
				}
			}
		}
		return list;
	}

	void BuyPurchaseItem1(GameObject go)
	{
		BuyPurchaseShop (go, go.GetComponent<PopupPurchaseContent>().itemShop [0]);
	}

	void BuyPurchaseItem2(GameObject go)
	{
		BuyPurchaseShop (go, go.GetComponent<PopupPurchaseContent>().itemShop [1]);
	}

	void BuyPurchaseItem3(GameObject go)
	{
		BuyPurchaseShop (go, go.GetComponent<PopupPurchaseContent>().itemShop [2]);
	}

	void BuyPurchaseItem4(GameObject go)
	{
		BuyPurchaseShop (go, go.GetComponent<PopupPurchaseContent>().itemShop [3]);
	}

	void BuyPurchaseItem5(GameObject go)
	{
		BuyPurchaseShop (go, go.GetComponent<PopupPurchaseContent>().itemShop [4]);
	}

	void BuyPurchaseShop(GameObject go, ShopItemCfg item)
	{
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		PopupPurchaseContent pContent = go.GetComponent<PopupPurchaseContent>();
		ErrorCode error = shopService.BuyItem (item);
		switch(error)
		{
			case ErrorCode.OK:	
				ShowNotice(config.text.BuySuccessfully);
				RefreshTab();
				ga.TrackCashItemBuy(item.Type, item.Id);
				break;
			case ErrorCode.NOT_ENOUGH_GOLD:
				ShowNoticePurchaseGold();
				break;
			case ErrorCode.NOT_ENOUGH_GEM:
				ShowNoticePurchaseGem();
				break;
			case ErrorCode.NOT_ENOUGH_SLOT:
				ShowNoticePurchaseSlot();
				break;
			case ErrorCode.WAITING_FOR_PROCESS:
				ShowNotice(config.text.WaitingProgress, false);
				routineRunner.StartCoroutine(TimeOutNotice());
				break;
			default:
				ShowNotice (error.ToString ());
				break;
		}
		Exit (go);
		GameObject slot = GameObject.Find("Slot");
		if(slot != null)
		{
			slot.GetComponent<UILabel>().text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		}
	}

	void BuyGold(GameObject go)
	{
		GameObject purchase = CreatePoolGameObject (purchasePool);
		CreatPopup (purchase, "gold", config.shopCfg.gold);
	}

	public void ShowNoticePurchaseGold()
	{
		GameObject purchase = CreatePoolGameObject (noticePurchasePool);
		CreatPopup (purchase, "gold", config.shopCfg.gold, config.text.NotEnoughGold);
	}

	public void BuySlot(GameObject go)
	{
		GameObject purchase = CreatePoolGameObject (purchasePool);
		CreatPopup (purchase, "bag", config.shopCfg.bag);
	}

	public void ShowNoticePurchaseSlot()
	{
		GameObject purchase = CreatePoolGameObject (noticePurchasePool);
		CreatPopup (purchase, "bag", config.shopCfg.bag, config.text.NotEnoughSlot);
	}

	void BuyGem(GameObject go)
	{
		GameObject purchase = CreatePoolGameObject (purchaseGemPool);
		CreatPopup (purchase, "jewel", config.shopCfg.gem);
	}

	public void ShowNoticePurchaseGem()
	{
		GameObject purchase = CreatePoolGameObject (noticePurchaseGemPool);
		CreatPopup (purchase, "jewel", config.shopCfg.gem, config.text.NotEnoughGem);
	}

	void Exit(GameObject go)
	{
		PopupPurchaseContent pContent = go.GetComponent<PopupPurchaseContent> ();
		pContent.exit.onClick.Clear ();
		for(int i = 0; i < pContent.name.Length; i++)
		{
			pContent.button[i].onClick.Clear();
		}
		if(pContent.text == null)
		{
			if(pContent.name.Length == 3)
				ReturnInstance (pContent.gameObject,purchasePool);
			else ReturnInstance (pContent.gameObject,purchaseGemPool);
		}
		else{
			if(pContent.name.Length == 3)
				ReturnInstance (pContent.gameObject,noticePurchasePool);
			else ReturnInstance (pContent.gameObject,noticePurchaseGemPool);
		}
	}

	void BuyEnergy(GameObject go)
	{
		GameObject purchase = CreatePoolGameObject (purchasePool);
		CreatPopup (purchase, "energy", config.shopCfg.energy);
	}

	public void ShowNoticePurchaseEnergy()
	{
		GameObject purchase = CreatePoolGameObject (noticePurchasePool);
		CreatPopup (purchase, "energy", config.shopCfg.energy, config.text.NotEnoughEnegry);
	}

	public void LoadInfo()
	{
		NameCharacter.text = config.UserData.Name;
		Level.text = config.UserData.currentLevel.ToString ();
		float percentLevel = (float) config.UserData.levelPercent;
		ExpNumber.text = Mathf.Floor(percentLevel*100)+"%";
		ExpProgress.value = percentLevel;
		Physical.text = config.UserData.energy + "/" + config.CharacterCfg.GetMaxEnergy();
		float mEnergy = (float)config.UserData.energy / config.CharacterCfg.GetMaxEnergy ()*0.9f;
		NumberGold.text = config.UserData.gold.ToString();
		NumberDiamond.text = config.UserData.gem.ToString();
		energy = ChangeValue (energy, mEnergy,EnergyProgress);
		if(!creat)
		{
			AddEventToButton (ButtonGold,"BuyGold",ButtonGold.gameObject);
			AddEventToButton (ButtonDiamond,"BuyGem",ButtonDiamond.gameObject);
			AddEventToButton (ButtonShoe,"BuyEnergy",ButtonShoe.gameObject);
			creat = true;
		}

	}

	float ChangeValue(float beforeValue, float afterValue, UISprite progress)
	{
		if(beforeValue != afterValue)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("fillAmount", afterValue).Ease (EaseType.EaseOutSine);
			HOTween.To(progress, 0.5f, parms);
			beforeValue = afterValue;
		}
		return beforeValue;
	}

	void SetUserName()
	{
		if(setPool == null)
			setPool = CreatePoolGameObject (setNamePool);
		setPool.transform.parent = Panel.transform;
		setPool.transform.localScale = Vector3.one;
		SetNameContent sContent = setPool.GetComponent<SetNameContent>();
		sContent.disable.alpha = 0.7f;
		sContent.camera.gameObject.SetActive (true);
		sContent.name.text = config.UserData.Name;
		GameObject male = null;
		GameObject female = null;
		CreateCharacter (sContent.camera.gameObject, true, o =>
        {
			male = o;
			AddEventToButton(sContent.maleSelect,"SelectCharacter", male);
		});
		CreateCharacter (sContent.camera.gameObject, false, o =>
		                 {
			female = o;
			AddEventToButton(sContent.femaleSelect,"SelectCharacter", female);
		});
		setPool.transform.localPosition = new Vector3 (0,-430,0);
		AddEventToButton(sContent.accept,"AcceptName", setPool);
	}

	void SelectCharacter(GameObject go)
	{
		Transform parent = go.transform.parent;
		UISprite select = parent.GetComponentInChildren<UISprite> ();
		if(go.name == "male")
		{
			config.UserData.CharacterID = 0;
			select.transform.localPosition = new Vector3(-116,34,0);
		}
		else {
			config.UserData.CharacterID = 1;
			select.transform.localPosition = new Vector3(104,34,0);
		}
		SkeletonAnimation skeletonAnimation = go.GetComponent<SkeletonAnimation>();
		skeletonAnimation.state.SetAnimation(0, AnimationName.Attack, false);
		skeletonAnimation.state.AddAnimation (0, AnimationName.Idle, true,0);
	}

	GameObject CreateCharacter(GameObject parent, bool male, Action<GameObject> onLoadGameObject)
	{
		int x = 0;
		int ID = 0;
		if(male)
		{
			ID = 0;
			x = -620;
		}
		else {
			ID = 1;
			x = 500;
		}
		mgr.GetAsset<GameObject> ("Prefabs/Battle/SimpleObj", delegate (GameObject go){
			GameObject player = GameObject.Instantiate(go) as GameObject;
			player.transform.parent = gameObject.transform;
			player.gameObject.layer = 10;
			player.transform.parent = parent.transform;
			CharacterData characterData = config.CharacterCfg.character[ID.ToString()];
			player.name = characterData.Name;
			player.transform.localScale = new Vector3 (3,3,3);

			mgr.GetAsset<SkeletonDataAsset>("Animation/Character/Character.ske", delegate (SkeletonDataAsset skele){
				SkeletonAnimation skeletonAnimation = player.GetComponent<SkeletonAnimation>();
				skeletonAnimation.skeletonDataAsset = skele;
				skeletonAnimation.skeletonDataAsset.Reset();
				skeletonAnimation.Reset();
				skeletonAnimation.skeleton.SetSkin(characterData.Name);
				skeletonAnimation.skeleton.SetSlotsToSetupPose();
				skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
				itemService.WearItemView(skeletonAnimation, config.UserData.EquippedItemData, characterData.Name, config.UserData.IsActiveSet());
				player.transform.localPosition = new Vector3(x, -200,150);
				onLoadGameObject(player);
			});
		});
		return null;
	}

	void AcceptName (GameObject go)
	{
		SetNameContent sContent = go.GetComponent<SetNameContent>();
		tutorialService.SetUserName (sContent.name.text);
		sContent.accept.onClick.Clear ();
		ReturnInstance (go, setNamePool);
		tutorialService.NextStep (config.UserData);
		houseManager.GoHouse (GoToHouseManager.HomeButton);
	}

	public IEnumerator CloseTutorialProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			SetNameContent sContent = go.GetComponent<SetNameContent>();
			if(sContent.flagTalk && (key < 1 || key > 99))
			{
				sContent.TalkComplete();
			} else {
				sContent.close.onClick.Clear ();
				GameObject gObject = null;
				if(listParent.Count > 0)
				{
					gObject = listParent [0].child.gameObject;
					for(int i = 0; i < listParent.Count; i++)
					{
						listParent[i].child.parent = listParent[i].parent;
						listParent[i].child.gameObject.SetActive(false);
						listParent[i].child.gameObject.SetActive(true);
					}
					listParent.Clear();
				}
				if(key == RETURN_POOL || key == SPECIAL_RETURN || key == 7)
				{
					TweenParms parms = new TweenParms();
					parms.Prop ("localScale", new Vector3 (0f, 0f, 0f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnPool), go, tutPool);
					HOTween.To(sContent.character.transform, 0.25f, parms);
				}
				if(key == HIDE_TEXT || key == SPECIAL || key == SPECIAL_RETURN || key == 3 || key == 4 || key == 6 || key == 7)
				{
					TweenParms parms = new TweenParms();
					parms.Prop ("localScale", new Vector3 (0f, 0f, 0f)).Ease (EaseType.EaseOutSine);
					HOTween.To(sContent.character.transform, 0.25f, parms);
				}
				float timeAdd= 0;
				bool isTutUpgrade = false;
				if(key == 3 || key == 6){
					sContent.arrow.gameObject.SetActive(false);
					if(key == 3)
						timeAdd = 1.5f;
					else isTutUpgrade = true;
					for(int i = 0; i < blackList.Count; i++)
					{
						blackList[i].GetComponent<UISprite>().alpha = 0.01f;
					}
				}
				if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.BUY_ITEM)
				{
					checkNotice = true;
					sContent.arrow.gameObject.SetActive(false);
					ReturnBlackObject();
				}
				else checkNotice = false;
				if(gObject != null)
				{
					clickTutMng.Progress (gObject, key);
					key = 0;
				}
				if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.RECEIVE_ITEM)
				{
					ReturnInstance (posObject, posPool);
					GameObject.Find("BlackSmith").GetComponent<LoadInfoBlacksmith>().InsertItemInTutBlackSmith();
				}
				tutorialService.NextStep (config.UserData);
				if(!isTutUpgrade)
					routineRunner.StartCoroutine(CheckTut(0.25f+timeAdd));
			}
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	void CloseTutorial (GameObject go)
	{
		routineRunner.StartCoroutine (CloseTutorialProgress(go));
	}

	void TutorialShow(HomeTownTutorialData tutoData, int type, Transform parent)
	{
		key = tutoData.EventName;
		if(tutObject == null || !tutObject.activeInHierarchy)
			tutObject = CreatePoolGameObject (tutPool);
		tutObject.transform.localScale = Vector3.one;
		if(config.UserData.GetDefeat())
			tutObject.transform.parent = parent;
		else
			tutObject.transform.parent = TutPanel.transform;
		tutObject.transform.localScale = Vector3.one;
		SetNameContent sContent = tutObject.GetComponent<SetNameContent>();
		sContent.talk.alpha = 1;
		sContent.character.alpha = 1;
		sContent.arrow.gameObject.SetActive (false);

		if(!config.UserData.GetDefeat())
		{
			CreateBlackObject (sContent.list.transform, tutoData, type);
		}
		if(key == CREATE_NEW || key == SPECIAL || key == SHOW_TEXT || key == SPECIAL_RETURN)
		{
			sContent.character.transform.localScale = Vector3.zero;
			tutObject.transform.localPosition = Vector3.zero;
		}
		if(key == TEXT_CONTINUE)
		{
			sContent.character.transform.localScale = Vector3.one;
			tutObject.transform.localPosition = Vector3.one;
		}
		sContent.textTalk.text = tutoData.Description;
		if(tutoData.Description == "empty")
		{
			sContent.textTalk.text = "";
			sContent.talk.alpha = 0;
			sContent.character.alpha = 0;
		}
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
		sContent.character.transform.localPosition = new Vector3 (150,-350,0);
		sContent.PreTalk ();
		if (config.UserData.GetDefeat ())
		{
			tutObject.transform.localPosition = new Vector3 (0,-130,0);
			sContent.skip.gameObject.SetActive(false);
		}
		routineRunner.StartCoroutine(Talk(sContent));
		if(type != HomeTownTutorialCfg.TUTCLICK && !config.UserData.GetDefeat())
		{
			sContent.close.gameObject.GetComponent<BoxCollider>().enabled = true;
			AddEventToButton(sContent.close,"CloseTutorial", tutObject);
		} else {
			sContent.close.gameObject.GetComponent<BoxCollider>().enabled = false;
		}
		sContent.textTalk.transform.localPosition = new Vector3 (tutoData.TextPosition[0],tutoData.TextPosition[1],0);
		if(key == CREATE_NEW || key == SPECIAL || key == SHOW_TEXT || key == SPECIAL_RETURN)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
			HOTween.To(sContent.character.transform, 0.5f, parms);	
		}
	}

	private IEnumerator Talk(SetNameContent sContent)
	{
		yield return new WaitForSeconds(0.5f);
		sContent.Talk ();
	}

	void Positioning(HomeTownTutorialData tutData, int type)
	{
		if(posObject == null || !posObject.activeInHierarchy)
		{
			for(int i = 0; i < BlackTut.transform.childCount; i++)
			{
				BlackTut.transform.GetChild(i).gameObject.SetActive(false);
			}
			posObject = CreatePoolGameObject (posPool);
			posObject.transform.localScale = Vector3.one;
		}
		OscillateView dContent = posObject.GetComponent<OscillateView>();
		dContent.state = OscillateView.VERTICALLY;
		Vector3 pos = GameObject.Find (tutData.Target).transform.localPosition;
		posObject.transform.localPosition = new Vector3 (pos.x,pos.y+120,pos.z);
		dContent.pos = posObject.transform.localPosition;
		dContent.start = true;
		if(type == HomeTownTutorialCfg.POSCLICK)
		{
			posObject.transform.parent = BlackTut.transform;
		}
		else posObject.transform.parent = houseManager.main.GetComponentInChildren<UIScrollView> ().gameObject.transform;
		posObject.GetComponentInChildren<UISprite>().MakePixelPerfect();
		
		key = tutData.EventName;
		CreateBlackObject (BlackTut.transform, tutData, type);

	}

	public void AddEventToButton(UIButton button, string nameEvent, GameObject go)
	{
		EventDelegate eventButton = new EventDelegate(this, nameEvent);
		eventButton.parameters[0] = new EventDelegate.Parameter(go, "go");
		button.onClick.Add(eventButton);
	}

	GameObject CreatePoolGameObject(IPool<GameObject> poolObject)
	{
		GameObject contentObject = poolObject.GetInstance();
		contentObject.SetActive(true);
		return contentObject;
	}
	
	void ReturnInstance(GameObject contentObject, IPool<GameObject> contentPool)
	{
		if(contentObject != null)
		{
			contentObject.SetActive (false);
			contentPool.ReturnInstance(contentObject);
		}

	}

	public void ShowNotice(string content, bool isAddEventHide = true)
	{
		popup = CreatePoolGameObject (popupPool);
		popup.transform.parent = Panel.transform;
		popup.transform.localScale = Vector3.one;
		PopUpContent pContent = popup.GetComponent<PopUpContent>();
		pContent.popUpDisabel.alpha = 0.7f;
		pContent.btHide.gameObject.SetActive (true);
		pContent.btHideIcon.gameObject.SetActive (false);
		if(isAddEventHide)
			AddEventToButton(pContent.btHide,"HideNotice", popup);
		pContent.content.text = content;
		popup.transform.localScale = new Vector3 (0.1f,0.1f,0.1f);
		pContent.popUpDisabel.transform.localScale = new Vector3 (10,10,10);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
		HOTween.To(pContent.transform, 0.25f, parms);
	}

	void HideNotice(GameObject go)
	{
		PopUpContent pContent = go.GetComponent<PopUpContent> ();
		if(pContent.popUpDisabel.transform != null)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("localScale", new Vector3 (20f, 20f, 20f)).Ease (EaseType.EaseOutSine);
			HOTween.To(pContent.popUpDisabel.transform, 0.25f, parms);
		}

		pContent.btHide.onClick.Clear ();
		TweenParms parms2 = new TweenParms();
		parms2.Prop ("localScale", new Vector3 (0.1f, 0.1f, 0.1f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnPool), go, popupPool);
		HOTween.To(pContent.transform, 0.25f, parms2);
	}

	void ReturnPool(TweenEvent tweenEvent)
	{
		IPool<GameObject> pool = (IPool<GameObject>)tweenEvent.parms [1];
		ReturnInstance (((GameObject)tweenEvent.parms [0]).gameObject, pool);
		ReturnBlackObject ();
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.CLICK_SHOP)
		{
			if(tutorialService.CheckInLocation("Shop"))
			{
				TabManager tm = GameObject.Find("TabsideShop1").GetComponent<TabManager>();
				tm.indexTabChoice = 2;
				tm.SetTab ();
				tutorialService.NextStep(config.UserData);
			}
		}
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.BLACKSMITH)
		{
			if(tutorialService.CheckInLocation("BlackSmith"))
			{
				tutorialService.NextStep(config.UserData);
			}
		}
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.TUT_OPEN_CHEST)
		{
			if(tutorialService.CheckInLocation("Home"))
			{
				tutorialService.NextStep(config.UserData);
			}
		}
		if(checkNotice)
		{
			CheckTypeTutorial(null);
			checkNotice = false;
		}
	}

	public IEnumerator CheckTut(float duration)
	{
		yield return new WaitForSeconds(duration);
		if(!checkNotice)
			CheckTypeTutorial(null);
	}

	private void CheckCurrentTutorial()
	{
		if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START && 
		   config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.END_SHOP)
		{
			if(config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.CLICK_SHOP)
			{
				for(int i = config.UserData.EquippedItemData.Count-1; i >= 0; i--)
				{
					config.UserData.UnequipItem(config.UserData.EquippedItemData[i]);
				}
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.START;
			} else if(config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.CLICK_SHOP &&
			          config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.BUY_ITEM)
			{
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.CLICK_SHOP;
			} else {
				config.UserData.currentStepTownTutorial =  TutorialFirstBattleLogic.BLACKSMITH;
			}
		}
		if(config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.BLACKSMITH && 
		   config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END)
		{
			if(config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.BLACKSMITH &&
			   config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.END)
			{
				if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.RECEIVE_ITEM)
				{
					RemoveItemReceive();
				}
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.BLACKSMITH;
			}
		}
	}

	public void CheckTypeTutorial(string nameHouse)
	{
		CheckTutOpenChest ();
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.TUT_OPEN_CHEST)
		{
			if(tutorialService.CheckInLocation("Home"))
			{
				tutorialService.NextStep(config.UserData);
			}
		}
		if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.TUT_OPEN_END &&
		   config.UserData.GetTutOpenChest() == 1 && current != 0)
		{
			config.UserData.currentStepTownTutorial = current;
			config.UserData.SetTutOpenChest(0);
		}
		if((tutorialService.CheckTownTutorial(config.UserData) && 
		    config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END)
		   || (config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.TUT_OPEN_CHEST 
		    &&  config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.TUT_OPEN_END))
		{
			if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.CLICK_SHOP)
			{
				for(int i = 0; i < BlackTut.transform.childCount; i++)
				{
					BlackTut.transform.GetChild(i).gameObject.SetActive(true);
				}
			}
			if(config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.START)
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.START;
			HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial (config.UserData.currentStepTownTutorial);
			tutoData.Description = tutoData.Description.Replace(";",",");
			if(tutorialService.CheckInLocation(tutoData.Location))
			{
				switch(tutoData.Type)
				{
				case HomeTownTutorialCfg.SET_USERNAME:
					SetUserName();
					break;
				case HomeTownTutorialCfg.TUTORIAL: 
					TutorialShow(tutoData, HomeTownTutorialCfg.TUTORIAL, null);
					break;
				case HomeTownTutorialCfg.TUTSHOW: 
					TutorialShow(tutoData, HomeTownTutorialCfg.TUTSHOW, null);
					break;
				case HomeTownTutorialCfg.TUTCLICK:
					TutorialShow(tutoData, HomeTownTutorialCfg.TUTCLICK, null);
					break;
				case HomeTownTutorialCfg.POSITIONING:
					Positioning(tutoData, HomeTownTutorialCfg.POSITIONING);
					break;
				case HomeTownTutorialCfg.POSCLICK:
					Positioning(tutoData, HomeTownTutorialCfg.POSCLICK);
					break;
				default:
					break;
				}
			}
		}		
	}

	public void CreateBlackObject(Transform parent, HomeTownTutorialData tutoData, int type)
	{
		if(type == HomeTownTutorialCfg.POSITIONING){

		}
		else if(type == HomeTownTutorialCfg.TUTORIAL)
		{
			CreateComponentBlack(-55,-45,900,1200,parent);
		} else {
			if(tutoData.Target != "empty")
			{
				UISprite img = GameObject.Find(tutoData.Target).GetComponent<UISprite>();
				Vector3 pos = Utils.GetPosition2D(img.transform);
				int sw = 640;
				int sh = 960;
				rect = new Rect(pos.x,pos.y,img.width,img.height);
				SetNameContent sContent = parent.parent.GetComponent<SetNameContent>();
				if(sContent != null)
				{
					sContent.arrow.gameObject.SetActive(true);
					sContent.arrow.transform.localPosition = new Vector3(pos.x, pos.y+img.height/2+50, pos.z);
				}
				if(key == 0)
				{
					CreateComponentBlack(-55,-45,900,1200,parent);
				}
				GameObject go;
				for(int i = 0; i < tutoData.ImgName.Count; i++)
				{
					go = GameObject.Find(tutoData.ImgName[i]);
					if(go != null && go.transform.parent != null)
					{
						ImgDataShow ids = new ImgDataShow(go.transform, go.transform.parent);
						if(listParent.Count > 0)
						{
							if(listParent[0].child.name == go.transform.name);
							else {
								listParent.Add(ids);
							}
						} else {
							listParent.Add(ids);
						}
						Vector3 posGo = Utils.GetPosition2D(go.transform);
						go.transform.parent = parent;
						go.transform.localPosition = posGo;
						go.SetActive(false);
						go.SetActive(true);
					}
				}
			}
		}
	}

	private void CreateComponentBlack(float x, float y, float w, float h, Transform parent, float alpha = 0.7f)
	{
		ReturnBlackObject ();
		blackList = new List<GameObject> ();
		GameObject blackObject = blackBgPool.GetInstance();
		blackList.Add(blackObject);
		blackObject.SetActive(true);
		blackObject.transform.parent = parent;
		UISprite image = blackObject.GetComponent<UISprite>();
		blackObject.transform.localScale = Vector3.one;
		image.fillAmount = 1;
		blackObject.transform.localPosition = new Vector3(x,y,0);
		image.width = (int)w;
		image.height = (int)h;
		image.alpha = alpha;
	}
	
	public void ReturnBlackObject()
	{
		if(blackList != null)
		{
			if(config.UserData.currentStepTownTutorial != TutorialFirstBattleLogic.END_SHOP)
			{
				for(int i = 0; i < blackList.Count; i++)
				{
					blackBgPool.ReturnInstance(blackList[i]);
					blackList[i].SetActive(false);
				}
				blackList.Clear ();
			}
		}

	}
	
	private void OnFingerDown(FingerDownEvent fingerDownEvent)
	{
		fDown = true;
		Vector2 worldPosition = myCamera.ScreenToWorldPoint(fingerDownEvent.Position);
		Vector2 boardPosition = TutPanel.transform.InverseTransformPoint(worldPosition);
		fingerDown = boardPosition;
	}

	private void OnFingerUp(FingerUpEvent fingerUpEvent)
	{
		fDown = false;
		Vector2 worldPosition = myCamera.ScreenToWorldPoint(fingerUpEvent.Position);
		Vector2 boardPosition = TutPanel.transform.InverseTransformPoint(worldPosition);
		if(rect!= null)
		{
			if(boardPosition.x >= rect.x - rect.width/2 && boardPosition.x <= rect.x + rect.width/2
			   && boardPosition.y >= rect.y - rect.height/2 && boardPosition.y <= rect.y + rect.height/2)
			{
				Vector2 compare = boardPosition - fingerDown;
				if(compare.sqrMagnitude <= 3)
				{
					HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial (config.UserData.currentStepTownTutorial);
					if(tutoData.Type == HomeTownTutorialCfg.TUTCLICK)
					{
						CloseTutorial(tutObject);
					}
				}
			}
		}
	}

	private void CheckTutOpenChest ()
	{
		if(config.UserData.ChestModel.chests.Count > 0 && config.UserData.GetTutOpenChest() == 1)
		{
			if(config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.TUT_OPEN_CHEST)
			{
				current = config.UserData.currentStepTownTutorial;
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.TUT_OPEN_CHEST;
			}
		}
	}

	public void CheckShopUpdate()
	{
		if(config.UserData.GetPopupUpdate() == 1)
		{
			config.UserData.SetPopupUpdate(0);
			ShowUpdatePopup(config.text.UpdateItemShop);
		}
		if(config.UserData.GetIconUpdate() == 0)
		{
			iconUpdateShopObj.SetActive(false);
		} else {
			iconUpdateShopObj.SetActive(true);
			OscillateView dContent = iconUpdateShopObj.GetComponent<OscillateView>();
			dContent.state = OscillateView.FLASH;
		}
	}

	public void CheckHomeUpdate()
	{
		if(config.UserData.GetItemUpdate())
		{
			iconUpdateHomeObj.SetActive(true);
			OscillateView dContent = iconUpdateShopObj.GetComponent<OscillateView>();
			dContent.state = OscillateView.FLASH;
		} else {
			iconUpdateHomeObj.SetActive(false);
		}
	}

	public void ShowNoticeGoDungeon(string content)
	{
		GameObject popup = CreatePoolGameObject (updateShopPool);
		popup.transform.parent = Panel.transform;
		popup.transform.localScale = Vector3.one;
		BannerContent bContent = popup.GetComponent<BannerContent>();
		bContent.disable.alpha = 0.7f;
		bContent.textContent.text = content;
		bContent.textBtGo.text = "Yes";
		bContent.textBtLater.text = "No";
		AddEventToButton(bContent.buttonLater,"HideBanner", bContent.gameObject);
		AddEventToButton(bContent.buttonGo,"GoDungeon", bContent.gameObject);
		
		
		bContent.transform.localScale = new Vector3 (0.1f,0.1f,0.1f);
		bContent.disable.transform.localScale = new Vector3 (10,10,10);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
		HOTween.To(bContent.transform, 0.25f, parms);
	}

	public void ShowInfoFBDetail(float x, float y, string name, int gender, List<int> equipments, Transform parent, ClickAvatarView avaView)
	{
		float posX = 0;
		float posY = 0;
		if(x < -160)
		{
			posX += 255;
		} else if(x > 160)
		{
			posX -= 140;
		}
		if(y < -scrollWorldMap.transform.localPosition.y)
		{
			posY += 270;
		} else
		{
			posY -= 220;
		}
		GameObject detailPopup = CreatePoolGameObject (detailPool);
		detailPopup.transform.parent = parent.parent;
		detailPopup.transform.localScale = Vector3.one;
		detailPopup.transform.localPosition = new Vector3 (posX, posY, 0);
		FBDetailContent content = detailPopup.GetComponent<FBDetailContent>();
		content.disable.alpha = 0.01f;
		content.name.text = name;
		content.view = avaView;
		detailPopup.transform.localScale = new Vector3 (0.1f,0.1f,0.1f);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
		HOTween.To(detailPopup.transform, 0.25f, parms);
		routineRunner.StartCoroutine (CompleteShowDetail(content, gender, equipments));

	}

	private IEnumerator CompleteShowDetail(FBDetailContent content, int gender, List<int> equipments)
	{
		yield return new WaitForSeconds(0.3f);
		Vector3 pos = Utils.GetPosition2D(content.transform);
		GameObject player = null;
		if(content.content.transform.childCount == 0)
		{
			mgr.GetAsset<GameObject>("Prefabs/Battle/SimpleObj", delegate (GameObject go){
				player = GameObject.Instantiate(go) as GameObject;
				FillDataToCharacter(player, pos, content, gender, equipments);
			});
		} else {
			player = content.content.transform.GetChild(0).gameObject;
			player.SetActive(true);
			FillDataToCharacter(player, pos, content, gender, equipments);
		}
		content.view.ResetAva ();
	}

	private void FillDataToCharacter(GameObject player, Vector3 pos, FBDetailContent content, int gender, List<int> equipments)
	{
		player.transform.parent = content.content.transform;
		player.gameObject.layer = 10;
		player.transform.parent = content.content.transform;
		string sex = "";
		if(gender == 0)
			sex = "male";
		else sex = "female";
		player.transform.localScale = Vector3.one * 0.7f;
		player.transform.localPosition = new Vector3(pos.x-10, pos.y -120, 0);
		UserData myUser = new UserData ();
		ItemBaseData item;
		myUser.Inventory = new InventoryModel ();
		myUser.EquippedItemData = new List<ItemBaseData>();
		myUser.Inventory.ListItemData = new List<ItemBaseData> ();
		for(int i = 0; i < equipments.Count; i++)
		{
			ItemCfgImpl iConfig = config.ItemCfg.GetItemByItemId(equipments[i]);
			item = new ItemBaseData(iConfig);
			myUser.Inventory.AddItem(item);
			myUser.EquipItem (item);
		}
		mgr.GetAsset<SkeletonDataAsset>("Animation/Character/Character.ske", delegate (SkeletonDataAsset skele){
			SkeletonAnimation skeletonAnimation = player.GetComponent<SkeletonAnimation>();
			skeletonAnimation.skeletonDataAsset = skele;
			skeletonAnimation.skeletonDataAsset.Reset();
			skeletonAnimation.Reset();
			skeletonAnimation.skeleton.SetSkin(sex);
			skeletonAnimation.skeleton.SetSlotsToSetupPose();
			skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
			content.ske = skeletonAnimation;
			itemService.WearItemView(skeletonAnimation, myUser.EquippedItemData, sex, myUser.IsActiveSet());
		});
		AddEventToButton(content.button,"HideDetail", content.gameObject);
	}

	private void HideDetail(GameObject go)
	{
		go.transform.localScale = Vector3.one;
		FBDetailContent content = go.GetComponent<FBDetailContent>();
		content.view.ResetAva ();
		for(int i = 0; i < content.content.transform.childCount; i++)
		{
			content.content.transform.GetChild(i).gameObject.SetActive(false);
		}
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (0f, 0f, 0f)).Ease (EaseType.EaseOutSine).
			OnComplete(new TweenDelegate.TweenCallbackWParms(CompleteHideDetail), go);
		HOTween.To(go.transform, 0.25f, parms);
	}

	private void CompleteHideDetail(TweenEvent tweenEvent)
	{
		GameObject go = (GameObject)tweenEvent.parms [0];
		ReturnInstance (go, detailPool);
	}

	private void ShowUpdatePopup(string content)
	{
		GameObject popup = CreatePoolGameObject (updateShopPool);
		popup.transform.parent = Panel.transform;
		popup.transform.localScale = Vector3.one;
		BannerContent bContent = popup.GetComponent<BannerContent>();
		bContent.disable.alpha = 0.7f;
		bContent.textContent.text = content;
		bContent.textBtGo.text = "GO NOW!";
		bContent.textBtLater.text = "LATER";
		AddEventToButton(bContent.buttonLater,"HideBanner", bContent.gameObject);
		AddEventToButton(bContent.buttonGo,"GoShop", bContent.gameObject);


		bContent.transform.localScale = new Vector3 (0.1f,0.1f,0.1f);
		bContent.disable.transform.localScale = new Vector3 (10,10,10);
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
		HOTween.To(bContent.transform, 0.25f, parms);
	}

	private IEnumerator CheckShowBanner()
	{
		yield return new WaitForSeconds(1f);
		if(config.UserData.GetDefeat())
		{
			int rateShowBanner = ReturnNumberRandom(1,100);
			if(rateShowBanner > 30)
			{
				GameObject banner = CreatePoolGameObject (bannerPool);
				HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial(TutorialFirstBattleLogic.BANNER_SHOW);
				TutorialShow(tutoData, 2, banner.transform);
				banner.transform.parent = Panel.transform;
				banner.transform.localScale = Vector3.one;
				BannerContent bContent = banner.GetComponent<BannerContent>();
				bool check = false;
				int id = 0;
				int index = 0;
				while(!check)
				{
					if(config.UserData.curMapId < ID_DUNGEON_MAP_2)
					{
						index = ReturnNumberRandom(1, NUMBER_RARE_SET);
					} else if(config.UserData.curMapId < ID_DUNGEON_MAP_3)
					{
						index = ReturnNumberRandom(1, NUMBER_RARE_SET + NUMBER_LEGEND_SET);
					} else {
						index = ReturnNumberRandom(NUMBER_RARE_SET + 1, NUMBER_RARE_SET + NUMBER_LEGEND_SET);
					}
					if(chestBanners.ContainsKey(index))
					{
						chestBanners.TryGetValue(index, out id);
						check = true;
					}
				}
				ShopItemCfg shopData = config.shopCfg.set[id.ToString()];
				bContent.textBtGo.text = shopData.Gem.ToString();
				bContent.shopData = shopData;
				Material newMaterial = new Material(Shader.Find("Unlit/Transparent Colored"));
				mgr.GetAsset<Texture2D>(setBannerPath+index, delegate (Texture2D mTexture){
					newMaterial.mainTexture = mTexture;
					bContent.texture.material = newMaterial;
				});
				bContent.disable.alpha = 0.7f;
				bContent.transform.localPosition = new Vector3(0,-300,0);
				bContent.transform.localScale = new Vector3 (0.1f,0.1f,0.1f);
				bContent.disable.transform.localScale = Vector3.one * 10;
				TweenParms parms = new TweenParms();
				parms.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine);
				HOTween.To(bContent.transform, 0.25f, parms);
				AddEventToButton(bContent.buttonLater,"HideBanner", bContent.gameObject);
				AddEventToButton(bContent.buttonGo,"BuyChest", bContent.gameObject);
			} else {
				config.UserData.SetDefeat(false);
			}
		}
	}

	void BuyChest(GameObject go)
	{
		routineRunner.StartCoroutine (BuyChestProgress(go));
	}

	IEnumerator BuyChestProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			ShopItemCfg shopData = go.GetComponent<BannerContent>().shopData;
			switch(shopService.BuyItem (shopData))
			{
				case ErrorCode.OK:	
					soundMng.PlayMusic(SoundName.BUY_ITEM_SUCCESS);
					ShowNotice(config.text.BuySuccessfully);
					config.UserData.SetItemUpdate(true);
					CheckHomeUpdate();
					break;
				case ErrorCode.NOT_ENOUGH_GOLD:
					ShowNoticePurchaseGold();
					break;
				case ErrorCode.NOT_ENOUGH_GEM:
					ShowNoticePurchaseGem();
					break;
				case ErrorCode.NOT_ENOUGH_SLOT:
					ShowNoticePurchaseSlot();
					break;
				default:
					ShowNotice(shopService.BuyItem (shopData).ToString());
					break;
			}
			HideBanner(go);
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	public void GoWorldMap()
	{
		ReturnBlackObject ();
		tutObject.SetActive (false);
		routineRunner.StartCoroutine (GoWorldMapProgress());
	}

	public IEnumerator GoWorldMapProgress()
	{
		yield return  new WaitForSeconds(0.5f);
		houseManager.GoHouse (GoToHouseManager.BackHome);
	}

	void GoShop(GameObject go)
	{
		HideBanner (go);
		houseManager.GoHouse (GoToHouseManager.ShopButton);
	}

	void GoDungeon(GameObject go)
	{
		HideBanner (go);
		goToDungeonSignal.Dispatch (data.dungeonChoose, data.nameDungeon, false);
	}

	void HideBanner(GameObject go)
	{
		BannerContent bContent = go.GetComponent<BannerContent>();
		bContent.buttonLater.onClick.Clear ();
		bContent.buttonGo.onClick.Clear ();

		if(bContent.disable != null)
		{
			TweenParms parms = new TweenParms();
			parms.Prop ("localScale", new Vector3 (20f, 20f, 20f)).Ease (EaseType.EaseOutSine);
			HOTween.To(bContent.disable.transform, 0.25f, parms);
		}
		config.UserData.SetDefeat(false);
		TweenParms parms2 = new TweenParms();
		parms2.Prop ("localScale", new Vector3 (0.1f, 0.1f, 0.1f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnPool), go, bannerPool);
		HOTween.To(bContent.transform, 0.25f, parms2);

	}

	private int ReturnNumberRandom(int min, int numb)
	{
		System.Random r = new System.Random ();
		int rs = r.Next (min,numb+1);
		return rs;
	}

	void SetStatShow(int stat, int statClone, UILabel label)
	{
		string text;
		text = statClone.ToString ();
		if(stat > statClone)
		{
			text += "(+"+(stat-statClone)+")";
		}
		label.text = text;
	}

	private void RemoveItemReceive()
	{
		int count = 0;
		for(int i = config.UserData.Inventory.ListItemData.Count - 1; i >=0; i--)
		{
			if(config.UserData.Inventory.ListItemData[i].Id == 1002)
			{
				config.UserData.Inventory.ListItemData.RemoveAt(i);
				count++;
			}
			if(count == 2) break;
		}
	}

	private void HandleGemBuying(int gemId, PaymentErrorCode error) {
		routineRunner.StartCoroutine(ProgressHide());
		switch(error)
		{
			case PaymentErrorCode.OK:
				break;
			default:
				routineRunner.StartCoroutine(ProgressError(config.text.ErrorCode));
				break;
		}
	}

	private IEnumerator TimeOutNotice()
	{
		yield return new WaitForSeconds(20f);
		routineRunner.StartCoroutine(ProgressHide());
	}

	public IEnumerator ProgressHide(bool onStart = false)
	{
		yield return new WaitForSeconds(0.5f);
		if(popup != null)
			HideNotice(popup);
		if(onStart)
			OnMethodStart();
	}

	private IEnumerator ProgressError(string s)
	{
		yield return new WaitForSeconds(0.5f);
		if(popup != null && popup.activeInHierarchy)
			HideNotice(popup);
		yield return new WaitForSeconds(0.25f);
		ShowNotice (s);
	}

	public void ShowLoading(bool isCheckAlpha = false)
	{
		popup = CreatePoolGameObject (loadingPool);
		popup.transform.parent = Panel.transform;
		popup.transform.localScale = Vector3.one;
		popup.transform.localPosition = new Vector3 (0,-420,0);
		GameObject.Find("DungeonLoading").GetComponent<UISprite>().alpha = 0.7f;
		if(isCheckAlpha)
		{
			int pos = (int)((-data.lastMapPosition.y + 480) / 960);
			if(pos > 0)
			{
				GameObject.Find("DungeonLoading").GetComponent<UISprite>().alpha = 1;
			}
		}
	}

	public IEnumerator ProgressHideLoading(bool onStart = false)
	{
		yield return new WaitForSeconds(0.5f);
		if(popup != null)
		{
			TweenParms parms2 = new TweenParms();
			parms2.Prop ("localScale", new Vector3 (1f, 1f, 1f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnPool), popup, loadingPool);
			HOTween.To(popup.transform, 0.1f, parms2);
		}
		if(onStart)
			OnMethodStart();
	}
}
