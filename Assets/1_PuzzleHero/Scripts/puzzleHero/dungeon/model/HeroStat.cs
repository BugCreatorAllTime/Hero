
using System;
public class HeroStat
{
	public const int BONUS_HP = 10;
	public const int BONUS_DMG = 20;
	public const int BONUS_ARMOR = 30;

	public int catId;
	public int factionId;
	public Stat baseStat;
	public Stat bonusFixStat = new Stat();
	public int level;
	private int currentBlock;
	private IntegerWrapper curHp = new IntegerWrapper();
	private IntegerWrapper curArmor = new IntegerWrapper();
	private IntegerWrapper currMana = new IntegerWrapper();

	public void Init()
	{
		CurHp = this.baseStat.hp + this.bonusFixStat.hp;
		CurArmor = this.baseStat.armor + this.bonusFixStat.armor;
		CurrentBlock = 0;
		currMana = 0;
	}

	public int CurHp
	{
		get
		{
			return curHp.GetValue();
		}
		set
		{
			curHp = value;
		}
	}

	public int CurArmor
	{
		get
		{
			return curArmor.GetValue();
		}
		set
		{
			curArmor = value;
		}
	}

	public float CurrentMana
	{
		get { return currMana.GetValue(); }
	}

	public bool AddMana(int num)
	{
		if (num < 0)
		{
			return false;
		}
		currMana += num;
		if (currMana > baseStat.manaPool)
		{
			currMana = baseStat.manaPool;
		}
		return true;
	}

	public void SubMana(int num)
	{
		if(num < 0)
			return;
		currMana -= num;
		currMana = currMana < 0 ? 0 : currMana;
	}

	public int CurrentBlock { get; set; }

	public void Leveling(int level)
	{
		this.level = level;
	}

	public int GetMaxHp()
	{
		int ret = baseStat.hp + bonusFixStat.hp;
		ret = ret < 0 ? 0 : ret;
		return ret;
	}

	public int GetMaxArmor()
	{
		int ret = baseStat.armor + bonusFixStat.armor;
		ret = ret < 0 ? 0 : ret;
		return ret;
	}

	public int GetArmor()
	{
		int ret = baseStat.armor + bonusFixStat.armor;
		ret = ret < 0 ? 0 : ret;
		return ret;
	}

	public int GetDmg()
	{
		int ret = baseStat.damage + bonusFixStat.damage;
		ret = ret < 0 ? 0 : ret;
		return ret;
	}

	public int GetTurn()
	{
		int ret = baseStat.turn + bonusFixStat.turn;
		ret = ret < 0 ? 1 : ret;
		return ret;
	}

	public void AddHp(int value)
	{
		if (value > 0 && CurHp > 0)
		{
			CurHp += value;
			int maxHp = GetMaxHp();
			CurHp = CurHp > maxHp ? maxHp : CurHp;
		}
	}

	public void SubHp(int value)
	{
		if (value > 0)
		{
			CurHp -= value;
			CurHp = CurHp < 0 ? 0 : CurHp;
		}
	}

	public void AddArmor(int value)
	{
		if (value > 0 && CurArmor >= 0)
		{
			CurArmor += value;
			int maxArmor = GetMaxArmor();
			CurArmor = CurArmor > maxArmor ? maxArmor : CurArmor;
		}
	}

	public void SubArmor(int value)
	{
		if (value > 0)
		{
			CurArmor -= value;
			CurArmor = CurArmor < 0 ? 0 : CurArmor;
		}
	}

	public void UpdateOptionToStat(int bonusType, int bonusNum, int mark)
	{
		switch (bonusType)
		{
			case BONUS_HP:
				int dHP = GetMaxHp() - CurHp;
				bonusFixStat.hp += bonusNum * mark;
				if (dHP == 0)
				{
					CurHp = GetMaxHp();
				}
				break;
			case BONUS_ARMOR:
				int dArmor = GetMaxArmor() - CurArmor;
				bonusFixStat.armor += bonusNum * mark;
				if (dArmor == 0)
				{
					CurArmor = GetMaxArmor();
				}
				break;
			case BONUS_DMG:
				bonusFixStat.damage += bonusNum * mark;
				break;
		}
	}

}


