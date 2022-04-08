using UnityEngine;
using System.Collections;

public class EquipItemTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		LoadInfoCharacter home = GameObject.Find ("Home").GetComponent<LoadInfoCharacter>();
		home.ClickEquip (go.transform.parent.gameObject);
	}
}
