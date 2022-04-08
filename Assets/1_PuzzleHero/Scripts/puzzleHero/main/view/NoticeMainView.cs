using UnityEngine;
using System.Collections;

public class NoticeMainView : IngameBaseView {

	public UIButton button;

	public override void Setup()
	{
		base.Setup();
		CacheObjectForEventHandling(gameObject);
		
		button = GameObject.Find(MainScreenObjectName.buttonNotice).GetComponent<UIButton>();
		SetupButton(button);
		CacheObjectForEventHandling(button.gameObject);

		gameObject.SetActive (false);
	}
}
