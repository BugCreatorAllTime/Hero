using strange.extensions.pool.api;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WordManager : MonoBehaviour
{
	public const int SMITH = 1;
	public const int HOME = 2;
	public const int SHOP = 3;
	public int state;
	public ItemService itemService;
	public ConfigManager configManager;
	public EndlessScroller table;
	public AssetMgr assetMgr;
	public IPool<GameObject> itemPool;
	public LoadInfoBlacksmith smith;
	public LoadInfoCharacter home;
	public LoadInfoShop shop;
	List<int> indexList;
	public int indexItem;
	List<ItemBaseData> words;
	GameObject wordPrefab;
	
	float posScrollY = 0f;
	public float cellWidth = 560;
	public float cellHeight = 155;
	float cellPadiding = 8f;
	float totalHeight = 0f;
	private string imageNormal = "tabslot";
	private string imageSelect = "tabitemselect";
	string eventName;
	string text;
	
	public void Init(List<ItemBaseData> data)
	{
		this.words = data;
		table = GetComponent<EndlessScroller>();
		for (int i = 0; i < 5; i++) {
			if (i >= words.Count) break;
			ItemBaseData word = words[i];
			SetWordItem(word, i);
		}
		
		totalHeight = cellHeight * words.Count + cellPadiding * (words.Count + 1)+45;
		BoxCollider collider = table.GetComponent<Collider>() as BoxCollider;
		collider.size = new Vector3(cellWidth, totalHeight, 1f);
		collider.center = new Vector3(0f, totalHeight * -0.5f + cellHeight * 0.5f, 0f);
		table.totalHeight = totalHeight;
		table.cellHeight = cellHeight;
	}

	public void SetData(ItemService itemService, ConfigManager configManager, AssetMgr assetMgr, IPool<GameObject> itemPool, int state)
	{
		this.state = state;
		this.itemService = itemService;
		this.configManager = configManager;
		this.assetMgr = assetMgr;
		this.itemPool = itemPool;
	}

	public void SetDataSmith(List<int> indexList)
	{
		this.indexList = indexList;
	}

	public void SetDataHome(int indexItem)
	{
		this.indexItem = indexItem;
	}

	public void SetDataShop(string eventName, string text)
	{
		this.eventName = eventName;
		this.text = text;
	}
	
	public void ReturnInstance(GameObject go)
	{
		Utils.RemoveEvent(go.GetComponent<ItemContentManager> ());
		go.SetActive(false);
		itemPool.ReturnInstance(go);
		
	}
	
	void Update () {
		if (words != null) UpdateTable();
	}
	
	// Display Items in Screen
	void UpdateTable(){
		int idxPos =  Mathf.FloorToInt( table.transform.localPosition.y / (cellHeight+cellPadiding) );
		ItemContentManager[] items = table.GetComponentsInChildren<ItemContentManager>();
		for (int i=idxPos-1;i<idxPos+4;i++) {
			if (i<0) continue;
			if (i>words.Count-1) continue;
			ItemBaseData word = words[i];
			bool ok = false;
			foreach (ItemContentManager item in items) {
				if (item.itemBaseData == word) ok = true;
			}
			if (ok) continue;
			SetWordItem(word, i);
		}
	}
	
	void UpdateTableItem(ItemContentManager item) {
		Transform mParentTrans = transform;
		Transform itemTransform = item.transform;
		Vector3 pos = mParentTrans.localPosition + itemTransform.localPosition;
		if (pos.y > table.cellHeight * 2f || pos.y < table.cellHeight * -6f) {
			Utils.RemoveEvent(item.gameObject.GetComponent<ItemContentManager> ());
			ReturnInstance(item.gameObject);

		}
	}
	
	// Display Word Item
	void SetWordItem(ItemBaseData itemBaseData, int i) {
		GameObject instance = itemPool.GetInstance();
		instance.SetActive (true);
		AddChild(table.gameObject, instance);
		ItemContentManager wditem = instance.GetComponent<ItemContentManager>();
		wditem.imgContent.gameObject.name = "Content";
		wditem.buttonSell.gameObject.name = "buttonSell";
		wditem.iconWeaponSet.gameObject.name = "IconWeaponSet";
		wditem.iconArmorSet.gameObject.name = "IconArmorSet";
		wditem.iconShieldSet.gameObject.name = "IconShieldSet";
		wditem.btInfo.gameObject.name = "ButtonInfo";
		wditem.buttonSell.gameObject.GetComponent<BoxCollider>().enabled = true;
		if(itemBaseData.Id > 7000)
		{
			wditem.iconWeaponSet.gameObject.SetActive (false);
			wditem.iconArmorSet.gameObject.SetActive (false);
			wditem.iconShieldSet.gameObject.SetActive (false);
			Vector3 pos = new Vector3(0f, i*-(cellHeight+cellPadiding)-30, 0f);
			instance.transform.localPosition = pos;
			ChestCfgImplement chestInfo = configManager.chestCfg.GetChestCfg(itemBaseData.Id - 6000);
			wditem.icon.spriteName = chestInfo.Icon;
			wditem.itemBaseData = itemBaseData;
			wditem.nameItem.text = chestInfo.Name;
			wditem.nameItem.color = Color.white;
			wditem.nameItem.width = 200;
			wditem.nameItem.height = 30;
			wditem.nameItem.transform.localPosition = new Vector3(40, -10, 0);
			wditem.iconCoin.gameObject.SetActive(false);
			wditem.iconArmor.gameObject.SetActive(false);
			wditem.iconDame.gameObject.SetActive(false);
			wditem.iconHp.gameObject.SetActive(false);
			wditem.iconWeight.gameObject.SetActive(false);
			wditem.theme.spriteName = configManager.text.NormalItem;
			wditem.index = itemBaseData.Id - 6000;
			wditem.indexOfInventory = i;
			wditem.buttonSell.gameObject.SetActive(true);
			wditem.btInfo.gameObject.SetActive(true);
			if(state == HOME)
			{
				wditem.typeEvent = LoadInfoCharacter.CHEST;
				wditem.price.text = "OPEN";
				home.AddEventToButton(wditem.btInfo,"InfoChest", instance);
				home.AddEventToButton(wditem.buttonSell,"OpenChest", instance);
			}
			if(state == SHOP)
			{
				ShopItemCfg shopItem = shop.GetShopInfo(wditem.index);
				wditem.iconCoin.gameObject.SetActive(true);
				if(shopItem.Gold > 0)
				{
					wditem.iconCoin.spriteName = "coin";
					wditem.price.text = shopItem.Gold.ToString();
				}
				if(shopItem.Gem > 0)
				{
					wditem.iconCoin.spriteName = "diamond";
					wditem.price.text = shopItem.Gem.ToString();
				}
				wditem.index = shopItem.Id;
				wditem.indexOfInventory = i;
				shop.AddEventToButton(wditem.btInfo, "InfoChest", instance);
				wditem.shop = shopItem;
				wditem.type = 3;
				shop.AddEventToButton(wditem.buttonSell, "BuyItem", instance);
			}
		}
		else if(itemBaseData.Id != -9999)
		{
			Utils.FillDataItem(instance, itemBaseData, itemService, configManager);
			Vector3 pos = new Vector3(0f, i*-(cellHeight+cellPadiding)-30, 0f);
			instance.transform.localPosition = pos;
			wditem.itemBaseData = itemBaseData;
			wditem.mParentTrans = table.transform;
			wditem.index = i;
			switch(state)
			{
			case SMITH:
				wditem.indexOfInventory = indexList[i];
				wditem.buttonSell.gameObject.SetActive(false);
				wditem.btInfo.gameObject.SetActive(true);
				smith.AddEventToButton(wditem.btInfo,"ShowInfo", instance);
				if(configManager.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.BLACKSMITH &&
				   configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END)
				{
					if(wditem.index == 0 && wditem.type == 0)
					{
						wditem.imgContent.gameObject.name = "ItemFirstBlackSmith";
					}
				}
				break;
			case HOME:
				wditem.typeEvent = LoadInfoCharacter.ITEM;
				wditem.buttonSell.gameObject.SetActive(false);
				wditem.iconWeight.gameObject.SetActive(true);
				home.AddEventToButton(wditem.btInfo,"ShowInfo", instance);
				UIButton contentButton = wditem.btContent;
				//color item choose
				if(i != indexItem)
				{
					home.FillColorToButton(contentButton, imageNormal);
				} else{
					home.FillColorToButton(contentButton, imageSelect);
				}
				if(wditem.index == 0 && wditem.type == 0)
				{
					wditem.imgContent.gameObject.name = "FirstWeapon";
				}
				if(wditem.index == 0 && wditem.type == 1)
				{
					wditem.imgContent.gameObject.name = "FirstArmor";
				}
				if(wditem.index == 0 && wditem.type == 2)
				{
					wditem.iconWeaponSet.gameObject.name = "FirstIcon";
					wditem.iconArmorSet.gameObject.name = "SecondIcon";
					wditem.iconShieldSet.gameObject.name = "ThirdIcon";
					wditem.imgContent.gameObject.name = "FirstShield";
					wditem.btInfo.gameObject.name = "FirstInfo";
				}
				break;
			case SHOP:
				shop.AddEventToButton(wditem.btInfo,"ShowInfo", instance);
				wditem.typeEvent = eventName;
				shop.AddEventToButton(wditem.buttonSell, eventName, instance);
				switch(eventName)
				{
				case LoadInfoShop.BUYBACK:
					wditem.price.text = itemService.GetItemPriceBuyBack(wditem.item).ToString();
					break;
				case LoadInfoShop.BUY:
					if(wditem.index == 0 && wditem.type == 0)
					{
						wditem.buttonSell.gameObject.name = "ButtonBuyFirst";
						if(configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.BUY_ITEM)
							wditem.buttonSell.gameObject.GetComponent<BoxCollider>().enabled = false;
						else wditem.buttonSell.gameObject.GetComponent<BoxCollider>().enabled = true;
					}
					wditem.buttonSell.gameObject.GetComponent<UIButton>().disabledColor = Color.white;
					wditem.price.text = itemService.GetItemPriceSellInShop(configManager.ItemCfg.GetItemByItemId(wditem.item.Id)).ToString();
					wditem.shop = shop.itemBuyList[wditem.type][i];
					break;
				case LoadInfoShop.SELL:
					shop.AddEventToButton(wditem.buttonSell, "SellItem", instance);
					wditem.price.text = itemService.GetItemPriceSellFromUser(wditem.item).ToString();
					break;
				}
				break;
			default:
				break;
			}
		} else {
			wditem.iconWeaponSet.gameObject.SetActive (false);
			wditem.iconArmorSet.gameObject.SetActive (false);
			wditem.iconShieldSet.gameObject.SetActive (false);
			Vector3 pos = new Vector3(0f, i*-(cellHeight+cellPadiding)-30, 0f);
			wditem.index = i;
			home.FillColorToButton(wditem.btContent, imageNormal);
			instance.transform.localPosition = pos;
			wditem.itemBaseData = itemBaseData;
			wditem.icon.spriteName = "AddSlot";
			wditem.buttonSell.gameObject.SetActive(false);
			wditem.nameItem.text = "Buy Slot";
			wditem.nameItem.color = Color.white;
			wditem.iconArmor.gameObject.SetActive (false);
			wditem.iconDame.gameObject.SetActive (false);
			wditem.iconHp.gameObject.SetActive (false);
			wditem.iconWeight.gameObject.SetActive (false);
			wditem.btInfo.gameObject.SetActive (false);
			wditem.icon.width = 100;
			wditem.icon.height = 100;
		}

	}
	
	private void AddChild(GameObject parent, GameObject child)
	{
		Transform t = child.transform;
		t.parent = parent.transform;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.one;
		child.layer = parent.gameObject.layer;
	}
}
