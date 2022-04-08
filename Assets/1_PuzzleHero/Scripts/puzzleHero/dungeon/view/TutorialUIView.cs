using strange.extensions.signal.impl;
using UnityEngine;

public class TutorialUIView : IngameBaseView {

	public UIButton button;

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);
		button = GameObject.Find(GuiObjectName.tutChaButton).GetComponent<UIButton>();
		SetupButton(button);
		CacheObjectForEventHandling(button.gameObject);
		
		gameObject.SetActive(false);

	}

	public void SetButton()
	{
		SetupButton(button);
	}
}
