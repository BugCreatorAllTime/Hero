using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class SettingsButtonView : IngameBaseView {
	public UIButton settingsButton;

	public override void Setup() {
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		settingsButton = GameObject.Find(MainScreenObjectName.settingsButton).GetComponent<UIButton>();
		SetupButton(settingsButton);
		CacheObjectForEventHandling(settingsButton.gameObject);
	}
}