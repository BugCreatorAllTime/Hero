using strange.extensions.pool.api;
using strange.examples.strangerocks;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;

public class LoadInfoBlacksmith : BaseView {

	[Inject(PrefabWorldMap.itemFx)]
	public IPool<GameObject> itemFxPool { get; set; }
	[Inject(Prefabs.fx)]
	public IPool<GameObject> spinePool { get; set; }
	[Inject(PrefabWorldMap.popup)]
	public IPool<GameObject> popupPool { get; set; }
	[Inject(PrefabWorldMap.content)]
	public IPool<GameObject> contentPool { get; set; }
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public AssetMgr mgr{get; set;}
	[Inject]
	public ConfigManager config { get; set;}
	[Inject]
	public ItemService itemService { get; set; }
	[Inject]
	public LoadInfoTab loadTab { set; get; }
	[Inject]
	public ItemChestEffectManager upgradeMng { get; set;}
	[NGUITag]
	public UILabel Slot { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView1 { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView2 { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView3 { get; set;}
	[NGUITag]
	public UIGrid MaterialList { get; set;}
	[NGUITag]
	public UIPanel Panel { get; set;}
	[NGUITag]
	public SpriteRenderer bgSmith { get; set;}

	private List<ItemBaseData>[] itemList = new List<ItemBaseData>[3];
	private List<int>[] indexList = new List<int>[3];
	private MaterialContent[] material = new MaterialContent[5];
	private EndlessScroller[] scroll = new EndlessScroller[3];
	public UpdateContent smith;
	private int state = 0;
	private const int NO_ITEM = 0;
	private const int HAS_ITEM = 1;
	private const int HAS_MATERIAL = 2;
	private string SET = "SetItem";
	private string RETURNITEM = "ReturnItem";
	private string UPGRADE = "UpgradeItem";
	private string RETURNMATERIAL = "ReturnMaterial";
	private string IMAGE_NO_MATERIAL = "material";
	private string IMAGE_HAS_MATERIAL = "topslot";
	private int percent = 0;
	private GameObject tabPanel;
	private GameObject blackSmith;
	private bool touch = true;
	float slip = 100;

	public bool isUpgrade = false;
	
	[PostConstruct]
	public void PostConstruct()
	{
		tabPanel = GameObject.Find ("tabPanel");
		blackSmith = GameObject.Find ("BlackSmith");
	}

	protected override void OnStart ()
	{
		base.OnStart ();
		if (Screen.dpi != 0)
			slip = Screen.dpi/2;
		loadTab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
		LoadInfo ();
		LoadItem ();
		smith = GameObject.Find("Smith").GetComponent<UpdateContent>();
		for(int i = 0; i < MaterialList.transform.childCount; i++)
		{
			material[i] = MaterialList.GetChild(i).GetComponent<MaterialContent>();
			UIButton bt = material[i].gameObject.GetComponent<UIButton>();
			bt.tweenTarget = null;
			AddEventToButton(bt, RETURNMATERIAL, material[i].gameObject);
		}
		HideList ();
		upgradeMng.SetBlackSmithLink ();
		AddEventToButton(smith.btForge, UPGRADE, smith.gameObject);
//		gameObject.SetActive (false);
	}


	public void RefreshScreen()
	{
		ResetUpgradeBar ();
		bgSmith.gameObject.layer = 0;
		state = NO_ITEM;
		LoadListItem ();
		RefreshList (scroll[0], itemList[0], indexList[0],true);
		RefreshList (scroll[1], itemList[1], indexList[1],true);
		RefreshList (scroll[2], itemList[2], indexList[2],true);
		Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
	}

	void LoadInfo()
	{
		bgSmith.gameObject.layer = 0;
		InventoryModel inventory = config.UserData.Inventory;
		if(Slot != null)
			Slot.text = "Slot: "+inventory.ListItemData.Count + "/" + inventory.MaxSlot;
	}

	void LoadItem()
	{
		LoadListItem ();
		scroll [0] = ScrollView1;
		scroll [1] = ScrollView2;
		scroll [2] = ScrollView3;
		InitData(itemList[0], ScrollView1, indexList[0], itemService, config, WordManager.SMITH);
		InitData(itemList[1], ScrollView2, indexList[1], itemService, config, WordManager.SMITH);
		InitData(itemList[2], ScrollView3, indexList[2], itemService, config, WordManager.SMITH);
	}

	void InitData(List<ItemBaseData> itemList, EndlessScroller parent, List<int> indexList, ItemService itemService, ConfigManager configManager, int state)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetData (itemService, configManager, mgr, contentPool, state);
		wordManager.SetDataSmith (indexList);
		SetList (itemList, parent, indexList);
	}

	void SetList(List<ItemBaseData> itemList, EndlessScroller parent, List<int> indexList)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetDataSmith (indexList);
		wordManager.Init(itemList);
		if(itemList.Count == 0)
		{
			GameObject item = Utils.ButtonNullSlot(parent.gameObject,contentPool,"tabslot",0,-30,config.text.NullItem);
			RefreshPositionScrollView (parent);
		}
	}

	void LoadListItem()
	{
		for(int i = 0; i < 3; i++)
		{
			itemList[i] = new List<ItemBaseData>();
			indexList[i] = new List<int>();
		}
		InventoryModel inventory = config.UserData.Inventory;
		for(int i = 0; i < inventory.ListItemData.Count; i++)
		{
			switch(inventory.ListItemData[i].GetSlot())
			{
			case ItemCfg.WEAPON_SLOT:
				itemList[0].Add(inventory.ListItemData[i]);
				indexList[0].Add(i);
				break;
			case ItemCfg.ARMOR_SLOT:
				itemList[1].Add(inventory.ListItemData[i]);
				indexList[1].Add(i);
				break;
			case ItemCfg.SHIELD_SLOT:
				itemList[2].Add(inventory.ListItemData[i]);
				indexList[2].Add(i);
				break;
			default:
				break;
			}
		}
	}

	public void UpgradeItem(GameObject go)
	{
		routineRunner.StartCoroutine (UpgradeItemProgress(go));
	}

	IEnumerator UpgradeItemProgress(GameObject go)
	{
		if(touch)
		{
			if(state == HAS_MATERIAL)
			{
				touch = false;
				List<int> iList = new List<int>();
				for(int i = 0; i < 5; i++)
				{
					if(material[i].hasMaterial)
					{
						iList.Add(material[i].indexOfInventory);
					}
				}
				upgradeMng.UpgradeItem(go.GetComponent<UpdateContent>(),iList,smith.indexOfInventory);
				
			} else {
				touch = false;
				loadTab.ShowNotice(config.text.NoMaterial);
				yield return  new WaitForSeconds(0.5f);
				touch = true;
			}
		}
	}

	int GetPriceUpdate()
	{
		List<ItemBaseData> itemList = new List<ItemBaseData> ();
		for(int i = 0; i < 5; i++)
		{
			if(material[i].hasMaterial)
			{
				itemList.Add(material[i].item);
			}
		}
		return itemService.GetUpgradePrice (itemList, smith.item);
	}

	public void AfterUpgrade(ItemBaseData item)
	{
		LoadListItem ();
		RefreshList (scroll[0], itemList[0], indexList[0],false);
		RefreshList (scroll[1], itemList[1], indexList[1],false);
		RefreshList (scroll[2], itemList[2], indexList[2],false);
		state = NO_ITEM;
		ResetUpgradeBar();
	}

	public void SetItem(GameObject go)
	{
		routineRunner.StartCoroutine (SetItemProgress(go));
	}
	
	public IEnumerator SetItemProgress(GameObject go)
	{
		if(touch || isUpgrade)
		{
			isUpgrade = false;
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			switch(state)
			{
			case NO_ITEM:
				content = go.GetComponent<ItemContentManager> ();
				if(content.item.LevelUpgrade < config.general.MaxItemLevelUpgrade)
				{
					state = HAS_ITEM;
					smith.theme.spriteName = content.theme.spriteName;
					smith.icon.spriteName = content.icon.spriteName;
					smith.nameItem.text = content.nameItem.text;
					smith.theme.gameObject.SetActive(true);
					smith.btForge.gameObject.SetActive(true);
					smith.nameItem.gameObject.SetActive(true);
					smith.nameItem.color = content.nameItem.color;
					smith.percent.color = Color.white;
					smith.percent.text = "Success Rate: 0%";
					smith.cost.gameObject.SetActive (false);
					smith.description.gameObject.SetActive(false);
					smith.btForge.isEnabled = false;
					smith.type = content.type;
					smith.item = content.item.Clone();
					smith.indexOfInventory = content.indexOfInventory;
					itemList [content.type].RemoveAt (content.index);
					indexList[content.type].RemoveAt(content.index);
					RefreshList (go.transform.parent.GetComponent<EndlessScroller>(), itemList[content.type], indexList[content.type],false);
				} else {
					loadTab.ShowNotice(config.text.ItemLevelMax);
				}
				break;
			case HAS_ITEM:
				state = HAS_MATERIAL;
				smith.btForge.isEnabled = true;
				smith.percent.color = new Color32(8, 255, 19, 255);
				smith.percent.text = "Success Rate: "+percent + "%";
				smith.cost.gameObject.SetActive (true);
				smith.cost.text = GetPriceUpdate().ToString();
				content = go.GetComponent<ItemContentManager> ();
				routineRunner.StartCoroutine(AddMaterial(content,go.transform.parent.GetComponent<EndlessScroller>()));
				break;
			case HAS_MATERIAL:
				if(CountMaterial() < 5)
				{
					content = go.GetComponent<ItemContentManager> ();
					routineRunner.StartCoroutine(AddMaterial(content,go.transform.parent.GetComponent<EndlessScroller>()));
				} else {
					loadTab.ShowNotice(config.text.FullMaterial);
				}
				break;
			}
			touch = false;
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	IEnumerator AddMaterial(ItemContentManager content, EndlessScroller scroll)
	{
		if(touch)
		{
			int index = 0;
			for(int i = 0; i < 5; i++)
			{
				if(!material[i].hasMaterial)
				{
					index = i;
					break;
				}
			}
			MaterialContent myMaterial = material[index];
			myMaterial.theme.gameObject.SetActive (true);
			myMaterial.item = content.item.Clone();
			myMaterial.image.spriteName = IMAGE_HAS_MATERIAL;
			myMaterial.theme.spriteName = content.theme.spriteName;
			myMaterial.icon.spriteName = content.icon.spriteName;
			myMaterial.hasMaterial = true;
			myMaterial.indexOfInventory = content.indexOfInventory;
			itemList [content.type].RemoveAt (content.index);
			indexList[content.type].RemoveAt(content.index);
			RefreshList (scroll, itemList[content.type],indexList[content.type],false);
			CalculatorPercent ();
			Transform sTransform = scroll.transform;
			touch = false;
		}
		yield return  new WaitForSeconds(0.3f);
		touch = true;
	}

	void CalculatorPercent()
	{
		List<ItemBaseData> listMaterial = new List<ItemBaseData> ();
		bool check = false;
		for(int i = 0; i < 5; i++)
		{
			if(material[i].hasMaterial)
			{
				check = true;
				listMaterial.Add(material[i].item);
			}
		}
		smith.percent.color = new Color32(8, 255, 19, 255);
		smith.percent.text = "Success Rate: "+itemService.GetUpgradeChance (listMaterial,smith.item)+"%";
		smith.cost.gameObject.SetActive (true);
		smith.cost.text = GetPriceUpdate().ToString();
		if(check)
			smith.nameItem.text = config.ItemCfg.GetItemByItemId (smith.item.Id).Name +" + "+(smith.item.LevelUpgrade + 1);
		else {
			smith.nameItem.text = config.ItemCfg.GetItemByItemId (smith.item.Id).Name +" + "+smith.item.LevelUpgrade;
			smith.btForge.isEnabled = false;
			smith.percent.color = Color.white;
			smith.percent.text = "Success Rate: 0%";
			smith.cost.gameObject.SetActive (false);
			state = HAS_ITEM;
		}
	}

	public void ReturnItem(GameObject go)
	{
		if(go != null)
		{
			if(state == HAS_ITEM || state == HAS_MATERIAL)
			{
				state = NO_ITEM;
				ResetUpgradeBar();
				UpdateContent content = go.GetComponent<UpdateContent> ();
				itemList [content.type].Add (content.item);
				for(int i = 0; i < 5; i++)
				{
					if(material[i].hasMaterial)
					{
						RemoveMaterial(material[i]);
					}
				}
				LoadListItem();
				RefreshList (scroll[0], itemList[0],indexList[0],false);
				RefreshList (scroll[1], itemList[1],indexList[1],false);
				RefreshList (scroll[2], itemList[2],indexList[2],false);
			}
		}

	}

	public void ReturnMaterial(GameObject go)
	{
		MaterialContent mContent = go.GetComponent<MaterialContent> ();
		if(mContent.item != null && mContent.item.Id != 0)
		{
			int index = (mContent.item.GetSlot()+2)%3;
			RemoveMaterial (mContent);
			CalculatorPercent ();
			RefreshList (scroll[index], itemList[index],indexList[index],false);
		}

	}

	void RemoveMaterial(MaterialContent myMaterial)
	{
		myMaterial.hasMaterial = false;
		itemList [(myMaterial.item.GetSlot()+2)%3].Add (myMaterial.item);
		indexList [(myMaterial.item.GetSlot () + 2) % 3].Add (myMaterial.indexOfInventory);
		myMaterial.item = null;
		myMaterial.theme.gameObject.SetActive(false);
		myMaterial.image.spriteName = IMAGE_NO_MATERIAL;
	}

	public void ShowInfo(GameObject go)
	{
		routineRunner.StartCoroutine (ShowInfoProgress(go));
	}

	public IEnumerator ShowInfoProgress(GameObject go)
	{
		if(touch)
		{
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
//			soundMng.PlaySound (SoundName.SHOW_INFO);
			if(!content.contentDes.gameObject.activeInHierarchy)
			{
				content.contentDes.gameObject.SetActive (true);
				TweenParms parms = new TweenParms();
				parms.Prop("localPosition", content.btContent.transform.localPosition).Ease(EaseType.EaseOutSine);
				HOTween.To(content.contentDes.transform, 0.5f, parms);
				TweenParms parms2 = new TweenParms();
				parms2.Prop("localRotation", new Vector3(0,180,0)).Ease(EaseType.EaseOutSine);
				HOTween.To(content.btInfo.transform, 0.25f, parms2);
			}
			else {
				TweenParms parms = new TweenParms();
				Vector3 mPosition = new Vector3(565, content.btContent.transform.localPosition.y,0);
				parms.Prop("localPosition", mPosition).Ease(EaseType.EaseOutSine).OnComplete(new TweenDelegate.TweenCallbackWParms(CompleteMove), content.contentDes.gameObject);
				HOTween.To(content.contentDes.transform, 0.5f, parms);
				TweenParms parms2 = new TweenParms();
				parms2.Prop("localRotation", new Vector3(0,0,0)).Ease(EaseType.EaseOutSine);
				HOTween.To(content.btInfo.transform, 0.25f, parms2);
			}
			touch = false;
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}
	
	void CompleteMove(TweenEvent tweenEvent)
	{
		((GameObject)tweenEvent.parms[0]).SetActive(false);
	}

	void RefreshList(EndlessScroller mParent, List<ItemBaseData> list, List<int> indexList, bool resetPos)
	{
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go,contentPool);
		}
		SetList(list, mParent, indexList);
		mParent.Drop ();
		if (resetPos)
			mParent.transform.localPosition = new Vector3 (-15,0,0);
	}

	void RefreshPositionScrollView(EndlessScroller scrollView)
	{
		scrollView.Drop ();
	}

	void HideList()
	{
		ScrollView2.gameObject.SetActive (false);
		ScrollView3.gameObject.SetActive (false);
		ResetUpgradeBar ();
		AddEventToButton (smith.returnItem, RETURNITEM, smith.gameObject);
	}

	void ResetUpgradeBar()
	{
		smith.percent.color = Color.white;
		smith.percent.text = "Success Rate: 0%";
		smith.cost.gameObject.SetActive (false);
		smith.description.gameObject.SetActive(true);
		smith.btForge.gameObject.SetActive (false);
		smith.nameItem.gameObject.SetActive (false);
		smith.theme.gameObject.SetActive (false);
		for(int i = 0; i < 5; i++)
		{
			if(material[i].hasMaterial)
			{
				material[i].hasMaterial = false;
				material[i].item = null;
				material[i].theme.gameObject.SetActive(false);
				material[i].image.spriteName = IMAGE_NO_MATERIAL;
			}

		}
	}
	
	void HideNotice(GameObject go)
	{
		go.GetComponent<PopUpContent> ().btHide.onClick.Clear ();
		ReturnInstance (go, popupPool);
	}

	int CountMaterial()
	{
		int count = 0;
		for(int i = 0; i < material.Length; i++)
		{
			if(material[i].hasMaterial) count++;
		}
		return count;
	}

	public void AddEventToButton(UIButton button, string nameEvent, GameObject item)
	{
		EventDelegate eventButton = new EventDelegate(this, nameEvent);
		eventButton.parameters[0] = new EventDelegate.Parameter(item, "go");
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
		contentObject.SetActive (false);
		contentPool.ReturnInstance(contentObject);
	}

	void HideUpgrade(GameObject go)
	{
		touch = true;
		ItemEfContent eContent = go.GetComponent<ItemEfContent> ();
		eContent.returnScreen.onClick.Clear ();
		blackSmith.SetActive (true);
		tabPanel.SetActive (true);
		GameObject fx = GameObject.Find ("fxShow");
		if(fx != null)
			ReturnInstance (fx, spinePool);
		ReturnInstance (eContent.gameObject, itemFxPool);
		RefreshScreen ();
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.END)
		{
			routineRunner.StartCoroutine(loadTab.CheckTut(0.25f));
		}
	}

	public void InsertItemInTutBlackSmith()
	{
		for(int i = 0; i < 2; i++)
		{
			ItemCfgImpl iConfig = config.ItemCfg.GetItemByItemId(1002);
			ItemBaseData item = new ItemBaseData(iConfig);
			config.UserData.Inventory.InsertItemToFirst(item);
		}
		config.UserData.AddGold (60);
		RefreshScreen ();
	}
}
