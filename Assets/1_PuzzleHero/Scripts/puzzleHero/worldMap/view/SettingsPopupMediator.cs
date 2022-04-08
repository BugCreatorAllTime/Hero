using strange.extensions.mediation.impl;
using UnityEngine;
using strange.examples.strangerocks;
using System.Collections;

public class SettingsPopupMediator : Mediator
{
	[Inject]
	public WorldMapHander worldMapHandler { get; set; }

	[Inject]
	public SettingsPopupView settingsPopupView { get; set; }

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public FbHandler fbHandler {get;set;}

	[Inject]
	public InfoFbManager infoFbManager { get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[PostConstruct]
	public void Listen()
	{
		fbHandler.OnLoginFaceBookEvent += new FbHandler.OnLoginFaceBook (OnLoginFaceBook);
		fbHandler.OnLogoutFaceBookEvent += new FbHandler.OnLogoutFaceBook (OnLogoutFaceBook);
		fbHandler.OnGetListFriendEvent += new FbHandler.OnGetListFriend (OnGetListFriend);
	}

	protected void OnClick(string buttonName) {
		//		Logger.Trace("click");
		worldMapHandler.HandleClick(buttonName);
	}

	protected void CacheObject(GameObject go) {
		worldMapHandler.CacheObject(go);
	}

	private void OnChange(UISlider slider)
	{
		worldMapHandler.HandleValueChange(slider);
	}

	public override void OnRegister()
	{
		base.OnRegister();

		settingsPopupView.onClick.AddListener(OnClick);
		settingsPopupView.cacheSignal.AddListener(CacheObject);
		settingsPopupView.onChange.AddListener(OnChange);
	}

	public void SetStateButtonLoginFaceBookSettingPopup()
	{
		if(!FB.IsLoggedIn)
		{
			settingsPopupView.fbLoggingButton.normalSprite = "FbLogin";
		} else {
			settingsPopupView.fbLoggingButton.normalSprite = "FbLogout";
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
		infoFbManager.HideUser ();
	}

	private void OnGetListFriend()
	{
		routineRunner.StartCoroutine(OnGetListFriendProgress());
		infoFbManager.SetInfoUserToMap ();
	}

	private IEnumerator OnGetListFriendProgress()
	{
		yield return new WaitForSeconds(0.5f);
		infoFbManager.SetInfoUserToMap ();
	}
}