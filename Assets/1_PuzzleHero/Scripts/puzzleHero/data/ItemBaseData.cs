using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class ItemBaseData
{
	public int Id { get; set; }
	public int LevelUpgrade { get; set; }
	public int SetId { get; set; }

	public ItemBaseData() 
	{

	}

	public ItemBaseData (ItemCfgImpl config)
	{
		this.Id = config.Id;
		this.SetId = config.SetId;
		this.LevelUpgrade = 0;
	}

	public int GetSlot()
	{
		return ItemService.GetSlot(Id);
	}

	public ItemBaseData Clone()
	{
		ItemBaseData item = new ItemBaseData();
		item.Id = this.Id;
		item.SetId = this.SetId;
		item.LevelUpgrade = this.LevelUpgrade;
		return item;
	}

	public ItemBaseData CloneStart()
	{
		ItemBaseData item = new ItemBaseData();
		item.Id = this.Id;
		item.SetId = this.SetId;
		item.LevelUpgrade = 0;
		return item;
	}


}
