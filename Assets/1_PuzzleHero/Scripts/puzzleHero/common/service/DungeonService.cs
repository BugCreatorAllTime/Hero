
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using strange.examples.strangerocks;

public class DungeonService
{
	[Inject]
	public ConfigManager mainCfg {get;set;}
	[Inject] 
	public ItemService itemService {get;set;}
	[Inject]
	public LoadingManager loadManager {get; set;}
	[Inject]
	public ConfigManager config { get; set;}
	[Inject]
	public SoundManager soundMng { get; set;}

	private const int OK = 0;
	private const int WEAPON = 1;
	private const int ARMOR = 2;
	private const int SHIELD = 3;

	public DungeonService ()
	{
	}

	public ErrorCode GoToDungeon(int id, CrossContextData data, UserData uData, bool checkWearItem)
	{
		int reqEnergy = GetEnergy(id);
		ErrorCode error = ErrorCode.OK;
		if(reqEnergy > uData.energy)
		{
			error = ErrorCode.NOT_ENOUGH_ENERGY;
		}else if(uData.Inventory.IsFull())
		{
			error = ErrorCode.NOT_ENOUGH_SLOT;
		}else if(uData.curMapId < id)
		{
			error = ErrorCode.NOT_UNLOCK;
		} 
		int check = CheckItem (uData);
		if(checkWearItem)
		{
			if(check == WEAPON)
			{
				error = ErrorCode.NOT_WEAR_WEAPON;
			} else if(check == ARMOR)
			{
				error = ErrorCode.NOT_WEAR_ARMOR;
			} else if(check == SHIELD)
			{
				error = ErrorCode.NOT_WEAR_SHIELD;
			}
		}

		if(error == ErrorCode.OK || config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.BEGIN)
		{
			uData.SubEnergy(reqEnergy);
			data.dungeonId = id;
			soundMng.PauseMusic();
			loadManager.SetScreen("Dungeon");
			Application.LoadLevel("Loading");
		}
		return error;
	}

	public int GetEnergy(int dungeonId)
	{
		DungeonConfImpl data = mainCfg.DungeonCfg.getDungeon(dungeonId);
		return data.EnergyReq;
	}

	public Reward DropRewardFromMonster(int mIdx, int level, int dungeonId)
	{
		Reward ret = new Reward();
		DungeonConfImpl dConfig = mainCfg.DungeonCfg.getDungeon(dungeonId);
		MonsterCfgImpl mConfig = mainCfg.MonsterCfg.GetMonsterCfgData(dConfig.IdMonster[mIdx]);
		DungeonConfDropItemImpl dropItemConf = dConfig.GetDungeonConfDropItemImplByIndex(mIdx+1);
		if(mConfig.Type > -1)
		{
			ret.gold = GetGoldDrop(mConfig,level);
			ret.exp = GetExpDrop(mConfig, level);
			if(dropItemConf != null)
			{
				int total = 0;
				int num = UnityEngine.Random.Range(0,100);
				int idxItem = -1;
				for(int i = 0; i < dropItemConf.rates.Length; ++i)
				{
					total += dropItemConf.rates[i];
					if(num < total)
					{
						idxItem = i;
						break;
					}
				}
				if(idxItem > -1)
				{
					int itemType = dropItemConf.types[idxItem];
					int itemColor = dropItemConf.colors[idxItem];
					List<ItemCfgImpl> items = mainCfg.ItemCfg.GetItemsByColor(itemColor);
					for(int i = 0; i< items.Count; ++i)
					{
						if(ItemService.GetSlot(items[i].Id) != itemType)
						{
							items.RemoveAt(i);
							--i;
						}
					}
					if(items.Count > 0)
					{
						ItemCfgImpl itemCfg = items[UnityEngine.Random.Range(0,items.Count)];
						ItemBaseData item = itemService.GenItem(itemCfg);
						ret.item = item;
					}
				}
			}
		}
		return ret;
	}

	public Stat FormularMonsterStat(MonsterCfgImpl cfgData, int level)
	{
		Stat ret = new Stat();
		ret.turn = cfgData.Stat.turn;
		ret.hp = level * cfgData.HpFormularA + cfgData.HpFormularC;
		ret.damage = (cfgData.Type == MonsterCfg.TYPE_BOSS?0:level) + cfgData.DamageFormularC;
		return ret;
	}

	public int GetGoldDrop(MonsterCfgImpl cfgData, int level)
	{
		Stat stat = FormularMonsterStat(cfgData,level);
		int ret = stat.damage + stat.hp / mainCfg.general.GoldMonsterDropFormular;
		return ret;
	}

	public int GetExpDrop(MonsterCfgImpl cfgData, int level)
	{
		Stat stat = FormularMonsterStat(cfgData,level);
		int ret = (stat.hp + stat.damage) / mainCfg.general.ExpMonsterDropFormular;
		return ret;
	}

	private int GetIndexByMatch(Matches.Shape shape)
	{
		int index = 0;
		switch(shape)
		{
		case Matches.Shape.Three:
			index = 0;
			break;
		case Matches.Shape.Four:
			index = 1;
			break;
		case Matches.Shape.Five:
			index = 2;
			break;
		case Matches.Shape.Tl:
			index = 3;
			break;
		}
		return index;
	}

	private int GetBonusCount(Action act)
	{
		int ret = 0;
		switch(act.shape)
		{
		case Matches.Shape.Undefined:
			ret = 1;
			break;
		case Matches.Shape.Three:
			ret = 3;
			break;
		case Matches.Shape.Four:
			ret = 4;
			break;
		case Matches.Shape.Five:
		case Matches.Shape.Tl:
			ret = 5;
			break;
		}
		return act.count - ret;
	}

	public int GetDamageFromMatch(Action act, int dmg)
	{
		float ret;
		int index = GetIndexByMatch(act.shape);
		int bonusCount = GetBonusCount(act);
		int formulaA = act.shape == Matches.Shape.Undefined?3:1;
		ret = dmg * (1 + mainCfg.general.AttackMatchBonusRates[index] / 100f)/ formulaA;
		ret += dmg *(1 + mainCfg.general.AttackMatchBonusRates[0] / 100f) * bonusCount/3f;
		return (int)(ret);
	}

	public int GetGoldFromMatch(Action act)
	{
		float ret;
		int index = GetIndexByMatch(act.shape);
		int bonusCount = GetBonusCount(act);
		int formulaA = act.shape == Matches.Shape.Undefined?3:1;
		ret = (mainCfg.general.GoldMatchBase + mainCfg.general.GoldMatchBonusRates[index])/formulaA;
		ret += (mainCfg.general.GoldMatchBase + mainCfg.general.GoldMatchBonusRates[0]) * bonusCount/3f;
		return (int)(ret);
	}

	public int GetArmorFromMatch(Action act, int maxArmor)
	{
		float ret;
		int index = GetIndexByMatch(act.shape);
		int bonusCount = GetBonusCount(act);
		int formulaA = act.shape == Matches.Shape.Undefined?3:1;
		ret = ((maxArmor * mainCfg.general.ArmorMatchBonusRates[index] / 100f) + mainCfg.general.ArmorMatchBase)/ formulaA;
		ret += ((maxArmor * mainCfg.general.ArmorMatchBonusRates[0] / 100f) + mainCfg.general.ArmorMatchBase) * bonusCount/3f;
		return (int)(ret);
	}

	public int GetHpFromMatch(Action act, int maxHp)
	{
		float ret;
		int index = GetIndexByMatch(act.shape);
		int bonusCount = GetBonusCount(act);
		int formulaA = act.shape == Matches.Shape.Undefined?3:1;
		ret = (maxHp * mainCfg.general.HealMatchBonusRates[index] / 100f)/formulaA;
		ret += maxHp * mainCfg.general.HealMatchBonusRates[0] / 100f * bonusCount/3f;
		return (int)(ret);
	}

	public int GetManaFromMatch(Action act, int maxMana)
	{
		float ret;
		int index = GetIndexByMatch(act.shape);
		int bonusCount = GetBonusCount(act);
		int formulaA = act.shape == Matches.Shape.Undefined?3:1;
		ret = (maxMana * mainCfg.general.ManaMatchBonusRates[index] / 100f)/ formulaA;
		ret += maxMana * mainCfg.general.ManaMatchBonusRates[0] / 100f * bonusCount/3f;
		return (int)(ret);
	}

	public float GetBonusFromCombo(int combo)
	{
		Dictionary<int, float> bonuses = new Dictionary<int, float>();
		int[] bonusesArray = config.general.ComboBonus;
		for (int i = 0; i < bonusesArray.Length / 2; i++)
		{
			bonuses.Add(bonusesArray[i * 2], bonusesArray[i * 2 + 1]);
		}
		int minCombo = bonusesArray[0];
		int maxCombo = bonusesArray[bonusesArray.Length - 2];
		if (combo < minCombo) return 0;
		if (combo >= maxCombo) return bonuses[maxCombo] / 100;
		return bonuses[combo] / 100;
	}

	private int CheckItem(UserData uData)
	{
		bool checkWeapon = false;
		bool checkArmor = false;
		bool checkShield = false;
		for(int i = 0; i < uData.EquippedItemData.Count; i++)
		{
			if(uData.EquippedItemData[i].GetSlot() == ItemCfg.WEAPON_SLOT)
				checkWeapon = true;
			if(uData.EquippedItemData[i].GetSlot() == ItemCfg.ARMOR_SLOT)
				checkArmor = true;
			if(uData.EquippedItemData[i].GetSlot() == ItemCfg.SHIELD_SLOT)
				checkShield = true;
		}
		if (!checkWeapon)
			return WEAPON;
		if (!checkArmor)
			return ARMOR;
		if (!checkShield)
			return SHIELD;
		return OK;
	}
}

public class Reward
{
	public int gold;
	public int exp;
	public ItemBaseData item;

	public override string ToString ()
	{
		return "gold: " + gold +
				"  exp: " + exp +
				"  item: " + (item == null? "Null" : item.Id.ToString());
	}
}


