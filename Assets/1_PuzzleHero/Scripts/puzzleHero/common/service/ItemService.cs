
using System;
using System.Collections.Generic;

public class ItemService
{
	[Inject]
	public ConfigManager configMgr { get; set; }

	[Inject]
	public UpgradeFxSignal upgradeFxSignal { get; set;}

	public ItemService()
	{
	}

	public int GetItemWeight(ItemCfgImpl item)
	{
		return configMgr.general.WeightPerColors[item.Color];
	}

	public int GetUpgradePrice(List<ItemBaseData> items,ItemBaseData targetItem)
	{
		int ret = 0;
		ItemCfgImpl targetItemCfg = configMgr.ItemCfg.GetItemByItemId(targetItem.Id);
		int w = GetItemWeight(targetItemCfg);
		for (int i = 0; i < items.Count; ++i)
		{
			ItemCfgImpl config = configMgr.ItemCfg.GetItemByItemId(items[i].Id);
			ret += w * configMgr.general.GoldPerWeight;
		}
		return ret;
	}

	public int GetUpgradeChance(List<ItemBaseData> items, ItemBaseData targetItem)
	{
		if (items.Count <= configMgr.general.MaxItemForUpgrade)
		{
			float ret = 0;
			int[] chance = null;
			ItemCfgImpl targetItemCfg = configMgr.ItemCfg.GetItemByItemId(targetItem.Id);
			int targetItemPoint = targetItemCfg.Stat.GetPoint();
			chance = configMgr.general.ItemLevelThreshold;

			if (chance != null)
			{ 
				for (int i = 0; i < items.Count; ++i)
				{
					ItemCfgImpl itemCfg = configMgr.ItemCfg.GetItemByItemId(items[i].Id);
					int indexThresHold = 0;
					for(int j = 0; j < configMgr.general.ItemLevelThreshold.Length; ++j)
					{
						if(targetItem.LevelUpgrade <= configMgr.general.ItemLevelThreshold[j])
						{
							indexThresHold = j;
							break;
						}
					}
					ret += (1 + items[i].LevelUpgrade / 2f) * 100f 
						* itemCfg.Stat.GetPoint() / targetItemPoint 
							/ configMgr.general.ItemLevelThreshold[indexThresHold];
					
				}
			}
			return (int) (ret > 100?100:ret);
		}
		return -1;
	}

	public ErrorCode ProcessUpgradeItem(UserData uData, List<int> itemIds, int targetItemId, bool tutUpgrade = false)
	{
		ItemBaseData targetItem = uData.Inventory.ListItemData[targetItemId];
		if (targetItem.LevelUpgrade < configMgr.general.MaxItemLevelUpgrade)
		{
			List<ItemBaseData> items = GetItemsByIndex(uData, itemIds);
			int goldPrice = GetUpgradePrice(items, targetItem);
			if(goldPrice > uData.gold)
			{
				return ErrorCode.NOT_ENOUGH_GOLD;
			}else
			{
				int chance = GetUpgradeChance(items, targetItem);
				int num = UnityEngine.Random.Range(0, 100);
				uData.SubGold(goldPrice);
				if(RemoveItems(uData, items))
				{
					if (num <= chance || tutUpgrade)
					{
						++targetItem.LevelUpgrade;
						return ErrorCode.OK;
					}
					else
					{
						return ErrorCode.ITEM_UPGRADE_FAIL;
					}
				}else
				{
					return ErrorCode.UNKNOWN_ERROR;
				}
			}


		}
		return ErrorCode.ITEM_MAX_LEVEL;
	}

	public bool RemoveItems(UserData uData, List<ItemBaseData> items)
	{
		int count = 0;
		for (int i = 0; i < items.Count; ++i)
		{
			count += uData.Inventory.RemoveItem(items[i]) ? 1 : 0;
		}
		return items.Count == count;
	}

	public List<ItemBaseData> GetItemsByIndex(UserData uData, List<int> indexes)
	{
		List<ItemBaseData> ret = new List<ItemBaseData>();
		List<ItemBaseData> items = uData.Inventory.ListItemData;
		for (int i = 0; i < indexes.Count; ++i)
		{
			if (indexes[i] < items.Count)
			{
				ret.Add(items[indexes[i]]);
			}
		}
		return ret;
	}

	public ErrorCode EquipItem(ItemBaseData item, UserData uData)
	{
		ItemCfgImpl cfg = configMgr.ItemCfg.GetItemByItemId(item.Id);
		int itemW = GetItemWeight(cfg);
		int curW = 0;
		for (int i = 0; i < uData.EquippedItemData.Count; ++i)
		{
			if (uData.EquippedItemData[i].GetSlot() != item.GetSlot())
				curW += GetItemWeight(configMgr.ItemCfg.GetItemByItemId(uData.EquippedItemData[i].Id));
		}
		int userW = uData.GetWeight();
		if (userW >= itemW + curW)
		{
			return uData.EquipItem(item)?ErrorCode.OK: ErrorCode.UNKNOWN_ERROR;
		}
		else
		{
			return ErrorCode.NOT_ENOUGH_WEIGHT;
		}
	}

	public void TrialItem(ItemBaseData item, UserData uData)
	{
		ItemBaseData oldItem = null;
		for(int i = 0; i < uData.EquippedItemData.Count; ++i)
		{
			if(item.GetSlot() == uData.EquippedItemData[i].GetSlot())
			{
				oldItem = uData.EquippedItemData[i];
				uData.EquippedItemData.RemoveAt(i);
				break;
			}
		}
		
		uData.Inventory.RemoveItem(item);
		uData.EquippedItemData.Add(item);
		if(oldItem != null)
		{
			uData.Inventory.AddItem(oldItem);
		}
	}

	public ErrorCode UnequipItem(ItemBaseData item, UserData uData)
	{
		if(uData.Inventory.IsFull())
		{
			return ErrorCode.NOT_ENOUGH_SLOT;
		}else
		{
			return uData.UnequipItem(item)?ErrorCode.OK: ErrorCode.UNKNOWN_ERROR;
		}
	}

	public void WearItemView(SkeletonAnimation skeAni, List<ItemBaseData> items, string skin, bool activeSet)
	{
		skeAni.skeleton.SetSkin(skin);
		skeAni.skeleton.SetSlotsToSetupPose();

		ItemCfgImpl config = null;
		ItemBaseData itemWeapon = new ItemBaseData();
		ItemBaseData itemShield = new ItemBaseData();
		for (int i = 0; i < items.Count; i++)
		{
			config = configMgr.ItemCfg.GetItemByItemId(items[i].Id);
			if (items[i].GetSlot() == ItemCfg.ARMOR_SLOT)
			{
				SetArmorAttachment(skeAni,config.SetName);
			}
			else
				SetNoneArmorAttachment(skeAni, ItemCfg.GetSlotName(items[i].GetSlot()), config.SetName);

			if(items[i].GetSlot() == ItemCfg.WEAPON_SLOT)
			{
				itemWeapon = items[i];
			}
			if(items[i].GetSlot() == ItemCfg.SHIELD_SLOT)
			{
				itemShield = items[i];
			}
		}
		upgradeFxSignal.Dispatch(itemWeapon, skeAni.gameObject, ItemCfg.WEAPON);
		upgradeFxSignal.Dispatch(itemShield, skeAni.gameObject, ItemCfg.SHIELD);

		if (activeSet)
		{
			config = configMgr.ItemCfg.GetItemByItemId(items[0].Id);
			SetFullSetAttachment(skeAni, config.SetName);
		}
	}

	public void TakeOffItemView(SkeletonAnimation skeAni, ItemBaseData item)
	{
		ItemCfgImpl config = null;
		ItemBaseData itemBase = new ItemBaseData();
		config = configMgr.ItemCfg.GetItemByItemId(item.Id);
		string s = "default";
		if (item.GetSlot() == ItemCfg.ARMOR_SLOT)
		{
			SetArmorAttachment(skeAni, s);
		}
		else
			SetNoneArmorAttachment(skeAni, ItemCfg.GetSlotName(item.GetSlot()), s);
		if (item.GetSlot() == ItemCfg.WEAPON_SLOT)
		{
			upgradeFxSignal.Dispatch(itemBase, skeAni.gameObject, ItemCfg.WEAPON);
		}
		if (item.GetSlot() == ItemCfg.SHIELD_SLOT)
		{
			upgradeFxSignal.Dispatch(itemBase, skeAni.gameObject, ItemCfg.SHIELD);
		}
		SetFullSetAttachment(skeAni,s);
	}

	public void SetArmorAttachment(SkeletonAnimation skeAni, string name)
	{
		skeAni.skeleton.SetAttachment("armor front", name);
		skeAni.skeleton.SetAttachment("armor arm1 lower", name);
		skeAni.skeleton.SetAttachment("armor arm1 upper", name);
		skeAni.skeleton.SetAttachment("armor arm2 lower", name);
		skeAni.skeleton.SetAttachment("armor arm2 upper", name);
		skeAni.skeleton.SetAttachment("armor leg1 lower", name);
		skeAni.skeleton.SetAttachment("armor leg1 upper", name);
		skeAni.skeleton.SetAttachment("armor leg2 lower", name);
		skeAni.skeleton.SetAttachment("armor leg2 upper", name);
	}

	public void SetNoneArmorAttachment(SkeletonAnimation skeAni, string slot, string name)
	{
		skeAni.skeleton.SetAttachment(slot, name);
	}

	public void SetFullSetAttachment(SkeletonAnimation skeAni, string name)
	{
		skeAni.skeleton.SetAttachment("front", name);
		skeAni.skeleton.SetAttachment("back", name);
	}

	public ItemBaseData GenItem(ItemCfgImpl cfg)
	{
		ItemBaseData ret = new ItemBaseData(cfg);
		return ret;
	}


	public int ItemStatFormular(int UpgradeLevel, int stat, int[] rate)
	{
		float ret = 0;
		for(int i = 0; i <= UpgradeLevel - 1; ++i)
		{
			ret += rate[i]/100f;
		}
		ret = (ret + 1) * stat;
		return (int)ret;
	}

	public Stat GetItemStat(ItemBaseData data)
	{
		ItemCfgImpl item = configMgr.ItemCfg.GetItemByItemId(data.Id);
		Stat ret = new Stat();
		ret.hp = ItemStatFormular(data.LevelUpgrade, item.Stat.hp, configMgr.general.ItemStatUpgradeRatePerLevel);
		ret.damage = ItemStatFormular(data.LevelUpgrade, item.Stat.damage, configMgr.general.ItemStatUpgradeRatePerLevel);
		ret.armor = ItemStatFormular(data.LevelUpgrade, item.Stat.armor, configMgr.general.ItemStatUpgradeRatePerLevel);
		return ret;
	}

	public HeroStat GetHeroStat(UserData uData, bool isCountSet = true)
	{
		HeroStat ret = new HeroStat();
		ret.baseStat = configMgr.CharacterCfg.GetCharacterData(uData.CharacterID).Stat;
		for (int i = 0; i < uData.EquippedItemData.Count; ++i)
		{
			Stat stat = GetItemStat(uData.EquippedItemData[i]);
			ret.bonusFixStat.hp += stat.hp;
			ret.bonusFixStat.armor += stat.armor;
			ret.bonusFixStat.damage += stat.damage;
		}

		if(uData.IsActiveSet() && isCountSet)
		{
			ItemCfgImpl config = configMgr.ItemCfg.GetItemByItemId(uData.EquippedItemData[0].Id);
			StatsBonus skillStatsBonus = configMgr.itemSetSkillCfg.GetSkillBonusById(config.SetSkillId);
			ret.UpdateOptionToStat(HeroStat.BONUS_HP,(int)(skillStatsBonus.hp * ret.GetMaxHp()),1);
			ret.UpdateOptionToStat(HeroStat.BONUS_DMG,(int)(skillStatsBonus.damage * ret.GetDmg()),1);
			ret.UpdateOptionToStat(HeroStat.BONUS_ARMOR,(int)(skillStatsBonus.armor * ret.GetMaxArmor()),1);
		}
		return ret;
	}

	public int GetItemPriceSellInShop(ItemCfgImpl cfg)
	{
		int ret = cfg.Stat.GetPoint() * GetItemWeight(cfg);
		return ret;
	}

	public int GetItemPriceSellFromUser(ItemBaseData item)
	{
		ItemCfgImpl cfg = configMgr.ItemCfg.GetItemByItemId(item.Id);
		int ret = GetItemPriceSellInShop(cfg);
		ret = (int)(ret * (1 - configMgr.general.UserSellDecreaseRate / 100f));
		return ret * (item.LevelUpgrade + 1);
	}

	public int GetItemPriceBuyBack(ItemBaseData item)
	{
		ItemCfgImpl cfg = configMgr.ItemCfg.GetItemByItemId(item.Id);
		int ret = GetItemPriceSellInShop(cfg);
		ret = (int)(ret * (1 - configMgr.general.UserBuyBackDecreaseRate / 100f));
		return ret * (item.LevelUpgrade + 1);
	}

	public static int GetSlot(int id)
	{
		return (id / 1000) % 3;
	}

}
