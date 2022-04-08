using strange.extensions.signal.impl;
using UnityEngine;

public class TutorialInfoUIView : IngameBaseView {

	public UIButton button;

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);
		button = GameObject.Find(GuiObjectName.tutButton).GetComponent<UIButton>();
		SetupButton(button);
		CacheObjectForEventHandling(button.gameObject);
		gameObject.SetActive(false);

	}

	public void SetButton()
	{
		SetupButton(button);
	}
}
