using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Collections;
using strange.extensions.pool.api;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using System.Linq;
using strange.examples.strangerocks;

public class LoadInfoCharacter : BaseView 
{
	[Inject(PrefabWorldMap.itemFx)]
	public IPool<GameObject> itemFxPool { get; set; }
	[Inject(Prefabs.fx)]
	public IPool<GameObject> spinePool { get; set; }
	[Inject(PrefabWorldMap.purchase)]
	public IPool<GameObject> purchasePool { get; set; }
	[Inject(PrefabWorldMap.content)]
	public IPool<GameObject> contentPool { get; set; }
	[Inject(PrefabWorldMap.popup)]
	public IPool<GameObject> popupPool { get; set; }
	[Inject(PrefabWorldMap.iconItem)]
	public IPool<GameObject> setItemPool { get; set; }
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public AssetMgr mgr{get; set;}
	[Inject]
	public ConfigManager config { get; set;}
	[Inject]
	public ItemService itemService { get; set; }
	[Inject]
	public ChestService chestService { get; set; }
	[Inject]
	public LoadInfoTab loadTab { get; set;}
	[Inject]
	public ItemChestEffectManager openChestMng { get; set;}
	[Inject]
	public SoundManager soundMng { get; set;}

	[NGUITag]
	public UILabel HP { get; set;}
	[NGUITag]
	public UILabel Armor { get; set;}
	[NGUITag]
	public UILabel Attack { get; set;}
	[NGUITag]
	public UILabel NumbWeight { get; set;}
	[NGUITag]
	public UILabel Slot { get; set;}
	[NGUITag]
	public UILabel NameCharacter { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView1 { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView2 { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView3 { get; set;}
	[NGUITag]
	public EndlessScroller ScrollView4 { get; set;}
	[NGUITag]
	public UIPanel Panel { get; set;}
	[NGUITag]
	public SpriteRenderer bgHome { get; set;}
	
	private List<ItemBaseData> weaponList;
	private List<ItemBaseData> armorList;
	private List<ItemBaseData> shieldList;
	private List<ItemBaseData> chestIList;
	int[] indexItem = new int[3] {-1,-1,-1};
	private string imageNormal = "tabslot";
	private string imageSelect = "tabitemselect";
	GameObject player;
	SkeletonAnimation skeletonAnimation;
	private GameObject tabPanel;
	private GameObject home;
	private bool touch = true;
	private bool isOpen = false;
	public const string ITEM = "Item";
	public const string CHEST = "Chest";
	PopUpContent pContent;

	[PostConstruct]
	public void PostConstruct()
	{
		tabPanel = GameObject.Find ("tabPanel");
		home = GameObject.Find ("Home");
	}

	protected override void OnStart ()
	{
		base.OnStart ();
		CreateCharacter ();
		SetAnchors (0, 0, 0, 0);
		openChestMng.SetHomeLink ();
//		gameObject.SetActive (false);

	}

	public void RefreshScreen()
	{
		player.layer = 0;
		SkeletonAnimation skeletonAnimation = player.GetComponent<SkeletonAnimation>();
		string skin;
		if(config.UserData.CharacterID == 0) skin = "male";
		else skin = "female";
		skeletonAnimation.skeletonDataAsset.Reset();
		skeletonAnimation.Reset();
		skeletonAnimation.skeleton.SetSkin(skin);
		skeletonAnimation.skeleton.SetSlotsToSetupPose();
		skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
		ChangeItem ();
		bgHome.gameObject.layer = 0;
		SetItemToList ();
		RefreshList (ScrollView1, weaponList, indexItem[0]);
		RefreshList (ScrollView2, armorList, indexItem[1]);
		RefreshList (ScrollView3, shieldList, indexItem[2]);
		ScrollView4.transform.localPosition = new Vector3 (-15,0,0);
		RefreshList (ScrollView4);
		SetOtherList ();
		Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
	}

	void RefreshList(EndlessScroller mParent, List<ItemBaseData> list, int indexItem, bool trans = true)
	{
		List<ItemBaseData> listItem = config.UserData.EquippedItemData;
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go, contentPool);
		}
		SetList(list, mParent, indexItem);
		if(trans)
		{
			mParent.Drop ();
			mParent.transform.localPosition = new Vector3 (-15,0,0);
		}
	}

	void RefreshPositionScrollView(UIScrollView scrollView)
	{
		scrollView.transform.localPosition = new Vector3 (scrollView.transform.localPosition.x,214,0);
		UIPanel mPanel = scrollView.gameObject.GetComponent<UIPanel> ();
		mPanel.clipOffset = new Vector2 (0,0);
	}

	void RefreshList(EndlessScroller mParent)
	{
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go, contentPool);
		}
		ScrollView4.Drop ();
	}
	
	public void ClickEquip(GameObject go)
	{
		ItemContentManager content = go.GetComponent<ItemContentManager> ();
		WordManager wordManager = go.transform.parent.GetComponent<WordManager>();
		if(content.index == indexItem[content.type])
		{
			switch(itemService.UnequipItem(go.GetComponent<ItemContentManager> ().item, config.UserData))
			{
				case ErrorCode.OK:
					wordManager.SetDataHome (-1);
					indexItem[content.type] = -1;
					soundMng.PlaySound (SoundName.WEAR_ITEM);
					itemService.TakeOffItemView(skeletonAnimation, go.GetComponent<ItemContentManager> ().item);
					WearItem (go,true);
					break;
				case ErrorCode.NOT_ENOUGH_WEIGHT:
					loadTab.ShowNotice(config.text.NotEnoughWeigh);
					break;
				default:
					loadTab.ShowNotice(itemService.UnequipItem(go.GetComponent<ItemContentManager> ().item, config.UserData).ToString());
					break;
			}

		} else{
			switch(itemService.EquipItem(go.GetComponent<ItemContentManager> ().item, config.UserData))
			{
				case ErrorCode.OK:
					soundMng.PlaySound (SoundName.WEAR_ITEM);
					wordManager.SetDataHome (content.index);
					indexItem[content.type] = content.index;
					ChangeItem ();
					WearItem (go,false);
					break;
				case ErrorCode.NOT_ENOUGH_WEIGHT:
					loadTab.ShowNotice(config.text.NotEnoughWeigh);
					break;
				case ErrorCode.NOT_ENOUGH_SLOT:
					loadTab.ShowNoticePurchaseSlot();
					break;
				default:
					loadTab.ShowNotice(itemService.EquipItem(go.GetComponent<ItemContentManager> ().item, config.UserData).ToString());
					break;
			}
		}
		int currentWeight = 0;
		for(int i = 0; i < config.UserData.EquippedItemData.Count; i++)
		{
			currentWeight += itemService.GetItemWeight(config.ItemCfg.GetItemByItemId(config.UserData.EquippedItemData[i].Id));
		}
		RefreshList (ScrollView1, weaponList, indexItem[0], false);
		RefreshList (ScrollView2, armorList, indexItem[1], false);
		RefreshList (ScrollView3, shieldList, indexItem[2], false);
		Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		NumbWeight.text = currentWeight+"/"+config.UserData.GetWeight();
		ShowStat ();
	}

	void ShowStat()
	{
		HeroStat hStat = itemService.GetHeroStat(config.UserData);
		HeroStat cStat = itemService.GetHeroStat(config.UserData, false);
		SetStatShow (hStat.GetDmg(), cStat.GetDmg(), Attack);
		SetStatShow (hStat.GetMaxHp(), cStat.GetMaxHp(), HP);
		SetStatShow (hStat.GetMaxArmor(), cStat.GetMaxArmor(), Armor);
	}

	void SetStatShow(int stat, int statClone, UILabel label)
	{
		string text;
		text = statClone.ToString ();
		if(stat > statClone)
		{
			text += "(+"+(stat-statClone)+")";
			label.color = Color.green;
		} else {
			label.color = Color.white;
		}
		label.text = text;
	}

	void HideNotice(GameObject go)
	{
		go.GetComponent<PopUpContent> ().btHideIcon.onClick.Clear ();
		ReturnInstance (go, popupPool);
	}

	public void InfoChest(GameObject go)
	{
		routineRunner.StartCoroutine (InfoChestProgress(go));
	}
	
	public IEnumerator InfoChestProgress(GameObject go)
	{
		if(touch)
		{
			soundMng.PlaySound (SoundName.BUTTON_CLICK);
			GameObject popup = CreatePoolGameObject (popupPool);
			popup.transform.parent = Panel.transform;
			popup.transform.localScale = Vector3.one;
			pContent = popup.GetComponent<PopUpContent>();
			pContent.chestDisable.alpha = 0.7f;
			pContent.btHide.gameObject.SetActive (false);
			pContent.btHideIcon.gameObject.SetActive (true);
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			ChestCfgImplement chestOpen = config.chestCfg.GetChestCfg (content.index);
			pContent.nameChest.text = chestOpen.Name;
			pContent.iconChest.spriteName = chestOpen.Icon;
			pContent.border.spriteName = chestOpen.Border;
			int checkSet = -1;
			int countSet = -1;
			int countItem = -1;
			SetItemContent setContent = null;
			int countItemSet = 0;
			for(int i = 0; i < chestOpen.FixItemIds.Length; i++)
			{
				ItemCfgImpl item = config.ItemCfg.GetItemByItemId(chestOpen.FixItemIds[i]);
				if(countItemSet == 0)
				{
					checkSet = item.SetId;
					GameObject set = CreatePoolGameObject(setItemPool);
					setContent = set.GetComponent<SetItemContent>();
					for(int j = 0; j < 3; j++)
					{
						setContent.item[j].gameObject.SetActive(true);
						setContent.icon[j].gameObject.SetActive(true);
						setContent.random[j].gameObject.SetActive(true);
					}
					setContent.item[0].transform.localPosition = new Vector3(-105,-100,0);
					setContent.item[1].transform.localPosition = new Vector3(0,-100,0);
					setContent.item[2].transform.localPosition = new Vector3(105,-100,0);
					countSet++;
					countItem = -1;
					CreatSetIcon(set,countSet,setContent,item,pContent);
				}
				countItemSet++;
				if(countItemSet == 3) countItemSet = 0;
				countItem++;
				setContent.random[countItem].gameObject.SetActive(false);
				setContent.icon[countItem].spriteName = item.Icon;
				switch(item.Color)
				{
				case 0:
					setContent.theme[countItem].spriteName = config.text.NormalItem;
					break;
				case 1:
					setContent.theme[countItem].spriteName = config.text.RareItem;
					break;
				case 2:
					setContent.theme[countItem].spriteName = config.text.LegendItem;
					break;
				}
			}
			if(chestOpen.RandomItem.Ids.Length > 0)
			{
				GameObject setRandom = CreatePoolGameObject(setItemPool);
				setContent = setRandom.GetComponent<SetItemContent>();
				for(int j = 0; j < 3; j++)
				{
					setContent.item[j].gameObject.SetActive(true);
					setContent.icon[j].gameObject.SetActive(true);
					setContent.random[j].gameObject.SetActive(true);
				}
				setContent.item[0].transform.localPosition = new Vector3(-105,-100,0);
				setContent.item[1].transform.localPosition = new Vector3(0,-100,0);
				setContent.item[2].transform.localPosition = new Vector3(105,-100,0);
				countSet++;
				countItem = -1;
				CreatSetIcon(setRandom,countSet,setContent,null,pContent);
				List<int> colorList = new List<int>();
				for(int i = 0; i < chestOpen.RandomItem.Ids.Length; i++)
				{
					ItemCfgImpl item = config.ItemCfg.GetItemByItemId(chestOpen.RandomItem.Ids[i]);
					colorList = GetColor(colorList, item.Color);
				}
				int countRItem = 0;
				for(int i = 0; i < 3; i++)
				{
					if(i+1 > colorList.Count)
					{
						setContent.item[i].gameObject.SetActive(false);
					} else {
						setContent.icon[i].gameObject.SetActive(false);
						switch(colorList[i])
						{
						case 0:
							setContent.theme[i].spriteName = config.text.NormalItem;
							break;
						case 1:
							setContent.theme[i].spriteName = config.text.RareItem;
							break;
						case 2:
							setContent.theme[i].spriteName = config.text.LegendItem;
							break;
						}
						countRItem++;
					}
				}
				if(countRItem == 2)
				{
					setContent.item[0].transform.localPosition = new Vector3(-55,setContent.item[0].transform.localPosition.y,0);
					setContent.item[1].transform.localPosition = new Vector3(55,setContent.item[1].transform.localPosition.y,0);
				}
				if(countRItem == 1)
				{
					setContent.item[0].transform.localPosition = new Vector3(0,setContent.item[0].transform.localPosition.y,0);
				}
			}
			pContent.imgContentChest.width = 450;
			pContent.imgContentChest.height = 140 + (countSet+1) * 140;
			pContent.border.height = 165 + (countSet+1) * 140;
			pContent.decor.height = 65 + (countSet+1) * 140;
			pContent.imgContentChest.gameObject.GetComponent<BoxCollider> ().size = new Vector3 (450,pContent.imgContentChest.height,0);
			pContent.imgContentChest.transform.localPosition = new Vector3 (-230,pContent.imgContentChest.height/2,0);
			BoxCollider bCollider = pContent.btHideIcon.GetComponent<BoxCollider>();
			bCollider.size = new Vector3 (640,960,0);
			float yBt = pContent.border.transform.localPosition.y - pContent.border.height-30;
			pContent.openChest.gameObject.SetActive(true);
			pContent.openChest.transform.localPosition = new Vector3(0,yBt,0);
			AddEventToButton(pContent.openChest,"OpenChest", content.gameObject);
			AddEventToButton(pContent.btHideIcon,"HideNoticeChest", pContent.imgContentChest.gameObject);
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	void HideNoticeChest(GameObject go)
	{
		go.transform.parent.transform.GetComponent<PopUpContent> ().btHide.onClick.Clear ();
		for(int i = 0; i < go.transform.childCount; i++)
		{
			GameObject child = go.transform.GetChild(i).gameObject;
			if(child.name != "ContentSet" && child.name != "DisableInput")
				ReturnInstance (child, setItemPool);
		}
		ReturnInstance (go.transform.parent.gameObject,popupPool);
	}

	List<int> GetColor(List<int> list, int color)
	{
		for(int i = 0; i < list.Count; i++)
		{
			if(i == color)
				return list;
		}
		list.Add (color);
		return list;
	}

	void CreatSetIcon(GameObject set, int countSet,SetItemContent setContent, ItemCfgImpl item, PopUpContent pContent)
	{
		UIWidget widget = set.GetComponent<UIWidget>();
		widget.pivot = UIWidget.Pivot.Top;
		setContent = set.GetComponent<SetItemContent>();
		if(item != null)
			setContent.nameSet.text = item.NameOfSet;
		else setContent.nameSet.text = "Random Item";
		set.transform.parent = pContent.btHideIcon.transform;
		set.transform.localPosition = new Vector3(220,-80 - 130*countSet,0);
		set.transform.localScale = Vector3.one;
	}

	void OpenChest(GameObject go)
	{
		routineRunner.StartCoroutine (OpenChestProgress(go));

	}

	IEnumerator OpenChestProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			if(pContent != null)
			{
				pContent.gameObject.SetActive(false);
				pContent = null;
			}
			if(!isOpen)
			{
				isOpen = true;
				routineRunner.StartCoroutine (ResetOpen());
				ItemContentManager content = go.GetComponent<ItemContentManager> ();	
				openChestMng.OpenChest (content);
			}

		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	IEnumerator ResetOpen()
	{
		yield return  new WaitForSeconds(2f);
		isOpen = false;
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
			soundMng.PlaySound (SoundName.SHOW_INFO);
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

	void SetItemToList()
	{
		weaponList = new List<ItemBaseData>();
		armorList = new List<ItemBaseData>();
		shieldList = new List<ItemBaseData>();
		
		for(int i = 0; i < config.UserData.EquippedItemData.Count; i++)
		{
			switch(config.UserData.EquippedItemData[i].GetSlot())
			{
			case ItemCfg.WEAPON_SLOT:
				weaponList.Add(config.UserData.EquippedItemData[i]);
				indexItem[0] = 0;
				break;
			case ItemCfg.ARMOR_SLOT:
				armorList.Add(config.UserData.EquippedItemData[i]);
				indexItem[1] = 0;
				break;
			case ItemCfg.SHIELD_SLOT:
				shieldList.Add(config.UserData.EquippedItemData[i]);
				indexItem[2] = 0;
				break;
			default:
				break;
			}
		}
		
		for(int i = 0; i < config.UserData.Inventory.ListItemData.Count; i++)
		{
			switch(config.UserData.Inventory.ListItemData[i].GetSlot())
			{
			case ItemCfg.WEAPON_SLOT:
				weaponList.Add(config.UserData.Inventory.ListItemData[i]);
				break;
			case ItemCfg.ARMOR_SLOT:
				armorList.Add(config.UserData.Inventory.ListItemData[i]);
				break;
			case ItemCfg.SHIELD_SLOT:
				shieldList.Add(config.UserData.Inventory.ListItemData[i]);
				break;
			default:
				break;
			}
		}
		ItemBaseData slot = new ItemBaseData();
		slot.Id = -9999;
		weaponList.Add (slot);
		armorList.Add (slot);
		shieldList.Add (slot);
	}

	public void LoadInfoUser()
	{
		bgHome.gameObject.layer = 0;
		player.gameObject.layer = 0;
		int attack = 0, armor = 0, hp = 0;
		List<ItemBaseData> equipItem = config.UserData.EquippedItemData;
		SetItemToList ();
		InitData(weaponList, indexItem[0], ScrollView1, itemService, config, WordManager.HOME);
		InitData(armorList, indexItem[1],ScrollView2, itemService, config, WordManager.HOME);
		InitData(shieldList, indexItem[2],ScrollView3, itemService, config, WordManager.HOME);
		SetOtherList ();
		ShowStat ();
		int currentWeight = 0;
		for(int i = 0; i < equipItem.Count; i++)
		{
			currentWeight += itemService.GetItemWeight(config.ItemCfg.GetItemByItemId(equipItem[i].Id));
		}
		NumbWeight.text = currentWeight+"/"+config.UserData.GetWeight();
		if(Slot != null)
			Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		ScrollView2.transform.parent.transform.parent.gameObject.SetActive (false);
		ScrollView3.transform.parent.transform.parent.gameObject.SetActive (false);
		ScrollView4.transform.parent.transform.parent.gameObject.SetActive (false);
	}

	void CreateCharacter()
	{
		mgr.GetAsset<GameObject>("Prefabs/Battle/SimpleObj", CreateCharacterLoad);
	}

	private void CreateCharacterLoad(GameObject go)
	{
		player = GameObject.Instantiate(go) as GameObject;
		player.transform.parent = gameObject.transform;
		CharacterData characterData = config.CharacterCfg.character[config.UserData.CharacterID.ToString()];
		player.name = characterData.Name;
		player.transform.localScale = new Vector3 (460,460,460);

		mgr.GetAsset<SkeletonDataAsset>("Animation/Character/Character.ske", delegate (SkeletonDataAsset skele){
			skeletonAnimation = player.GetComponent<SkeletonAnimation>();
			skeletonAnimation.skeletonDataAsset = skele;
			skeletonAnimation.skeletonDataAsset.Reset();
			skeletonAnimation.Reset();
			skeletonAnimation.skeleton.SetSkin(characterData.Name);
			skeletonAnimation.skeleton.SetSlotsToSetupPose();
			skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
			ChangeItem ();
			player.transform.localPosition = new Vector2(-37166, 45391);
			LoadInfoUser ();
		});
	}

	public void FillColorToButton(UIButton button, string image)
	{
		button.normalSprite = image;
		button.hoverSprite = image;
		button.pressedSprite = image;
	}

	public void WearItem(GameObject go, bool check)
	{
		ItemContentManager content = go.GetComponent<ItemContentManager> ();
		UIButton button = content.btContent;
		for(int i = 0; i < go.transform.parent.transform.childCount; i++)
		{
			if(go.transform.parent.GetChild(i).transform != go.transform || check)
			{

				UIButton buttonOther = go.transform.parent.GetChild(i).GetComponent<ItemContentManager> ().btContent;
				FillColorToButton(buttonOther, imageNormal);
			} else {
				FillColorToButton(button, imageSelect);
			}
			ItemContentManager contentOther =  go.transform.parent.GetChild(i).GetComponent<ItemContentManager> ();
			Stat iStat = itemService.GetItemStat(content.item);
			Stat cStat = iStat;
			if(content.item.LevelUpgrade > 0)
			{
				ItemBaseData itemClone = content.item.CloneStart ();
				cStat = itemService.GetItemStat(itemClone);
			}
			if(!check)
			{
				if(contentOther.numberDame.gameObject.activeInHierarchy)
				{
					if(!content.numberDame.gameObject.activeInHierarchy)
					{
						contentOther.numberDame.color = Color.green;
					} else {
						if(contentOther.stat.damage > content.stat.damage)
						{
							contentOther.numberDame.color = Color.green;
						}
						if(contentOther.stat.damage == content.stat.damage)
						{
							contentOther.numberDame.color = Color.white;
						}
						if(contentOther.stat.damage < content.stat.damage)
						{
							contentOther.numberDame.color = Color.red;
						}
					}
				}
				if(contentOther.numberHp.gameObject.activeInHierarchy)
				{
					if(!content.numberHp.gameObject.activeInHierarchy)
					{
						contentOther.numberHp.color = Color.green;
					} else {
						if(contentOther.stat.hp > content.stat.hp)
						{
							contentOther.numberHp.color = Color.green;
						}
						if(contentOther.stat.hp == content.stat.hp)
						{
							contentOther.numberHp.color = Color.white;
						}
						if(contentOther.stat.hp < content.stat.hp)
						{
							contentOther.numberHp.color = Color.red;
						}
					}
				}
				if(contentOther.numberArmor.gameObject.activeInHierarchy)
				{
					if(!content.numberArmor.gameObject.activeInHierarchy)
					{
						contentOther.numberArmor.color = Color.green;
					} else {
						if(contentOther.stat.armor > content.stat.armor)
						{
							contentOther.numberArmor.color = Color.green;
						}
						if(contentOther.stat.armor == content.stat.armor)
						{
							contentOther.numberArmor.color = Color.white;
						}
						if(contentOther.stat.armor < content.stat.armor)
						{
							contentOther.numberArmor.color = Color.red;
						}
					}
				}
			} else {
				contentOther.numberDame.color = Color.green;
				contentOther.numberHp.color = Color.green;
				contentOther.numberArmor.color = Color.green;
			}
		}
	}

	void ChangeItem()
	{
		itemService.WearItemView(skeletonAnimation, config.UserData.EquippedItemData, config.UserData.GetCharacterName(), config.UserData.IsActiveSet());
	}

	void SetOtherList()
	{
		List<ChestData> chestList = config.UserData.ChestModel.chests;
		chestIList = new List<ItemBaseData>();
		for(int i = 0; i < chestList.Count; i++)
		{
			ItemBaseData item = new ItemBaseData();
			item.Id = chestList[i].chestId + 6000;
			chestIList.Add(item);
		}
		ItemBaseData slot = new ItemBaseData();
		slot.Id = -9999;
		chestIList.Add (slot);
		InitData(chestIList, 3, ScrollView4, itemService, config, WordManager.HOME);
		ScrollView4.Drop ();
		ScrollView4.gameObject.GetComponent<BoxCollider> ().enabled = true;
	}

	void InitData(List<ItemBaseData> itemList, int indexItem,EndlessScroller parent, ItemService itemService, ConfigManager configManager, int state)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetData (itemService, configManager, mgr, contentPool, state);
		wordManager.SetDataHome (indexItem);
		SetList (itemList, parent, indexItem);
	}

	void SetList(List<ItemBaseData> itemList, EndlessScroller parent, int indexItem)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetDataHome (indexItem);
		wordManager.Init(itemList);
	}

	void ButtonAddSlot(EndlessScroller parent, int count)
	{
		GameObject item = CreatePoolGameObject(contentPool);
		item.name = "Item";
		item.transform.parent = parent.transform;
		ItemContentManager content = item.GetComponent<ItemContentManager>();
		FillColorToButton(content.btContent, imageNormal);
		item.transform.localPosition = new Vector3(3, -80-160*count,0);
		content.icon.spriteName = "AddSlot";
		content.buttonSell.gameObject.SetActive(false);
		content.nameItem.text = "Buy Slot";
		content.nameItem.color = Color.white;
		content.iconArmor.gameObject.SetActive (false);
		content.iconDame.gameObject.SetActive (false);
		content.iconHp.gameObject.SetActive (false);
		content.iconWeight.gameObject.SetActive (false);
		content.btInfo.gameObject.SetActive (false);
		content.icon.width = 100;
		content.icon.height = 100;
		item.transform.localScale = Vector3.one;
	}

	public void BuySlot(GameObject go)
	{
		loadTab.BuySlot (go);
	}

	public void AddEventToButton(UIButton button, string nameEvent, GameObject item)
	{
		routineRunner.StartCoroutine (AddEventToButtonProgress(button, nameEvent, item));
	}
	
	public IEnumerator AddEventToButtonProgress(UIButton button, string nameEvent, GameObject item)
	{
		EventDelegate eventButton = new EventDelegate(this, nameEvent);
		eventButton.parameters[0] = new EventDelegate.Parameter(item, "go");
		button.onClick.Add(eventButton);
		item.transform.localScale = Vector3.one;
		yield return  new WaitForSeconds(0.5f);
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

	void HideOpenChest(GameObject go)
	{
		ItemEfContent eContent = go.GetComponent<ItemEfContent> ();
		eContent.returnScreen.onClick.Clear ();
		home.SetActive (true);
		tabPanel.SetActive (true);
		GameObject fx = GameObject.Find ("fxShow");
		ReturnInstance (fx, spinePool);
		ReturnInstance (eContent.gameObject, itemFxPool);
		RefreshScreen ();
	}

	void CompleteOpenChest(GameObject go)
	{
		routineRunner.StartCoroutine (CompleteOpenChestProgress(go));
	}

	IEnumerator CompleteOpenChestProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			openChestMng.CompleOpenChest (go);
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}
}