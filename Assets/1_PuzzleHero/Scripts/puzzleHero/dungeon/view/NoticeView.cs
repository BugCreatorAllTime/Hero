using strange.extensions.signal.impl;
using UnityEngine;

public class NoticeView : IngameBaseView {

	public UIButton button;

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);
		
		button = GameObject.Find(GuiObjectName.noticeButton).GetComponent<UIButton>();
		SetupButton(button);
		CacheObjectForEventHandling(button.gameObject);
		UIButton btNo = GameObject.Find(GuiObjectName.noticeNo).GetComponent<UIButton>();
		SetupButton(btNo);
		CacheObjectForEventHandling(btNo.gameObject);
		UIButton btYes = GameObject.Find(GuiObjectName.noticeYes).GetComponent<UIButton>();
		SetupButton(btYes);
		CacheObjectForEventHandling(btYes.gameObject);
		GameObject content = GameObject.Find(GuiObjectName.noticeContent);
		CacheObjectForEventHandling(content);
		GameObject disable = GameObject.Find(GuiObjectName.noticeDisable);
		CacheObjectForEventHandling(disable);
		gameObject.SetActive(false);
	}

	public void SetButton()
	{
		SetupButton(button);
	}

}
