using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniMapCfg
{
	public Dictionary<string, MiniMapData> minimap = new Dictionary<string, MiniMapData>();
	
	public MiniMapData getMiniMap (int id)
	{
		return this.minimap [id.ToString ()];
	}
}