using UnityEngine;
using System.Collections;

public class ShowInfoTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		LoadInfoCharacter home = GameObject.Find ("Home").GetComponent<LoadInfoCharacter>();
		home.ShowInfo (go.transform.parent.parent.gameObject);
	}
}
