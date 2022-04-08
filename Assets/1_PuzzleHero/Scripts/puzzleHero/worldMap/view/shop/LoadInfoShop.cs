using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Collections;
using strange.extensions.pool.api;
using strange.examples.strangerocks;
using System.Linq;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;

public class LoadInfoShop : BaseView {

	[Inject(PrefabWorldMap.iconItem)]
	public IPool<GameObject> setItemPool { get; set; }
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
	public ShopService shopService { get; set; }
	[Inject]
	public LoadInfoTab loadTab { get; set; }
	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public GA ga { get; set; }

	[NGUITag]
	public UILabel Slot { get; set;}
	[NGUITag]
	public UILabel HP { get; set;}
	[NGUITag]
	public UILabel Armor { get; set;}
	[NGUITag]
	public UILabel Attack { get; set;}
	[NGUITag]
	public UILabel NumbWeight { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewSellWeapon { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewSellArmor { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewSellShield { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewBuyWeapon { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewBuyArmor { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewBuyShield { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewBuyOther { get; set;}
	[NGUITag]
	public EndlessScroller ScrollViewBuyBack { get; set;}
	[NGUITag]
	public UIWidget TabContainer2 { get; set;}
	[NGUITag]
	public UIWidget TabContainer3 { get; set;}
	[NGUITag]
	public UIPanel Panel { get; set;}
	[NGUITag]
	public SpriteRenderer bgShop { get; set;}

	private List<ItemBaseData>[] itemSellList = new List<ItemBaseData>[3];
	private List<ItemBaseData> chestIList = new List<ItemBaseData>();
	public List<ShopItemCfg>[] itemBuyList = new List<ShopItemCfg>[3];
	public const string BUY = "BuyItem";
	public const string SELL = "SellItem";
	public const string BUYBACK = "BuyBackItem";
	public const string TRIAL = "TrialItem";
	private int[] indexItem = new int[3] {-1,-1,-1};
	private UIButton[] trialItem = new UIButton[3];
	private string imageNormal = "tabslot";
	private string imageSelect = "tabitemselect";
	private UserData myUser;
	private int curMapId;
	GameObject player;
	SkeletonAnimation skeletonAnimation;
	float slip = 100;
	private bool touch = true;

	protected override void OnStart ()
	{
		base.OnStart ();
		curMapId = config.UserData.curMapId;
		if (Screen.dpi != 0)
			slip = Screen.dpi/2;
		loadTab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
		CreateCharacter ();
	}

	void CreateCharacter()
	{
		mgr.GetAsset<GameObject>("Prefabs/Battle/SimpleObj", CreatePlayerLoad);
	}

	private void CreatePlayerLoad(GameObject go)
	{
		player = GameObject.Instantiate(go) as GameObject;
		player.transform.parent = gameObject.transform;
		CharacterData characterData = config.CharacterCfg.character[config.UserData.CharacterID.ToString()];
		player.name = characterData.Name;
		player.transform.localScale = new Vector3 (460,460,460);
		player.transform.localPosition = new Vector2(-37166, 45391);

		mgr.GetAsset<SkeletonDataAsset>("Animation/Character/Character.ske", delegate (SkeletonDataAsset skele){
			skeletonAnimation = player.GetComponent<SkeletonAnimation>();
			skeletonAnimation.skeletonDataAsset = skele;
			skeletonAnimation.skeletonDataAsset.Reset();
			skeletonAnimation.Reset();
			skeletonAnimation.skeleton.SetSkin(characterData.Name);
			skeletonAnimation.skeleton.SetSlotsToSetupPose();
			skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
			ChangeItem (config.UserData);
			LoadInfo ();
			LoadItemSell ();
			
			LoadItemBuy (true);
			InitData (config.UserData.Inventory.BuyBackListItem, ScrollViewBuyBack, itemService, config, WordManager.SHOP, BUYBACK, config.text.BuyBackNull);
			HideList ();
//			gameObject.SetActive (false);
		});
	}

	void ChangeItem(UserData uData)
	{
		itemService.WearItemView(skeletonAnimation, uData.EquippedItemData, config.UserData.GetCharacterName(), uData.IsActiveSet());
	
	}

	public void DragButton(GameObject go, Vector2 delta)
	{
		go.transform.parent.transform.parent.GetComponent<UIScrollView> ().Scroll (-delta.y/slip);
	}

	public void DragButtonInfo(GameObject go, Vector2 delta)
	{
		go.transform.parent.transform.parent.transform.parent.GetComponent<UIScrollView> ().Scroll (-delta.y/slip);
	}

	void RefreshInfo()
	{
		Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		RefreshStatus (config.UserData);
		myUser = config.UserData.CloneBugUser ();
		RefreshTrialItem ();
		loadTab.RefreshTab();
	}

	void LoadInfo()
	{
		bgShop.gameObject.layer = 0;
		player.gameObject.layer = 0;
		if(Slot != null)
			Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		RefreshStatus (config.UserData);
		myUser = config.UserData.CloneBugUser ();
		RefreshTrialItem ();
	}

	public void RefreshScreen()
	{
		bgShop.gameObject.layer = 0;
		player.gameObject.layer = 0;
		SkeletonAnimation skeletonAnimation = player.GetComponent<SkeletonAnimation>();
		string skin;
		if(config.UserData.CharacterID == 0) skin = "male";
		else skin = "female";
		skeletonAnimation.skeletonDataAsset.Reset();
		skeletonAnimation.Reset();
		skeletonAnimation.skeleton.SetSkin(skin);
		skeletonAnimation.skeleton.SetSlotsToSetupPose();
		skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
		RefreshInfo ();
		loadTab.RefreshTab ();
		LoadListItemSell ();
		RefreshTrialItem ();
		ScrollViewSellWeapon.transform.localPosition = new Vector3 (-15,0,0);
		ScrollViewSellArmor.transform.localPosition = new Vector3 (-15,0,0);
		ScrollViewSellShield.transform.localPosition = new Vector3 (-15,0,0);
		RefreshList (ScrollViewSellWeapon, itemSellList[0], SELL,config.text.NullItem);
		RefreshList (ScrollViewSellArmor, itemSellList[1], SELL,config.text.NullItem);
		RefreshList (ScrollViewSellShield, itemSellList[2], SELL,config.text.NullItem);
		RefreshListBuy (ScrollViewBuyWeapon);
		RefreshListBuy (ScrollViewBuyArmor);
		RefreshListBuy (ScrollViewBuyShield);

		RefreshList (ScrollViewBuyBack, config.UserData.Inventory.BuyBackListItem, BUYBACK, config.text.BuyBackNull);
		LoadItemBuy (false);
		Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
	}

	void RefreshList(UIScrollView mParent, List<ChestData> list, int x, int y)
	{
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go, contentPool);
		}
		mParent.UpdateScrollbars ();
		RefreshPositionScrollView (mParent);
	}

	void RefreshPositionScrollView(UIScrollView scrollView)
	{
		int y = 102;
		if (scrollView == ScrollViewBuyBack)
		{
			y = 220;
		}
		scrollView.transform.localPosition = new Vector3 (scrollView.transform.localPosition.x,y,0);
		UIPanel mPanel = scrollView.gameObject.GetComponent<UIPanel> ();
		mPanel.clipOffset = new Vector2 (0,0);
	}

	public void RefreshTrialItem()
	{
		UserData myUser = config.UserData.CloneBugUser ();
		for(int i = 0; i < 3; i++)
		{
			indexItem[i] = -1;
			if(trialItem[i] != null)
			{
				FillColorToButton(trialItem[i],imageNormal);
				trialItem[i] = null;
			}
		}
		ChangeItem (config.UserData);
		RefreshStatus (config.UserData);
	}

	void ShowStat(UserData uData)
	{
		HeroStat hStat = itemService.GetHeroStat(uData);
		HeroStat cStat = itemService.GetHeroStat(uData, false);
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

	void RefreshStatus(UserData myData)
	{
		ShowStat (myData);
		int currentWeight = 0;
		for(int i = 0; i < myData.EquippedItemData.Count; i++)
		{
			currentWeight += itemService.GetItemWeight(config.ItemCfg.GetItemByItemId(myData.EquippedItemData[i].Id));
		}
		NumbWeight.text = currentWeight+"/"+config.UserData.GetWeight();
	}

	public void TrialItem(GameObject go)
	{
		RefreshStatus (myUser);
		ItemContentManager content = go.GetComponent<ItemContentManager> ();
		if(content.index == indexItem[content.type])
		{
			for(int i = 0; i < config.UserData.EquippedItemData.Count;i++)
			{
				if(content.item.GetSlot() == config.UserData.EquippedItemData[i].GetSlot())
				{
					itemService.TrialItem(config.UserData.EquippedItemData[i], myUser);
				}
			}
			indexItem[content.type] = -1;
			trialItem[content.type] = null;
			itemService.TakeOffItemView(skeletonAnimation, go.GetComponent<ItemContentManager> ().item);
			myUser = config.UserData.CloneBugUser ();
			WearItem (go, true, content);
			ChangeItem(myUser);
		} else{
			indexItem[content.type] = content.index;
			trialItem[content.type] = go.GetComponent<ItemContentManager>().btContent;
			itemService.TrialItem(go.GetComponent<ItemContentManager> ().item, myUser);
			WearItem (go, false, content);
			ChangeItem(myUser);
		}
		RefreshStatus (myUser);
	}

	void WearItem(GameObject go, bool check,ItemContentManager content)
	{
		content = go.GetComponent<ItemContentManager> ();
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
		}
	}

	void FillColorToButton(UIButton button, string image)
	{
		button.normalSprite = image;
		button.hoverSprite = image;
		button.pressedSprite = image;
	}

	public void BuyBackItem(GameObject go)
	{
		routineRunner.StartCoroutine (BuyBackItemProgress(go));
	}
	
	public IEnumerator BuyBackItemProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			switch(shopService.BuyBackItem (content.item, config.UserData))
			{
			case ErrorCode.OK:
				soundMng.PlaySound(SoundName.BUY_ITEM_SUCCESS);
				itemSellList [content.type].Add (content.item);
				loadTab.RefreshTab ();
				switch(content.type)
				{
				case 0:
					RefreshList (ScrollViewSellWeapon, itemSellList[content.type], SELL,config.text.NullItem);
					break;
				case 1:
					RefreshList (ScrollViewSellArmor, itemSellList[content.type], SELL,config.text.NullItem);
					break;
				case 2:
					RefreshList (ScrollViewSellShield, itemSellList[content.type], SELL,config.text.NullItem);
					break;
				}
				RefreshList (ScrollViewBuyBack, config.UserData.Inventory.BuyBackListItem,BUYBACK,config.text.BuyBackNull);
				loadTab.ShowNotice(config.text.BuySuccessfully);
				break;
			case ErrorCode.NOT_ENOUGH_GOLD:
				loadTab.ShowNoticePurchaseGold();
				break;
			case ErrorCode.NOT_ENOUGH_SLOT:
				loadTab.ShowNoticePurchaseSlot();
				break;
			default:
				loadTab.ShowNotice(shopService.BuyBackItem (content.item, config.UserData).ToString());
				break;
			}
			Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	public void BuyItem(GameObject go)
	{
		routineRunner.StartCoroutine (BuyItemProgress(go));
	}
	
	public IEnumerator BuyItemProgress(GameObject go)
	{
		if(touch)
		{
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			switch(shopService.BuyItem (content.shop))
			{
			case ErrorCode.OK:
				if(content.type < 3)
					itemSellList [content.type].Add (content.item);
				soundMng.PlaySound(SoundName.BUY_ITEM_SUCCESS);
				RefreshInfo();
				LoadListItemSell ();
				RefreshTrialItem ();
				RefreshList (ScrollViewSellWeapon, itemSellList[0], SELL,config.text.NullItem);
				RefreshList (ScrollViewSellArmor, itemSellList[1], SELL,config.text.NullItem);
				RefreshList (ScrollViewSellShield, itemSellList[2], SELL,config.text.NullItem);

				loadTab.ShowNotice(config.text.BuySuccessfully);
					switch (content.type)
					{
						case 0:
						case 1:
						case 2:
							ga.TrackEquipmentBuy(content.shop.Id);
							break;
						case 3:
							ga.TrackChestBuy(content.shop.Id);
							break;
					}
				config.UserData.SetItemUpdate(true);
				break;
			case ErrorCode.NOT_ENOUGH_GOLD:
				loadTab.ShowNoticePurchaseGold();
				break;
			case ErrorCode.NOT_ENOUGH_GEM:
				loadTab.ShowNoticePurchaseGem();
				break;
			case ErrorCode.NOT_ENOUGH_SLOT:
				loadTab.ShowNoticePurchaseSlot();
				break;
			default:
				loadTab.ShowNotice(shopService.BuyItem (content.shop).ToString());
				break;
			}
			Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	public void SellItem(GameObject go)
	{
		routineRunner.StartCoroutine (SellItemProgress(go));
	}
	
	public IEnumerator SellItemProgress(GameObject go)
	{
		if(touch)
		{
			touch = false;
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
			switch(shopService.SellItem (content.item, config.UserData))
			{
			case ErrorCode.OK:
				soundMng.PlaySound(SoundName.BUY_ITEM_SUCCESS);
				itemSellList [content.type].RemoveAt (content.index);
				loadTab.RefreshTab();
				RefreshList (go.transform.parent.GetComponent<EndlessScroller>(), itemSellList[content.type], SELL,config.text.NullItem);
				RefreshList (ScrollViewBuyBack, config.UserData.Inventory.BuyBackListItem, BUYBACK,config.text.BuyBackNull);
				Slot.text = "Slot: "+config.UserData.Inventory.ListItemData.Count + "/" + config.UserData.Inventory.MaxSlot;
				loadTab.ShowNotice(config.text.SellItemSuccessfully);
				break;
			default:
				loadTab.ShowNotice(shopService.SellItem (content.item, config.UserData).ToString());
				break;
			}
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	public void ShowInfo(GameObject go)
	{
		routineRunner.StartCoroutine (ShowInfoProgress(go));
	}
	
	public IEnumerator ShowInfoProgress(GameObject go)
	{
		if(touch)
		{
			soundMng.PlaySound (SoundName.SHOW_INFO);
			ItemContentManager content = go.GetComponent<ItemContentManager> ();
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

	void RefreshListBuy(EndlessScroller mParent)
	{
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go, contentPool);
		}
		mParent.transform.localPosition = new Vector3 (-15,0,0);
	}

	void RefreshList(EndlessScroller mParent, List<ItemBaseData> list, string nameEvent, string text)
	{
		for(int i = 0; i < mParent.transform.childCount; i++)
		{
			GameObject go = mParent.transform.GetChild(i).gameObject;
			Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
			ReturnInstance (go, contentPool);
		}

		SetList(list, mParent, nameEvent, text);
		mParent.Drop ();
	}

	void LoadItemBuy(bool checkLoadChest)
	{
		List<ItemBaseData> weaponBuyList = new List<ItemBaseData>();
		List<ItemBaseData> armorBuyList = new List<ItemBaseData>();
		List<ItemBaseData> shieldBuyList = new List<ItemBaseData>();
		itemBuyList[0] = new List<ShopItemCfg>();
		itemBuyList[1] = new List<ShopItemCfg>();
		itemBuyList[2] = new List<ShopItemCfg>();
		int countItemShop = 0;
		for(int i = 0; i < config.shopCfg.item.Count; i++)
		{
			ShopItemCfg shopItemCfg = config.shopCfg.item.ElementAt(i).Value;
			if(shopItemCfg.Require <= curMapId)
			{
				ItemBaseData item = new ItemBaseData(config.ItemCfg.GetItemByItemId(shopItemCfg.Id));
				countItemShop++;
				switch(item.GetSlot())
				{
					case ItemCfg.WEAPON_SLOT:
						itemBuyList[0].Add(config.shopCfg.item.ElementAt(i).Value);
						weaponBuyList.Add(item);
						break;
					case ItemCfg.ARMOR_SLOT:
						itemBuyList[1].Add(config.shopCfg.item.ElementAt(i).Value);
						armorBuyList.Add(item);
						break;
					case ItemCfg.SHIELD_SLOT:
						itemBuyList[2].Add(config.shopCfg.item.ElementAt(i).Value);
						shieldBuyList.Add(item);
						break;
					default:
						break;
				}
			}
		}
		itemBuyList [0] = SortList (itemBuyList [0]);
		itemBuyList [1] = SortList (itemBuyList [1]);
		itemBuyList [2] = SortList (itemBuyList [2]);
		weaponBuyList = SortList (weaponBuyList);
		armorBuyList = SortList (armorBuyList);
		shieldBuyList = SortList (shieldBuyList);
		if(config.UserData.numberItemShop != 0 && config.UserData.numberItemShop < countItemShop)
		{
			config.UserData.SetPopupUpdate(1);
			config.UserData.SetIconUpdate(1);
		}
		config.UserData.numberItemShop = countItemShop;
//		loadTab.CheckShopUpdate ();
		InitData (weaponBuyList, ScrollViewBuyWeapon, itemService, config, WordManager.SHOP, BUY, config.text.NullItem);
		InitData (armorBuyList, ScrollViewBuyArmor, itemService, config, WordManager.SHOP, BUY, config.text.NullItem);
		InitData (shieldBuyList, ScrollViewBuyShield, itemService, config, WordManager.SHOP, BUY, config.text.NullItem);
		if(checkLoadChest)
			SetOtherListBuy ();
	}

	void LoadListItemSell()
	{
		itemSellList[0] = new List<ItemBaseData>();
		itemSellList[1] = new List<ItemBaseData>();
		itemSellList[2] = new List<ItemBaseData>();
		for(int i = 0; i < config.UserData.Inventory.ListItemData.Count; i++)
		{
			switch(config.UserData.Inventory.ListItemData[i].GetSlot())
			{
			case ItemCfg.WEAPON_SLOT:
				itemSellList[0].Add(config.UserData.Inventory.ListItemData[i]);
				break;
			case ItemCfg.ARMOR_SLOT:
				itemSellList[1].Add(config.UserData.Inventory.ListItemData[i]);
				break;
			case ItemCfg.SHIELD_SLOT:
				itemSellList[2].Add(config.UserData.Inventory.ListItemData[i]);
				break;
			default:
				break;
			}
		}
	}

	List<ItemBaseData> SortList(List<ItemBaseData> list) 
	{
		bool check = false;
		while(!check)
		{
			check = true;
			for(int i = 0; i < list.Count-1; i++)
			{
				int price1 = itemService.GetItemPriceSellInShop(config.ItemCfg.GetItemByItemId(list[i].Id));
				int price2 = itemService.GetItemPriceSellInShop(config.ItemCfg.GetItemByItemId(list[i+1].Id));
				if(price1 > price2)
				{
					check = false;
					ItemBaseData item = list[i].Clone();
					list[i] = list[i+1].Clone();
					list[i+1] = item.Clone();
				}
			}
		}
		return list;
	}

	List<ShopItemCfg> SortList(List<ShopItemCfg> list) 
	{
		bool check = false;
		while(!check)
		{
			check = true;
			for(int i = 0; i < list.Count-1; i++)
			{
				int price1 = itemService.GetItemPriceSellInShop(config.ItemCfg.GetItemByItemId(list[i].Id));
				int price2 = itemService.GetItemPriceSellInShop(config.ItemCfg.GetItemByItemId(list[i+1].Id));
				if(price1 > price2)
				{
					check = false;
					ShopItemCfg item = list[i].Clone();
					list[i] = list[i+1].Clone();
					list[i+1] = item.Clone();
				}
			}
		}
		return list;
	}

	void LoadItemSell()
	{
		LoadListItemSell ();
		InitData (itemSellList[0], ScrollViewSellWeapon, itemService, config, WordManager.SHOP, SELL, config.text.NullItem);
		InitData (itemSellList[1], ScrollViewSellArmor, itemService, config, WordManager.SHOP, SELL, config.text.NullItem);
		InitData (itemSellList[2], ScrollViewSellShield, itemService, config, WordManager.SHOP, SELL, config.text.NullItem);
	}

	void HideList()
	{
		ScrollViewSellArmor.gameObject.SetActive (false);
		ScrollViewSellShield.gameObject.SetActive (false);
		ScrollViewBuyArmor.gameObject.SetActive (false);
		ScrollViewBuyShield.gameObject.SetActive (false);
		ScrollViewBuyWeapon.gameObject.SetActive (false);
		TabContainer2.gameObject.SetActive (false);
		TabContainer3.gameObject.SetActive (false);
	}

	void InitData(List<ItemBaseData> itemList, EndlessScroller parent, ItemService itemService, ConfigManager configManager, int state, string eventName, string text)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetData (itemService, configManager, mgr, contentPool, state);
		wordManager.SetDataShop (eventName, text);
		SetList (itemList, parent, eventName, text);
	}

	void SetList(List<ItemBaseData> itemList, EndlessScroller parent, string eventName, string text)
	{
		WordManager wordManager = parent.GetComponent<WordManager>();
		wordManager.SetData (itemService, config, mgr, contentPool, WordManager.SHOP);
		wordManager.SetDataShop (eventName, text);
		wordManager.Init(itemList);
		if(itemList.Count == 0)
		{
			GameObject item = Utils.ButtonNullSlot(parent.gameObject,contentPool,"tabslot",0,-30,config.text.NullItem);
			parent.Drop();
		}
	}

	public ShopItemCfg GetShopInfo(int ID)
	{
		ShopItemCfg shopIf = null;
		for(int i = 0; i < config.shopCfg.chest.Count; i++)
		{
			ShopItemCfg shopItem = config.shopCfg.chest.ElementAt(i).Value;
			if(shopItem.Id == ID)
				return shopItem;
		}
		return shopIf;
	}

	void SetOtherListBuy()
	{
		List<ShopItemCfg> listChestShop = new List<ShopItemCfg> ();
		for(int i = 0; i < config.shopCfg.chest.Count; i++)
		{
			ShopItemCfg shopItem = config.shopCfg.chest.ElementAt(i).Value;
			listChestShop.Add(shopItem);
		}
		bool check = false;
		while(!check)
		{
			check = true;
			for(int i = 0; i < listChestShop.Count-1; i++)
			{
				if(listChestShop[i].Gem > listChestShop[i+1].Gem || 
				   (listChestShop[i].Gold > listChestShop[i+1].Gold && listChestShop[i].Gem == listChestShop[i+1].Gem))
				{
					ShopItemCfg shop = CloneShopItem(listChestShop[i]);
					listChestShop[i] = CloneShopItem(listChestShop[i+1]);
					listChestShop[i+1] = CloneShopItem(shop);
					check = false;
					break;
				}
			}

		}
		for(int i = 0; i < listChestShop.Count; i++)
		{
			ItemBaseData item = new ItemBaseData();
			item.Id = listChestShop[i].Id + 6000;
			chestIList.Add(item);
		}
		InitData (chestIList, ScrollViewBuyOther, itemService, config, WordManager.SHOP, BUY, config.text.NullItem);
	}

	ShopItemCfg CloneShopItem(ShopItemCfg shop)
	{
		ShopItemCfg clone = new ShopItemCfg ();
		clone.Id = shop.Id;
		clone.Gem = shop.Gem;
		clone.Gold = shop.Gold;
		return clone;
	}

	void InfoChest(GameObject go)
	{
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		GameObject popup = CreatePoolGameObject (popupPool);
		popup.transform.parent = Panel.transform;
		popup.transform.localScale = Vector3.one;
		PopUpContent pContent = popup.GetComponent<PopUpContent>();
		pContent.openChest.gameObject.SetActive (false);
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
		int countItemSet = 0;
		SetItemContent setContent = null;
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
		AddEventToButton(pContent.btHideIcon,"HideNoticeChest", pContent.imgContentChest.gameObject);
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

	void HideNotice(GameObject go)
	{
		go.GetComponent<PopUpContent> ().btHide.onClick.Clear ();
		ReturnInstance (go,popupPool);
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
}
