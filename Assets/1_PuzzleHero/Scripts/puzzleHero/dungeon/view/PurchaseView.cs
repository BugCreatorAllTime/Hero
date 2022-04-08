using strange.extensions.signal.impl;
using UnityEngine;

public class PurchaseView : IngameBaseView {

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);
		GameObject purchase = GameObject.Find(GuiObjectName.popupPurchase);
		CacheObjectForEventHandling(purchase);
		UIButton firstButton = GameObject.Find(GuiObjectName.purchaseButtonFirst).GetComponent<UIButton>();
		SetupButton(firstButton);
		CacheObjectForEventHandling(firstButton.gameObject);
		UIButton secondButton = GameObject.Find(GuiObjectName.purchaseButtonSecond).GetComponent<UIButton>();
		SetupButton(secondButton);
		CacheObjectForEventHandling(secondButton.gameObject);
		UIButton thirdButton = GameObject.Find(GuiObjectName.purchaseButtonThird).GetComponent<UIButton>();
		SetupButton(thirdButton);
		CacheObjectForEventHandling(thirdButton.gameObject);
		UIButton fourthButton = GameObject.Find(GuiObjectName.purchaseButtonFourth).GetComponent<UIButton>();
		SetupButton(fourthButton);
		CacheObjectForEventHandling(fourthButton.gameObject);
		UIButton fifthButton = GameObject.Find(GuiObjectName.purchaseButtonFifth).GetComponent<UIButton>();
		SetupButton(fifthButton);
		CacheObjectForEventHandling(fifthButton.gameObject);
		UIButton quitButton = GameObject.Find(GuiObjectName.purchaseQuit).GetComponent<UIButton>();
		SetupButton(quitButton);
		CacheObjectForEventHandling(quitButton.gameObject);

		Debug.Log ("set inactive");
		gameObject.SetActive(false);
	}
}
