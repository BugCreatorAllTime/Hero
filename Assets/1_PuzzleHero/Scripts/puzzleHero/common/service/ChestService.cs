
using System;
using System.Collections.Generic;

public class ChestService
{
	[Inject]
	public ConfigManager configMgr {get;set;}

	public ChestService ()
	{
	}

	public ItemBaseData OpenChest(UserData uData, int index)
	{
		ChestModel chestM = uData.ChestModel;
		ChestData chestData = chestM.OpenChest(index);
		if(chestData != null)
		{
			ChestCfgImplement config = configMgr.chestCfg.GetChestCfg(chestData.chestId);
			int randomItemChen = config.RandomItem.GetChance(chestData.source);
			int numRandom = UnityEngine.Random.Range(0,randomItemChen);
			int itemId = -1;
			int[] ids;
			if(numRandom < randomItemChen)
			{
				ids = config.RandomItem.Ids;
			}else{
				ids = config.FixItemIds;
			}
			itemId = ids[UnityEngine.Random.Range(0, ids.Length)];
			ItemCfgImpl itemcfg = configMgr.ItemCfg.GetItemByItemId(itemId);
			ItemBaseData data = new ItemBaseData(itemcfg);
			uData.Inventory.AddItem(data);
			return data;
		}
		return null;
	}

	public void AddChest(int chestId, ChestSource source, UserData uData)
	{
		ChestData data = new ChestData();
		data.chestId = chestId;
		data.source = source;
		uData.ChestModel.AddChest(data,source);
	}


	
}


