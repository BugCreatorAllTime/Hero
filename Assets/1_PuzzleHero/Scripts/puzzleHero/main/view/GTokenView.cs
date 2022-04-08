using UnityEngine;
using System.Collections;

public class GTokenView : IngameBaseView {

	public UIButton loginBtn;
	public UIButton registerBtn;
	public UIButton okLoginBtn;
	public UIButton cancelLoginBtn;
	public UIButton okRegisterBtn;
	public UIButton cancelRegisterBtn;

	public GameObject lbUsernameLogin;
	public GameObject lbPasswordLogin;
	public GameObject lbUsernameRegister;
	public GameObject lbPasswordRegister;
	public GameObject lbConfirmPasswordRegister;
	public GameObject lbEmailRegister;
	public GameObject lbReferralRegister;

	public GameObject loginPanel;
	public GameObject registerPanel;

	public override void Setup()
	{
		base.Setup();
		
		CacheObjectForEventHandling(gameObject);

		loginBtn = GameObject.Find(MainScreenObjectName.loginBtn).GetComponent<UIButton>();
		SetupButton(loginBtn);
		CacheObjectForEventHandling(loginBtn.gameObject);

		registerBtn = GameObject.Find(MainScreenObjectName.registerBtn).GetComponent<UIButton>();
		SetupButton(registerBtn);
		CacheObjectForEventHandling(registerBtn.gameObject);

		okLoginBtn = GameObject.Find(MainScreenObjectName.okLoginBtn).GetComponent<UIButton>();
		SetupButton(okLoginBtn);
		CacheObjectForEventHandling(okLoginBtn.gameObject);

		cancelLoginBtn = GameObject.Find(MainScreenObjectName.cancelLoginBtn).GetComponent<UIButton>();
		SetupButton(cancelLoginBtn);
		CacheObjectForEventHandling(cancelLoginBtn.gameObject);

		okRegisterBtn = GameObject.Find(MainScreenObjectName.okRegisterBtn).GetComponent<UIButton>();
		SetupButton(okRegisterBtn);
		CacheObjectForEventHandling(okRegisterBtn.gameObject);

		cancelRegisterBtn = GameObject.Find(MainScreenObjectName.cancelRegisterBtn).GetComponent<UIButton>();
		SetupButton(cancelRegisterBtn);
		CacheObjectForEventHandling(cancelRegisterBtn.gameObject);

		lbUsernameLogin = GameObject.Find (MainScreenObjectName.usernameLogin);
		CacheObjectForEventHandling (lbUsernameLogin);
		lbPasswordLogin = GameObject.Find (MainScreenObjectName.passwordLogin);
		CacheObjectForEventHandling (lbPasswordLogin);
		lbUsernameRegister = GameObject.Find (MainScreenObjectName.usernameRegister);
		CacheObjectForEventHandling (lbUsernameRegister);
		lbPasswordRegister = GameObject.Find (MainScreenObjectName.passwordRegister);
		CacheObjectForEventHandling (lbPasswordRegister);
		lbConfirmPasswordRegister = GameObject.Find (MainScreenObjectName.confirmPasswordRegister);
		CacheObjectForEventHandling (lbConfirmPasswordRegister);
		lbEmailRegister = GameObject.Find (MainScreenObjectName.emailRegister);
		CacheObjectForEventHandling (lbEmailRegister);
		lbReferralRegister = GameObject.Find (MainScreenObjectName.referralRegister);
		CacheObjectForEventHandling (lbReferralRegister);

		loginPanel = GameObject.Find (MainScreenObjectName.loginPanel);
		CacheObjectForEventHandling (loginPanel);
		registerPanel = GameObject.Find (MainScreenObjectName.registerPanel);
		CacheObjectForEventHandling (registerPanel);

		registerPanel.gameObject.SetActive (false);
		gameObject.SetActive (false);
	}
}
