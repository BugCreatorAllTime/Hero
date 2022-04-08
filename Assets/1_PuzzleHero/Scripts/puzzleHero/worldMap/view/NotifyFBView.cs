using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;

public class NotifyFBView : IngameBaseView {

	[Inject]
	public ConfigManager config {get;set;}

	public UIButton fbLoggingButton;
	public UIButton closeNotify;

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		fbLoggingButton = GameObject.Find(MainScreenObjectName.fbLoggingToggleNotify).GetComponent<UIButton>();
		SetupButton(fbLoggingButton);
		CacheObjectForEventHandling(fbLoggingButton.gameObject);

		closeNotify = GameObject.Find(MainScreenObjectName.btCloseNotify).GetComponent<UIButton>();
		SetupButton(closeNotify);
		CacheObjectForEventHandling(closeNotify.gameObject);

		/*if(!FB.IsLoggedIn)
		{
			fbLoggingButton.normalSprite = "FbLogin";
		} else {
			fbLoggingButton.normalSprite = "FbLogout";
		}*/
		
		gameObject.SetActive(false);
	}
}
