using UnityEngine;
using System.Collections;

public class GoToWorldMapTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		GameObject.Find ("Tab").GetComponent<LoadInfoTab> ().GoWorldMap ();
	}
}
