using strange.extensions.signal.impl;
using UnityEngine;

public class VictoryView : IngameBaseView {

	public UIButton next;
	public UIButton town;
	public UIProgressBar expProgress;

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);
		
		next = GameObject.Find(GuiObjectName.victoryPopup_NextButton).GetComponent<UIButton>();
		SetupButton(next);
		CacheObjectForEventHandling(next.gameObject);
		town = GameObject.Find(GuiObjectName.victoryPopup_TownButton).GetComponent<UIButton>();
		SetupButton(town);
		CacheObjectForEventHandling(town.gameObject);
		GameObject gold = GameObject.Find (GuiObjectName.victoryGold);
		CacheObjectForEventHandling (gold);
		GameObject grid = GameObject.Find (GuiObjectName.gridVictory);
		CacheObjectForEventHandling (grid);
		GameObject expAdd = GameObject.Find (GuiObjectName.expAdd);
		CacheObjectForEventHandling (expAdd);
		expProgress = GameObject.Find (GuiObjectName.expProgress).GetComponent<UIProgressBar>();
		CacheObjectForEventHandling (expProgress.gameObject);
		GameObject disable = GameObject.Find (GuiObjectName.victoryDisable);
		CacheObjectForEventHandling (disable);
		gameObject.SetActive(false);
	}

}
