using UnityEngine;
using LitJson;
using System.Collections;
using System.Collections.Generic;

public class DungeonCfg
{
	public Dictionary<string,DungeonConfImpl> dungeon = new Dictionary<string, DungeonConfImpl> ();

	public DungeonConfImpl getDungeon (int id)
	{
		return this.dungeon [id.ToString ()];
	}

}
