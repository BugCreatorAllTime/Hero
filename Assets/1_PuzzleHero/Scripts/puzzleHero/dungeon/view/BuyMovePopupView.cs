using strange.extensions.signal.impl;
using UnityEngine;

public class BuyMovePopupView : IngameBaseView {

	public int numberMove = 1;

	public override void Setup()
	{
		base.Setup();
		CacheObjectForEventHandling(gameObject);
		UIButton next = GameObject.Find(GuiObjectName.buyMoveNext).GetComponent<UIButton>();
		SetupButton(next);
		CacheObjectForEventHandling(next.gameObject);
		UIButton pre = GameObject.Find(GuiObjectName.buyMovePre).GetComponent<UIButton>();
		SetupButton(pre);
		CacheObjectForEventHandling(pre.gameObject);
		UIButton ok = GameObject.Find(GuiObjectName.buyMoveBtOk).GetComponent<UIButton>();
		SetupButton(ok);
		CacheObjectForEventHandling(ok.gameObject);
		UIButton no = GameObject.Find(GuiObjectName.buyMoveBtNo).GetComponent<UIButton>();
		SetupButton(no);
		CacheObjectForEventHandling(no.gameObject);
		GameObject cost = GameObject.Find(GuiObjectName.buyMoveCost);
		CacheObjectForEventHandling(cost);
		GameObject number = GameObject.Find(GuiObjectName.buyMoveNumber);
		CacheObjectForEventHandling(number);
		GameObject disable = GameObject.Find(GuiObjectName.buyMoveDisable);
		CacheObjectForEventHandling(disable);
		gameObject.SetActive (false);
	}
}
