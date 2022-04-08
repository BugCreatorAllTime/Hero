using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchTutorialCfg {

	public Dictionary<string, MatchTutorialData> matchtutorial = new Dictionary<string, MatchTutorialData>();
	public Dictionary<string, ItemTutorialData> itemtutorial = new Dictionary<string, ItemTutorialData> ();
	
	public MatchTutorialData getMatchTutorial (int id)
	{
		return this.matchtutorial [id.ToString ()];
	}

	public ItemTutorialData getItemTutorial (int id)
	{
		return this.itemtutorial [id.ToString ()];
	}
}
