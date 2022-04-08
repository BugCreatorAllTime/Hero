using System.Collections;
using System.Collections.Generic;
using System;

public class ItemCfg
{
	public const int SHIELD_SLOT = 0;
	public const int WEAPON_SLOT = 1;
	public const int ARMOR_SLOT = 2;

	public const string ARMOR = "armor";
	public const string WEAPON = "weapon";
	public const string SHIELD = "shield";
	public const string DEFAULT = "default";
	
	public const int NORMAL = 0;
	public const int RARE = 1;
	public const int LEGENDARY = 2;

	public Dictionary<string, ItemCfgImpl> itemSet = new Dictionary<string, ItemCfgImpl>();
	public Dictionary<string, ItemCfgImpl> itemNoneSet = new Dictionary<string, ItemCfgImpl>();

	public static string GetSlotName(int slot)
	{
		switch(slot)
		{
		case WEAPON_SLOT:
			return WEAPON;
		case SHIELD_SLOT:
			return SHIELD;
		case ARMOR_SLOT:
			return ARMOR;
		}
		return DEFAULT;
	}

	public List<ItemCfgImpl> GetItemsBySetId(int setId)
	{
		IList<ItemCfgImpl> items = new List<ItemCfgImpl>(itemSet.Values);
		List<ItemCfgImpl> ret = new List<ItemCfgImpl>();
		for (int i = 0; i < items.Count; ++i)
		{
			if (items[i].SetId == setId)
			{
				ret.Add(items[i]);
			}
		}
		return ret;
	}

	public ItemCfgImpl GetItemByItemId(int itemId)
	{
		if (itemId >= 1000 && itemId < 4000)
		{
			return itemSet[itemId.ToString()];
		}
		else
		{
			return itemNoneSet[itemId.ToString()];
		}
	}

	public List<ItemCfgImpl> GetItemsByColor(int color)
	{
		IList<ItemCfgImpl> items = new List<ItemCfgImpl>(itemNoneSet.Values);
		List<ItemCfgImpl> ret = new List<ItemCfgImpl>();
		for (int i = 0; i < items.Count; ++i)
		{
			if (items[i].Color == color)
			{
				ret.Add(items[i]);
			}
		}
		return ret;
	}
	
}
