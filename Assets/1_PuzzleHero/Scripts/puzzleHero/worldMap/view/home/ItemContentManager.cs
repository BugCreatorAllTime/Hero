using UnityEngine;
using System.Collections;

public class ItemContentManager : MonoBehaviour {

	public UISprite iconCoin;
	public UISprite imgContent;
	public UISprite theme;
	public UISprite icon;
	public UILabel nameItem;
	public UISprite iconDame;
	public UISprite iconArmor;
	public UISprite iconHp;
	public UISprite iconWeight;
	public UILabel numberDame;
	public UILabel numberArmor;
	public UILabel numberHp;
	public UILabel numberWeight;
	public UIButton buttonSell;
	public UILabel price;
	public UISprite imgInfo;
	public UIButton btInfo;
	public UIButton btContent;
	public ItemBaseData item;
	public UISprite contentIcon;

	public UISprite iconWeaponSet;
	public UISprite iconArmorSet;
	public UISprite iconShieldSet;

	public UIButton contentDes;
	public UILabel nameDes;
	public UILabel description;

	public ShopItemCfg shop;
	public Stat stat;
	public int index;
	public int type;
	public int indexOfInventory;
	public string typeEvent;

	public Transform mParentTrans;
	public ItemBaseData itemBaseData;
	
	private EndlessScroller table;
	private WordManager wordManager;

	// Clear Outside Word Item
	void UpdateTableItem() {
		if (mParentTrans == null) return;
		if (table == null)
		{
			table = mParentTrans.GetComponent<EndlessScroller>();
			wordManager = mParentTrans.GetComponent<WordManager>();
		}
		Vector3 pos = mParentTrans.localPosition + transform.localPosition;
		if (pos.y > table.cellHeight * 2f || pos.y < table.cellHeight * -6f)
		{
			wordManager.ReturnInstance(gameObject);
			//			Destroy(gameObject);
		}
	}
	
	void Update() {
		UpdateTableItem();
	}

	public void OnClick()
	{
		WordManager manager = transform.parent.GetComponent<WordManager>();
		if(manager.smith != null)
		{
			manager.smith.SetItem(gameObject);
		}
		if(manager.home != null)
		{
			if(itemBaseData.Id == -9999)
			{
				manager.home.BuySlot(gameObject);
			} else {
				if(typeEvent == LoadInfoCharacter.ITEM)
					manager.home.ClickEquip(gameObject);
			}
		}
		if(manager.shop != null)
		{
			switch(typeEvent)
			{
				case LoadInfoShop.BUY:
					manager.shop.TrialItem(gameObject);
					break;
				case LoadInfoShop.SELL:
					break;
				case LoadInfoShop.BUYBACK:
					break;
				default:
					break;
			}
		}
	}
}
