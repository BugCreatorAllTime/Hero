using strange.extensions.signal.impl;
using UnityEngine;

public class ReviveView : IngameBaseView
{
	public UIButton no;
	public UIButton revive;

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		no = GameObject.Find(GuiObjectName.revivePopup_NoButton).GetComponent<UIButton>();
		SetupButton(no);
		CacheObjectForEventHandling(no.gameObject);
		
		revive = GameObject.Find(GuiObjectName.revivePopup_ReviveButton).GetComponent<UIButton>();
		SetupButton(revive);
		CacheObjectForEventHandling(revive.gameObject);

		GameObject disable = GameObject.Find(GuiObjectName.reviveDisabel);
		CacheObjectForEventHandling(disable);

		gameObject.SetActive(false);
	}
}