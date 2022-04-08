using UnityEngine;
using System.Collections;

public class SelectTabTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		int indexTab = go.transform.parent.GetComponent<ClickTab> ().indexTab;
		TabManager tm = go.transform.parent.parent.GetComponent<TabManager>();
		tm.indexTabChoice = indexTab;
		tm.SetTab ();
	}
}
