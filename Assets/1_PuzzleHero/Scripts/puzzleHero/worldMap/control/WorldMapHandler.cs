using UnityEngine;
using GoPlaySDK;
using System.Collections;
using System.Collections.Generic;

public class WorldMapHander {

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public LoadingManager loadManager { get; set; }

	[Inject]
	public FbHandler fbHandler { get; set; }

	[Inject]
	public GoPlayFlushData goPlayFlushData { get; set;}

	public Dictionary<string, GameObject> cachedGameObjects;

	public delegate void OnGuiBeingDisplayed(bool displayed);
	public event OnGuiBeingDisplayed OnGuiBeingDisplayedEvent;

	private Stack<Dictionary<string, GameObject>> popupStack;
	private bool isGuiBeingDisplayed = false;

	[PostConstruct]
	public void InitObjects() {
		cachedGameObjects = new Dictionary<string, GameObject>();
		popupStack = new Stack<Dictionary<string, GameObject>>();
		if(GoPlaySdk.Instance.GetOnLogoutLength() < 1)
			GoPlaySdk.Instance.OnLogOut += HandlOnLogOut;
	}

	public void HandleClick(string buttonName) {
		//		Logger.Trace("pauseEventHandler handleClick ", buttonName);
		//Logger.Trace("onclick ", buttonName);
		switch (buttonName) {
			case MainScreenObjectName.settingsButton:
				if(popupStack.Count > 0) break;
				ShowPopup(MainScreenObjectName.settingsPopup);
				break;
			case MainScreenObjectName.backButton:
				HidePopup(MainScreenObjectName.settingsPopup);
				break;
			case MainScreenObjectName.btCloseNotify:
				HidePopup(MainScreenObjectName.notifyFB);
				break;
			case MainScreenObjectName.rateButton:
				string androidUrl = configManager.general.GooglePlayUrl;
				string iosUrl = configManager.general.AppStoreUrl;
				string url = Application.platform == UnityEngine.RuntimePlatform.Android ? androidUrl : iosUrl;
				Application.OpenURL(url);
				break;
			/*case MainScreenObjectName.fbLoggingToggle:
				if(!fbHandler.GetIsClickButtonLogFaceBook())
				{
					if (!FB.IsLoggedIn)
					{
						fbHandler.LogIn();
					}
					else
					{
						fbHandler.LogOut();
					}
				}
				break;
			case MainScreenObjectName.fbLoggingToggleNotify:
				if(!fbHandler.GetIsClickButtonLogFaceBook())
				{
					if (!FB.IsLoggedIn)
					{
						fbHandler.LogIn();
					}
					else
					{
						fbHandler.LogOut();
					}
				}
				break;
			case MainScreenObjectName.logoutButton:
				LogoutAccount();*/
				break;
		}
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
	}

	public void HandleValueChange(UISlider slider)
	{
		switch (slider.gameObject.name)
		{
			case MainScreenObjectName.musicSlider:
				configManager.UserData.musicValue = slider.value;
				soundMng.SetVolumeMusic((float)slider.value);
				break;
			case MainScreenObjectName.soundSlider:
				configManager.UserData.soundValue = slider.value;
				break;
		}
	}

	public void ShowPopup(string popupName) {
		GameObject popup = FindGameObject(popupName);
		popup.SetActive(true);
		Component[] buttons = popup.GetComponentsInChildren(typeof(UIButton));
		string[] buttonNames = new string[buttons.Length];
		for (int i = 0; i < buttons.Length; i++) {
			buttonNames[i] = buttons[i].gameObject.name;
		}
		PushToStack(buttonNames);
	}

	public void HidePopup(string popupName) {
		PopFromStack();
		FindGameObject(popupName).SetActive(false);
	}

	private void PushToStack(string[] buttonNames) {
		Dictionary<string, GameObject> item = new Dictionary<string, GameObject>();
		for (int i = 0; i < buttonNames.Length; i++) {
			GameObject go = FindGameObject(buttonNames[i]);
			item[go.name] = go;
		}
		popupStack.Push(item);
	}

	private bool IsOnTop(string gameObjectName) {
		GameObject go = null;
		if (popupStack.Count <= 0) {
			return false;
		}
		popupStack.Peek().TryGetValue(gameObjectName, out go);
		return go != null;
	}

	private Dictionary<string, GameObject> PopFromStack() {
		return popupStack.Pop();
	}

	public void CacheObject(GameObject go) {
		if (go == null) {
			return;
		}
		GameObject cachedObject = null;
		cachedGameObjects.TryGetValue(go.name, out cachedObject);
		if (cachedObject == null) {
//						Logger.Trace("cache ", go.name);
			cachedGameObjects[go.name] = go;
		}
	}

	public bool IsGuiBeingDisplayed() {
		return isGuiBeingDisplayed;
	}

	public void SetGuiBeingDisplayed(bool isBeingDisplayed) {
		this.isGuiBeingDisplayed = isBeingDisplayed;
		if (OnGuiBeingDisplayedEvent != null) {
			OnGuiBeingDisplayedEvent(isBeingDisplayed);
		}
	}

	private GameObject FindGameObject(string name) {
		GameObject go = null;
		cachedGameObjects.TryGetValue(name, out go);
		if (go == null) {
			go = GameObject.Find(name);
			cachedGameObjects[name] = go;
		}
		return go;
	}

	private GameObject FindChild(GameObject parent, string childName) {
		GameObject go = null;
		cachedGameObjects.TryGetValue(childName, out go);
		if (go == null) {
			go = parent.transform.Find(childName).gameObject;
			cachedGameObjects[childName] = go;
		}
		return go;
	}

	private void OnMyPictureLoaded(Texture texture)
	{
		GameObject avatar = GameObject.Find("Avatar");
		Sprite s = Sprite.Create((Texture2D) texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 1);
		avatar.GetComponent<SpriteRenderer>().sprite = s;
	}

	private void FillFbData()
	{
		
	}

	public void ShowNotifyFB()
	{
		if (popupStack.Count > 0)
						return;
		ShowPopup(MainScreenObjectName.notifyFB);
	}

	public void LogoutAccount()
	{
		GameObject.Find ("Tab").GetComponent<LoadInfoTab> ().ShowLoading ();
		goPlayFlushData.ResetDataClient ();
		GoPlaySdk.Instance.LogOut();
	}

	private void HandlOnLogOut(IResult result)
	{
		GoPlaySdk.Instance.ClearEventDelegate ();
		goPlayFlushData.isInGame = false;
		GameObject.Find ("DestroyProgress").GetComponent<DestroyProgress>().DeleteAll();
		Application.LoadLevel("Main");
	}

}