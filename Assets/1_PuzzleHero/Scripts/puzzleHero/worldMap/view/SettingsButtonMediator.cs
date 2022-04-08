using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
using strange.examples.strangerocks;

public class SettingsButtonMediator : Mediator {
	[Inject]
	public WorldMapHander worldMapHandler { get; set; }

	[Inject]
	public SettingsButtonView settingsButtonView { get; set; }

	protected void OnButtonClick(string buttonName) {
		//		Logger.Trace("click");
		worldMapHandler.HandleClick(buttonName);
	}

	protected void CacheObject(GameObject go) {
		worldMapHandler.CacheObject(go);
	}

	public override void OnRegister() {
		base.OnRegister();

		settingsButtonView.onClick.AddListener(OnButtonClick);
		settingsButtonView.cacheSignal.AddListener(CacheObject);
	}
}
