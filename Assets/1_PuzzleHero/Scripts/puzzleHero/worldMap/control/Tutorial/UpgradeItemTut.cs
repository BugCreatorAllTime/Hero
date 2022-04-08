using UnityEngine;
using System.Collections;

public class UpgradeItemTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		GameObject.Find ("Tab").GetComponent<LoadInfoTab> ().ReturnBlackObject ();
	}
}
