using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CharacterCfg
{
	public Dictionary<string, CharacterData> character = new Dictionary<string, CharacterData>();
	public Dictionary<string, CharacterConstants> config;

	public CharacterData GetCharacterData(int id)
	{
		return this.character[id.ToString()];
	}

	public int GetMaxEnergy()
	{
		return config.ElementAt(0).Value.MaxEnergy;
	}

	public List<int> GetExpSteps()
	{
		return config.ElementAt(0).Value.ExpSteps;
	}
}
public class CharacterData
{
	public int Id { get; set; }
	public string Name { get; set; }
	public int Weight {get;set;}
	public Stat Stat = new Stat();
	public int InitSlot {get; set;}
	public int InitGold {get; set;}
	public int InitGem {get; set;}
	public List<int> InitItems {get;set;}
}

public class CharacterConstants
{
	public int MaxEnergy;
	public List<int> ExpSteps;
}