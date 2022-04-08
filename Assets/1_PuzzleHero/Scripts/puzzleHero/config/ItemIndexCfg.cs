using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ItemIndexCfg {

	public Dictionary<string, ItemIndexData> WeaponIndex = new Dictionary<string, ItemIndexData>();
	public Dictionary<string, ItemIndexData> ArmorIndex = new Dictionary<string, ItemIndexData>();
	public Dictionary<string, ItemIndexData> ShieldIndex = new Dictionary<string, ItemIndexData>();
	
	public int getWeaponIndex (int id)
	{
		return this.WeaponIndex [id.ToString ()].Index;
	}

	public int getWeaponId(int index)
	{
		for(int i = 0; i < WeaponIndex.Count; i++)
		{
			if(WeaponIndex.ElementAt(i).Value.Index == index)
				return WeaponIndex.ElementAt(i).Value.Id;
		}
		return 0;
	}

	public int getArmorIndex (int id)
	{
		return this.ArmorIndex [id.ToString ()].Index;
	}
	
	public int getArmorId(int index)
	{
		for(int i = 0; i < ArmorIndex.Count; i++)
		{
			if(ArmorIndex.ElementAt(i).Value.Index == index)
				return ArmorIndex.ElementAt(i).Value.Id;
		}
		return 0;
	}

	public int getShieldIndex (int id)
	{
		return this.ShieldIndex  [id.ToString ()].Index;
	}
	
	public int getShieldId(int index)
	{
		for(int i = 0; i < ShieldIndex .Count; i++)
		{
			if(ShieldIndex .ElementAt(i).Value.Index == index)
				return ShieldIndex .ElementAt(i).Value.Id;
		}
		return 0;
	}
	
}
