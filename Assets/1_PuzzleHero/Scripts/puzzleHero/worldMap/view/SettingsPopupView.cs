using strange.extensions.signal.impl;
using UnityEngine;

public class SettingsPopupView : IngameBaseView
{
	[Inject]
	public ConfigManager config {get;set;}

	public UISlider musicSlider;
	public UISlider soundSlider;
	public UIButton fbLoggingButton;
	public UIButton backButton;
	public UIButton rateButton;
	public UIButton logoutButton;

	public Signal<UISlider> onChange = new Signal<UISlider>();

	public override void Setup()
	{
		base.Setup();

		CacheObjectForEventHandling(gameObject);

		backButton = GameObject.Find(MainScreenObjectName.backButton).GetComponent<UIButton>();
		SetupButton(backButton);
		CacheObjectForEventHandling(backButton.gameObject);

		rateButton = GameObject.Find(MainScreenObjectName.rateButton).GetComponent<UIButton>();
		SetupButton(rateButton);
		CacheObjectForEventHandling(rateButton.gameObject);

		logoutButton = GameObject.Find(MainScreenObjectName.logoutButton).GetComponent<UIButton>();
		SetupButton(logoutButton);
		CacheObjectForEventHandling(logoutButton.gameObject);

		musicSlider = GameObject.Find(MainScreenObjectName.musicSlider).GetComponent<UISlider>();
		musicSlider.value = (float)config.UserData.musicValue;
		SetupSlider(musicSlider);
		CacheObjectForEventHandling(musicSlider.gameObject);

		soundSlider = GameObject.Find(MainScreenObjectName.soundSlider).GetComponent<UISlider>();
		soundSlider.value = (float)config.UserData.soundValue;
		SetupSlider(soundSlider);
		CacheObjectForEventHandling(soundSlider.gameObject);

		fbLoggingButton = GameObject.Find(MainScreenObjectName.fbLoggingToggle).GetComponent<UIButton>();
		SetupButton(fbLoggingButton);
		CacheObjectForEventHandling(fbLoggingButton.gameObject);
		
		/*if(!FB.IsLoggedIn)
		{
			fbLoggingButton.normalSprite = "FbLogin";
		} else {
			fbLoggingButton.normalSprite = "FbLogout";
		}*/

		gameObject.SetActive(false);
	}

	private void OnChange(UISlider slider)
	{
		onChange.Dispatch(slider);
	}

	private void SetupSlider(UISlider slider)
	{
		EventDelegate eventDelegate = new EventDelegate(this, "OnChange");
		EventDelegate.Parameter parms = new EventDelegate.Parameter(slider, "slider");
		eventDelegate.parameters[0] = parms;
		slider.onChange.Add(eventDelegate);
	}
}