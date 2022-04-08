using System.Collections.Generic;
using UnityEngine;
using GoPlaySDK;
using System.Linq;
using System;

public class MainScreenHandler {
	[Inject]
	public PlaySignal playSignal { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public GoPlayFlushData goPlayFlushData {get;set;}

	private Dictionary<string, GameObject> cachedGameObjects;

	public delegate void OnGuiBeingDisplayed(bool displayed);
	public event OnGuiBeingDisplayed OnGuiBeingDisplayedEvent;

	private Stack<Dictionary<string, GameObject>> popupStack;
	private bool isGuiBeingDisplayed = false;

	[PostConstruct]
	public void InitObjects() {
		cachedGameObjects = new Dictionary<string, GameObject>();
		popupStack = new Stack<Dictionary<string, GameObject>>();
		GoPlaySdk.Instance.OnRegister += HandlOnRegister;
		GoPlaySdk.Instance.OnLogin+= HandleOnLogin;
//		GoPlaySdk.Instance.OnGetProfile += HandleOnGetProfile;
	}

	public void HandleClick(string buttonName) {
		//		Logger.Trace("pauseEventHandler handleClick ", buttonName);
//		Logger.Trace("onclick ", buttonName);
		switch (buttonName) {
			case MainScreenObjectName.settingsButton:
				if(popupStack.Count > 0) break;
				ShowPopup(MainScreenObjectName.settingsPopup);
				break;
			case  MainScreenObjectName.playButton:
//				if (popupStack.Count > 0) break;
//				playSignal.Dispatch();
//			GoPlaySdk.Instance.SaveProgress("1 cai gi do");
				CheckLogin();
				break;
			case MainScreenObjectName.backButton:
				HidePopup(MainScreenObjectName.settingsPopup);
				break;
			case MainScreenObjectName.rateButton:
				string androidUrl = configManager.general.GooglePlayUrl;
				string iosUrl = configManager.general.AppStoreUrl;
				string url = Application.platform == UnityEngine.RuntimePlatform.Android ? androidUrl : iosUrl;
				Application.OpenURL(url);
				break;
			case MainScreenObjectName.registerBtn:
				ChangeTab(MainScreenObjectName.loginPanel, MainScreenObjectName.registerPanel,
			          MainScreenObjectName.loginBtn, MainScreenObjectName.registerBtn);
				break;
			case MainScreenObjectName.loginBtn:
				ChangeTab(MainScreenObjectName.registerPanel, MainScreenObjectName.loginPanel,
			          MainScreenObjectName.registerBtn, MainScreenObjectName.loginBtn);
				break;
			case MainScreenObjectName.cancelLoginBtn:
				HidePopup(MainScreenObjectName.gToken);
				break;
			case MainScreenObjectName.cancelRegisterBtn:
				HidePopup(MainScreenObjectName.gToken);
				break;
			case MainScreenObjectName.okRegisterBtn:
				RegisterAccount();
				break;
			case MainScreenObjectName.buttonNotice:
				HidePopup(MainScreenObjectName.notice);
				break;
			case MainScreenObjectName.okLoginBtn:
				LoginAccount();
				break;
		}
	}

	private void CheckLogin()
	{
		if(GoPlaySdk.Instance.UserSession.HasLoggedIn)
		{
			ShowPopup(MainScreenObjectName.loading);
			goPlayFlushData.Load();
		} else {
			ShowPopup(MainScreenObjectName.gToken);
		}
	}

	private void RegisterAccount()
	{
		string userName = FindGameObject(MainScreenObjectName.usernameRegister).GetComponent<UILabel>().text + "";
		string passWord = FindGameObject(MainScreenObjectName.passwordRegister).GetComponent<UIInput>().value + "";
		string confirmPassword = FindGameObject(MainScreenObjectName.confirmPasswordRegister).GetComponent<UIInput>().value + "";
		if(passWord.Equals(confirmPassword))
		{
			if(Preprocessing(userName, passWord))
			{
				string email = FindGameObject(MainScreenObjectName.emailRegister).GetComponent<UIInput>().value;
				string referral = FindGameObject(MainScreenObjectName.referralRegister).GetComponent<UIInput>().value;
				ShowPopup(MainScreenObjectName.loading);
				HidePopup(MainScreenObjectName.gToken);
				GoPlaySdk.Instance.Register(userName: userName,
				                            password: passWord,
				                            email: email,
				                            referal: referral);
			}
		} else {
			HidePopup(MainScreenObjectName.gToken);
			BannerContent notice = FindGameObject(MainScreenObjectName.notice).GetComponent<BannerContent>();
			notice.textContent.text = "Password confirm is incorrect";
			ShowPopup(MainScreenObjectName.notice);
		}

	}

	private void LoginAccount()
	{
		string userName = FindGameObject(MainScreenObjectName.usernameLogin).GetComponent<UILabel>().text + "";
		string passWord = FindGameObject(MainScreenObjectName.passwordLogin).GetComponent<UIInput>().value + "";
		if(Preprocessing(userName, passWord))
		{
			ShowPopup(MainScreenObjectName.loading);
			HidePopup(MainScreenObjectName.gToken);
			GoPlaySdk.Instance.Login(userName, passWord);
		}
	}


	private bool Preprocessing(string userName, string passWord)
	{
		BannerContent notice = FindGameObject(MainScreenObjectName.notice).GetComponent<BannerContent>();
		switch(CheckUserName(userName))
		{
			case 1:
				notice.textContent.text = "Username is between 3-20 characters";
				ShowPopup(MainScreenObjectName.notice);
				return false;
			default:
				break;
		}
		switch(CheckUserName(passWord))
		{
			case 1:
				notice.textContent.text = "Password must be more than 3 characters";
				ShowPopup(MainScreenObjectName.notice);
				return false;
			default:
				break;
		}
		return true;
	}

	private int CheckUserName(string name)
	{
		if(name.Length < 3 || name.Length > 20)
		{
			return 1;
		}
		return 0;
	}

	private int CheckPassWord(string pass)
	{
		if(pass.Length < 3)
		{
			return 1;
		}
		return 0;
	}

	public void ChangeTab(string obj1, string obj2, string btn1, string btn2)
	{
		FindGameObject (obj1).SetActive (false);
		FindGameObject (obj2).SetActive (true);
		UIButton button1 = FindGameObject (btn1).GetComponent<UIButton>();
		UIButton button2 = FindGameObject (btn2).GetComponent<UIButton>();
		SetImageToButton (button1, "ButtonCoin");
		SetImageToButton (button2, "ButtonCoinPress");
	}

	private void SetImageToButton(UIButton button, string imageName)
	{
		button.normalSprite = imageName;
		button.hoverSprite = imageName;
		button.pressedSprite = imageName;
	}

	public void HandleValueChange(UISlider slider)
	{
		switch (slider.gameObject.name)
		{
			case MainScreenObjectName.musicSlider:
				Logger.Trace(slider, slider.value);
				break;
			case MainScreenObjectName.soundSlider:
				Logger.Trace(slider, slider.value);
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
//		PushToStack(buttonNames);
	}

	public void HidePopup(string popupName) {
//		PopFromStack();
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
			//			Logger.Trace("cache ", go.name);
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
			if(go == null)
			{
				go = GameObjectHardFind(name);
			}
			cachedGameObjects[name] = go;
		}
		return go;
	}

	public GameObject GameObjectHardFind(string str)
	{
		GameObject result = null;
		foreach (GameObject root in GameObject.FindObjectsOfType(typeof(GameObject)))
		{
			if (root.transform.parent == null)
			{ // means it's a root GO
				result = GameObjectHardFind(root, str, 0);
				if (result != null) break;
			}
		}
		return result;
	}

	private GameObject GameObjectHardFind(GameObject item, string str, int index)
	{
		if (index == 0 && item.name == str) return item;
		if (index < item.transform.childCount)
		{
			GameObject result = GameObjectHardFind(item.transform.GetChild(index).gameObject, str, 0);
			if (result == null)
			{
				return GameObjectHardFind(item, str, ++index);
			}
			else
			{
				return result;
			}
		}
		return null;
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

	private void HandlOnRegister(IResult result)
	{
		HidePopup(MainScreenObjectName.loading);
		GoPlaySDK.RegisterResult response = result as GoPlaySDK.RegisterResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			BannerContent notice = FindGameObject(MainScreenObjectName.notice).GetComponent<BannerContent>();
			notice.textContent.text = response.Message;
			ShowPopup(MainScreenObjectName.notice);
		} else {
			GoToGame();
		}
	}

	private void HandleOnLogin(IResult result)
	{
		HidePopup(MainScreenObjectName.loading);
		GoPlaySDK.LoginResult response = result as GoPlaySDK.LoginResult;
		if (response.ErrorCode != GoPlaySDK.Error.None)
		{
			BannerContent notice = FindGameObject(MainScreenObjectName.notice).GetComponent<BannerContent>();
			notice.textContent.text = response.Message;
			ShowPopup(MainScreenObjectName.notice);
		} else {
			goPlayFlushData.Load();
		}
	}

//	private void HandleOnGetProfile(IResult result)
//	{
//		HidePopup(MainScreenObjectName.loading);
//		GoPlaySDK.ProfileResult response = result as GoPlaySDK.ProfileResult;
//		if (response.ErrorCode != GoPlaySDK.Error.None)
//		{
//			BannerContent notice = FindGameObject(MainScreenObjectName.notice).GetComponent<BannerContent>();
//			notice.textContent.text = response.Message;
//			ShowPopup(MainScreenObjectName.notice);
//		} else {
//			GoToGame();
//		}
//	}

	private void GoToGame()
	{
		playSignal.Dispatch();
	}
}