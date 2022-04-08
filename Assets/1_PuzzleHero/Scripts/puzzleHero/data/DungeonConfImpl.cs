using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonConfImpl
{
	public int dungeonId { get; set; }
	public string Name { get; set; }
	public List<int> IdMonster = new List<int>();
	public List<int> LvMonster = new List<int>();
	public int BoardBgId;
	public int BattleBgId;
	public int MiniMapId { get; set;}
	public int EnergyReq { get; set;}
	public Dictionary<string, DungeonConfDropItemImpl> confDropItem;

	public int getId()
	{
		return this.dungeonId;
	}
	public string getName()
	{
		return this.Name;
	}

	public DungeonConfDropItemImpl GetDungeonConfDropItemImplByIndex(int index)
	{
		DungeonConfDropItemImpl ret = null;
		if(confDropItem != null)
		{
			string key = index.ToString();
			confDropItem.TryGetValue(key,out ret);
		}
		return ret;
	}
}


public class DungeonConfDropItemImpl
{
	public int[] rates {get; set;}
	public int[] types {get; set;}
	public int[] colors {get; set;}
}
