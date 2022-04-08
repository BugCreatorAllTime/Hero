using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HintCfg {

	public Dictionary<string, HintData> hint = new Dictionary<string, HintData>();
	
	public HintData getSound (int id)
	{
		return this.hint [id.ToString ()];
	}
}
