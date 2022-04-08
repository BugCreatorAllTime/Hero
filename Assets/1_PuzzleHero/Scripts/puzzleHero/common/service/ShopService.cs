
using System;

public class ShopService
{
	[Inject]
	public ConfigManager mainCfg {get; set;}

	[Inject]
	public ChestService chestService {get; set;}

	[Inject]
	public ItemService itemService {get; set;}

	[Inject]
	public IPaymentService paymentService {get; set;}

	public delegate void BuyGemResponse(int id, PaymentErrorCode error);
	public BuyGemResponse BuyGemHandler;

	public ShopService ()
	{
	}
	
	public ErrorCode SellItem(ItemBaseData item, UserData uData)
	{
		int amount = itemService.GetItemPriceSellFromUser(item);
		if(uData.Inventory.RemoveItem(item))
		{
			uData.Inventory.AddBBItem(item, mainCfg.general.MaxBuyBackSlot);
			uData.AddGold(amount);
			return ErrorCode.OK;
		}
		return ErrorCode.UNKNOWN_ERROR;
	}

	public void CheckPayment()
	{
		paymentService.CheckPayment ();
	}

	public ErrorCode BuyItem(ShopItemCfg shopItem)
	{
		UserData uData = mainCfg.UserData;
		if(shopItem.Type == ShopCfg.SHOP_ITEM_GEM)
		{
			paymentService.BuyItem(shopItem.StoreId,1);
			return ErrorCode.WAITING_FOR_PROCESS;
		}else 
		{
			ErrorCode error = CheckRequireResource(shopItem,uData);
			if(error == ErrorCode.OK)
			{
				switch(shopItem.Type)
				{
				case ShopCfg.SHOP_ITEM_CHEST:
					ProcessBuyChest(shopItem,uData);
					break;
				case ShopCfg.SHOP_ITEM_ENERGY:
					ProcessBuyEnergy(shopItem,uData);
					break;
				case ShopCfg.SHOP_ITEM_GOLD:
					ProcessBuyGold(shopItem,uData);
					break;
				case ShopCfg.SHOP_ITEM_ITEM:
					ProcessBuyItem(shopItem,uData);
					break;
				case ShopCfg.SHOP_ITEM_BAG:
					ProcessBuyBag(shopItem,uData);
					break;
				case ShopCfg.SHOP_ITEM_SET:
					ProcessBuySet(shopItem,uData);
					break;
				}
			}
			return error;
		}
	}

	void ProcessBuyBag (ShopItemCfg shopItem, UserData uData)
	{
		ConsumeUserResource(shopItem,uData);
		uData.Inventory.IncreaseMaxSlot(shopItem.Quantity);
	}

	public ErrorCode BuyBackItem(ItemBaseData item, UserData uData)
	{
		int amount = itemService.GetItemPriceBuyBack(item);
		if(amount < uData.gold)
		{
			if(!uData.Inventory.IsFull())
			{
				uData.Inventory.RemoveBBItem(item);
				uData.Inventory.AddItem(item);
				uData.SubGold(amount);
				return ErrorCode.OK;
			}else {
				return ErrorCode.NOT_ENOUGH_SLOT;
			}

		}else {
			return ErrorCode.NOT_ENOUGH_GOLD;
		}

	}

	public bool ProcessRevive(int currentReviveNumber)
	{
		int revivePrice = CalculateRevivePrice(currentReviveNumber);
		return mainCfg.UserData.SubGem(revivePrice);
	}

	public bool ProcessBuyMove(int currentMoveNumber)
	{
		int movePrice = CalculateMovePrice(currentMoveNumber);
		return mainCfg.UserData.SubGem(movePrice);
	}

	public int CalculateMovePrice(int currentMoveNumber)
	{
		string defaultBuyMoveId = "1";
		return currentMoveNumber * mainCfg.shopCfg.move[defaultBuyMoveId].Gem;
	}

	public int CalculateRevivePrice(int currentReviveNumber)
	{
		string defaultReviveItemId = "1";
		return currentReviveNumber + mainCfg.shopCfg.revive[defaultReviveItemId].Gem;
	}

	private void ProcessBuyChest(ShopItemCfg shopItem, UserData uData)
	{
		ConsumeUserResource(shopItem,uData);
		chestService.AddChest(shopItem.Id, ChestSource.FROM_GEM, uData);
	}

	private void ProcessBuyEnergy(ShopItemCfg shopItem, UserData uData)
	{
		ConsumeUserResource(shopItem,uData);
		uData.AddEnergy(shopItem.Quantity);
	}

	private void ProcessBuyGold(ShopItemCfg shopItem, UserData uData)
	{
		ConsumeUserResource(shopItem,uData);
		uData.AddGold(shopItem.Quantity);
	}

	void ProcessBuySet (ShopItemCfg shopItem, UserData uData)
	{
		SetBannerInfoData data = mainCfg.setBannerCfg.getSetInfo (shopItem.Id);
		if(data.Random == 0)
		{
			for(int i = 0; i < data.ListItem.Count; i++)
			{
				ItemCfgImpl cfg = mainCfg.ItemCfg.GetItemByItemId(data.ListItem[i]);
				ItemBaseData item = itemService.GenItem(cfg);
				uData.Inventory.AddItem(item);
			}
		} else {
			System.Random r = new System.Random ();
			int rs = r.Next (0, data.ListItem.Count-1);
			ItemCfgImpl cfg = mainCfg.ItemCfg.GetItemByItemId(data.ListItem[rs]);
			ItemBaseData item = itemService.GenItem(cfg);
			uData.Inventory.AddItem(item);
		}
		uData.SubGem(shopItem.Gem);
	}

	private void ProcessBuyItem(ShopItemCfg shopItem, UserData uData)
	{
		ItemCfgImpl cfg = mainCfg.ItemCfg.GetItemByItemId(shopItem.Id);
		ItemBaseData data = itemService.GenItem(cfg);
		int amount = itemService.GetItemPriceSellInShop(cfg);
		uData.Inventory.AddItem(data);
		uData.SubGold(amount);
	}

	private ErrorCode CheckRequireResource(ShopItemCfg shopItem, UserData uData)
	{
		if(shopItem.Gem > uData.gem)
		{
			return ErrorCode.NOT_ENOUGH_GEM;
		}

		if(shopItem.Gold > uData.gold)
		{
			return ErrorCode.NOT_ENOUGH_GOLD;
		}

		if(shopItem.Type == ShopCfg.SHOP_ITEM_ITEM )
		{
			ItemCfgImpl cfg = mainCfg.ItemCfg.GetItemByItemId(shopItem.Id);
			int amount = itemService.GetItemPriceSellInShop(cfg);
			if(amount > uData.gold)
			{
				return ErrorCode.NOT_ENOUGH_GOLD;
			}
			if(uData.Inventory.IsFull())
			{
				return ErrorCode.NOT_ENOUGH_SLOT;
			}
		}

		return ErrorCode.OK;
	}

	private void ConsumeUserResource(ShopItemCfg shopItem, UserData uData)
	{
		uData.SubGem(shopItem.Gem);
		if(shopItem.Type == ShopCfg.SHOP_ITEM_ITEM )
		{
			ItemCfgImpl cfg = mainCfg.ItemCfg.GetItemByItemId(shopItem.Id);
			int amount = itemService.GetItemPriceSellInShop(cfg);
			uData.SubGold(amount);
		}else
		{
			uData.SubGold(shopItem.Gold);
		}
	}

	public void OnBuyGemResponse(string itemId, int quantity, string transactionId, PaymentErrorCode error)
	{
		if(error == PaymentErrorCode.OK)
		{
			UserData uData = mainCfg.UserData;
			string[] arr = itemId.Split('.');
			string itemshopId = arr[arr.Length - 1];
			ShopItemCfg config = mainCfg.shopCfg.gem[itemshopId];
			uData.AddGem(config.Quantity);
		}
		if (BuyGemHandler != null)
		{
			string[] arr = itemId.Split('.');
			string itemshopId = arr[arr.Length - 1];
			if(error == PaymentErrorCode.OK)
				BuyGemHandler(int.Parse(itemshopId), error);
			else
				BuyGemHandler(-1,error);
		}

		//Logger.Trace("shopservice::OnBuyGemResponse", itemId, transactionId, error);
	}
	
}


