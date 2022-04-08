using UnityEngine;
using System.Collections;

public class LoadingView : IngameBaseView {

	public override void Setup()
	{
		base.Setup();
		CacheObjectForEventHandling(gameObject);
		gameObject.SetActive (false);
	}
}
