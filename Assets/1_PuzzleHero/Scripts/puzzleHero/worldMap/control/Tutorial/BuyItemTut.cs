using UnityEngine;
using System.Collections;

public class BuyItemTut : ClickTutHandle {

	public override void ProgressClick(GameObject go)
	{
		LoadInfoShop shop = GameObject.Find ("Shop").GetComponent<LoadInfoShop>();
		shop.BuyItem (go.transform.parent.parent.gameObject);
	}
}
