using strange.extensions.signal.impl;
using UnityEngine;

public class DefeatedView : IngameBaseView
{
	public UIButton surrender;
	public UIButton revive;

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		surrender = GameObject.Find(GuiObjectName.defeatedPopup_SurrenderButton).GetComponent<UIButton>();
		SetupButton(surrender);
		CacheObjectForEventHandling(surrender.gameObject);
		revive = GameObject.Find(GuiObjectName.defeatedPopup_ReviveButton).GetComponent<UIButton>();
		SetupButton(revive);
		CacheObjectForEventHandling(revive.gameObject);
		GameObject defeatedReviveLabel = GameObject.Find (GuiObjectName.defeatedPopup_ReviveLabel);
		CacheObjectForEventHandling (defeatedReviveLabel);
		GameObject reviveReviveLabel = GameObject.Find (GuiObjectName.revivePopup_ReviveLabel);
		CacheObjectForEventHandling (reviveReviveLabel);
		GameObject grid = GameObject.Find (GuiObjectName.gridDefeated);
		CacheObjectForEventHandling (grid);
		GameObject exp = GameObject.Find (GuiObjectName.defeatedExp);
		CacheObjectForEventHandling (exp);
		GameObject gold = GameObject.Find (GuiObjectName.defeatedGold);
		CacheObjectForEventHandling (gold);
		GameObject progress = GameObject.Find (GuiObjectName.defeatedExpProgress);
		CacheObjectForEventHandling (progress);
		GameObject disable = GameObject.Find (GuiObjectName.defeatedDisable);
		CacheObjectForEventHandling (disable);
		gameObject.SetActive(false);
	}
}