using UnityEngine;
using System.Collections;

public class SetItemTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		LoadInfoBlacksmith smith = GameObject.Find ("BlackSmith").GetComponent<LoadInfoBlacksmith>();
		smith.isUpgrade = true;
		smith.SetItem (go.transform.parent.gameObject);
	}
}
