using strange.extensions.mediation.impl;
using UnityEngine;

public class NotifyFBMediator : Mediator {

	[Inject]
	public WorldMapHander worldMapHandler { get; set; }
	
	[Inject]
	public NotifyFBView notifyFBView { get; set; }
	
	[Inject]
	public ConfigManager config { get; set;}
	
	[Inject]
	public FbHandler fbHandler {get;set;}

	[PostConstruct]
	public void Listen()
	{
		fbHandler.OnLoginFaceBookEvent += new FbHandler.OnLoginFaceBook (OnLoginFaceBook);
		fbHandler.OnLogoutFaceBookEvent += new FbHandler.OnLogoutFaceBook (OnLogoutFaceBook);
	}

	protected void OnClick(string buttonName) {
		//		Logger.Trace("click");
		worldMapHandler.HandleClick(buttonName);
	}
	
	protected void CacheObject(GameObject go) {
		worldMapHandler.CacheObject(go);
	}

	public override void OnRegister()
	{
		base.OnRegister();
		
		notifyFBView.onClick.AddListener(OnClick);
		notifyFBView.cacheSignal.AddListener(CacheObject);
	}

	public void SetStateButtonLoginFaceBookSettingPopup()
	{
		if(!FB.IsLoggedIn)
		{
			notifyFBView.fbLoggingButton.normalSprite = "FbLogin";
		} else {
			notifyFBView.fbLoggingButton.normalSprite = "FbLogout";
		}
	}
	
	private void OnLoginFaceBook()
	{
		if(config.UserData.GetFirstLoginFB() == 1)
		{
			config.UserData.AddGem (5);
			config.UserData.AddGold (5000);
			config.UserData.AddEnergy (12);
			config.UserData.SetFirstLoginFB (0);
		}
		SetStateButtonLoginFaceBookSettingPopup ();
		fbHandler.SendScore ();
	}
	
	private void OnLogoutFaceBook()
	{
		SetStateButtonLoginFaceBookSettingPopup ();
	}
}
