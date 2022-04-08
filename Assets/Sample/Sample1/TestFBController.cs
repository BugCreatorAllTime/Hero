using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GoPlaySDK;
enum MainState{
	Login,
	Register,
	Logout,
	None
}

public class TestFBController : MonoBehaviour {
	MainState mainState;
	int buttonW = 200;
	int buttonH = 100;
	string Username = "";
	string Password = "";
	string Referral = "";
	string Status;
	string StatusAfterLogin;

	bool isShowUnbindButton = false;
	bool isLoginSucess = false;
	bool isClickUnfullfilled=false;
	void Awake(){
		mainState = MainState.None;
		GoPlaySdk.Instance.OnLogin+= HandleOnLogin;
		GoPlaySdk.Instance.OnRegister+= HandleOnRegister;
		GoPlaySdk.Instance.OnLogOut+= HandleOnLogOut;
		GoPlaySdk.Instance.OnGetUnFullFilledExchanges += HandleOnGetUnFullFilledExchanges; 
		GoPlaySdk.Instance.OnFullFillExchange += HandleOnFullFillExchange;
		GoPlaySdk.Instance.OnUpdateExternalExchange += HandleOnUpdateExternalExchange;
		GoPlaySdk.Instance.OnRejectExchange += HandleOnRejectExchange;
	}

	public void HandleOnRejectExchange(IResult result)
	{

		if(result.Succeeded)
		{
			//todo
		}

	}
	void HandleOnUpdateExternalExchange (IResult result)
	{
	    if (result.Succeeded) 
        {
			    StatusAfterLogin =  "Success!";
	    } 
        else 
        {
			    StatusAfterLogin = "UpdateExternalExchangeResult Fail! Error Message = " + result.Message;
	    }
	}

	void HandleOnFullFillExchange (IResult result)
	{
        var r = result as FullFillExchangeResult;
        if (r == null)
            return;

		if(r.Succeeded)
        {
			var rs= "Success: "+ r.Succeeded;
			var ex=r.Exchange;
			rs+= ", Exchange: transaction_id: "+ex.TransactionId +", exchange_option_type: ";
			rs+=ex.ExchangeType +", exchange_option_identifier: "+ex.ExchangeOptionIdentifier+", goplay_token_value: "+ ex.GoPlayTokenValue;
			rs+=",  quantity: "+ex.Quantity+", is_free: "+ ex.IsFree+" ";
            StatusAfterLogin =rs;
        }
        else
        {
			StatusAfterLogin = "FullFillExchange Fail! Error Message = " + r.Message;
		}
	}

	void HandleOnGetUnFullFilledExchanges (IResult result)
	{
        var r = result as GetUnFullFilledExchangesResult;
        if (r == null)
            return;

        if (r.Succeeded)
        {
			var exchange=r.Exchanges;
			var rs= "";
			if(exchange!=null && exchange.Count>0){
				var arr=exchange.ToArray();
				rs="Exchanges: Total:"+ exchange.Count;
				for(int i=0;i<exchange.Count;i++)
				{
					rs+=" : Item"+ i +": { transaction_id: "+arr[i].TransactionId +", exchange_option_type: ";
					rs+=arr[i].ExchangeType +", exchange_option_identifier: "+arr[i].ExchangeOptionIdentifier+", goplay_token_value: "+ arr[i].GoPlayTokenValue;
					rs+=", quantity: "+arr[i].Quantity+", is_free: "+ arr[i].IsFree+"}  ";
				}
			}
			if(isClickUnfullfilled)
				StatusAfterLogin =rs==""? "No data" : rs;
        }
        else
        {
            StatusAfterLogin = "GetUnFullFilledExchanges Fail! Error Message = " + r.Message;
        }
	}
	
	void HandleOnLogOut (IResult result)
	{
	    if(result.Succeeded)
        {
		    Status = "Logout Success!";
		    mainState = MainState.None;
		    isLoginSucess = false;
		    IsFullFillShow = false;
		    IsExchangeUiShow = false;
	    }
        else
        {
            Status = "logoutResult Fail! Error Message = " + result.Message;
	    }
	}

	void HandleOnRegister (IResult result)
	{
        var r = result as RegisterResult;
        if (r == null)
            return;
		if(r.Succeeded)
        {
			Status = "Register Success!";
			isShowUnbindButton = false;
			mainState = MainState.None;
		}
        else
        {
            Status = "Register Fail! Error Message = " + r.Message;
		}
	}

	LoginResult _loginResult;

	void HandleOnLogin (IResult result)
	{
        var r = result as LoginResult;
        if (r == null)
            return;
	
		if(r.Succeeded)
        {
			Status = "Login Success! ";
			_loginResult = r;
			isShowUnbindButton = false;
			isLoginSucess = true;
		}
        else
        {
			Status = "Login Fail! Error Message = "+r.Message;	
			if(isShowUnbindButton)
            {
				HandleBindingFacebookFail();
			}
		}
	}

	void HandleBindingFacebookFail(){
		Status = "Your account not Binded with Gtoken Account yet. Please login or register to bind";
		mainState = MainState.Register;	
	}

	void Start(){

	}

	public void OnGUI(){
		MainButtonArea ();
		StatusArea ();
		MainStateUI ();
		UnbindButtonUI ();
		SixbuttonUI ();
		FullFillUI ();
		ExchangeUI ();
	}

	bool IsFullFillShow = false;
	string transID = "";				

	void FullFillUI(){

		if(IsFullFillShow == true){		
			GUILayout.BeginHorizontal();
			GUILayout.Label("Transaction ID:", GUILayout.Width(100));
			transID = GUILayout.TextField(transID);
			
			if (GUILayout.Button ("OK", GUILayout.Width (buttonW / 2), GUILayout.Height (buttonH / 2))) {
				GoPlaySdk.Instance.FullFillExchange(transID);//need register event
				IsFullFillShow = false;
			}
			
			GUILayout.EndHorizontal();
		}
	}

	bool IsExchangeUiShow = false;
	string ExID = "";				

	void ExchangeUI(){
		
		if(IsExchangeUiShow == true){		
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Transaction ID:", GUILayout.Width(100));
			transID = GUILayout.TextField(transID);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Excahnge Option ID:", GUILayout.Width(100));
			ExID = GUILayout.TextField(ExID);
			GUILayout.EndHorizontal();

			if (GUILayout.Button ("OK", GUILayout.Width (buttonW / 2), GUILayout.Height (buttonH / 2))) {

				GoPlaySdk.Instance.UpdateExternalExchange(transID,ExID);//need register event
				IsFullFillShow = false;
			}
			
			GUILayout.EndVertical();
		}
	}

	void SixbuttonUI(){
		if (isLoginSucess) {
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Profile", GUILayout.Width (buttonW / 2), GUILayout.Height (buttonH / 2))) {
				IsFullFillShow = false;
				IsExchangeUiShow = false;
				StatusAfterLogin = "Username = " + _loginResult.Profile.UserName + 
									" == NickName :" + _loginResult.Profile.NickName +
									" == Contry :" + _loginResult.Profile.CountryCode + 
									" == Email :" + _loginResult.Profile.Email;
	
			}

			if (GUILayout.Button ("Get UnFullfilled", GUILayout.Width (buttonW / 1.5f), GUILayout.Height (buttonH / 2))) {
				StatusAfterLogin = "";
				IsFullFillShow = false;
				IsExchangeUiShow = false;
				isClickUnfullfilled=true;
				GoPlaySdk.Instance.GetUnFullFilledExchanges();//need register event
			}

			if (GUILayout.Button ("FullFill", GUILayout.Width (buttonW / 2), GUILayout.Height (buttonH / 2))) {
				StatusAfterLogin = "";
				IsExchangeUiShow = false;
				IsFullFillShow = true;
			}

			if (GUILayout.Button ("Update External Exchange", GUILayout.Width (buttonW / 1.1f), GUILayout.Height (buttonH / 2))) {
				StatusAfterLogin = "";
				IsFullFillShow = false;
				IsExchangeUiShow = true;
				//current not ready
			}
            /*
			if (GUILayout.Button ("Disaouth", GUILayout.Width (buttonW / 2), GUILayout.Height (buttonH / 2))) {
				IsFullFillShow = false;
				IsExchangeUiShow = false;
				GoPlaySDK.Instance.UnBindOAuth();
				Status = "Disaouth Successful!!";
				isLoginSucess = false;
			}
             */

			GUILayout.EndHorizontal();

			GUILayout.Label(StatusAfterLogin);
		}
	}

	void UnbindButtonUI(){
		if (isShowUnbindButton) {
			if (GUILayout.Button ("Not Binding", GUILayout.Width (100))) {
				isShowUnbindButton = false;
				Status = "";
				mainState = MainState.None;
			}		
		}
	}

	void OnApplicationQuit() {
		GoPlaySdk.Instance.LogOut();
	}

	void MainStateUI(){
		if (GoPlaySdk.Instance.UserSession.HasLoggedIn) {
			mainState = MainState.Logout;
		} 

		switch (mainState) {
		case MainState.Login:
			LoginArea (false);
			break;

		case MainState.Register:
			LoginArea(true);
			break;

		case MainState.Logout:
			GUI.color = Color.white;
			if (GUILayout.Button ("Log out", GUILayout.Width (buttonW), GUILayout.Height (buttonH / 2))) {
				GoPlaySdk.Instance.LogOut();
			}
		break;

		case MainState.None:
		default:
			break;
		}
	}

	void MainButtonArea(){
		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Login", GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
		{
			if(mainState != MainState.Logout){
				mainState = MainState.Login;
			}else{
				Status = "You need to Logout First";
			}
		} 
		
		if (GUILayout.Button("Login Facebook", GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
		{
			if(mainState != MainState.Logout){
				mainState = MainState.None;
				if(isShowUnbindButton == false){

					FacebookController.Instance.Login(delegate(string accessToken) {
						isShowUnbindButton = true;
						GoPlaySdk.Instance.Login(SocialPlatforms.FaceBook,accessToken);
					});

				}
			}else{
				Status = "You need to Logout First";
			}
		}
		
		if (GUILayout.Button("Register", GUILayout.Width(buttonW),GUILayout.Height(buttonH)))
		{
			if(mainState != MainState.Logout){
				mainState = MainState.Register;
			}else{
				Status = "You need to Logout First";
			}
		}		
		
		GUILayout.EndHorizontal();	
		GUILayout.Space(15);
		
	}

	void StatusArea(){
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		GUI.color = Color.red;
		GUILayout.Label("Status : ");
		GUILayout.Label(Status);
		GUILayout.EndHorizontal();	
	}

	void LoginArea(bool _isRegister){
		GUI.color = Color.white;
		//UserName:
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		GUILayout.Label("UserName:", GUILayout.Width(100));
		Username = GUILayout.TextField(Username);
		GUILayout.Space(105);
		GUILayout.EndHorizontal();

		//Password		
		GUILayout.BeginHorizontal();
		GUILayout.Space(15);
		GUILayout.Label("Password:", GUILayout.Width(100));
		Password = GUILayout.TextField(Password);
		GUILayout.Space(105);
		GUILayout.EndHorizontal();

		if (_isRegister) {
			//Referral
			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			GUILayout.Label ("Referral:", GUILayout.Width (100));
			Referral = GUILayout.TextField (Referral);
			GUILayout.Space (105);
			GUILayout.EndHorizontal ();

			GUILayout.Space (20);
		}

		GUILayout.BeginHorizontal ();
		GUILayout.Space (118);
		if (GUILayout.Button ("OK", GUILayout.Width (buttonW), GUILayout.Height (buttonH / 2))) {
			switch (mainState) {
			case MainState.Login:
				GoPlaySdk.Instance.Login(Username,Password);
			break;
			case MainState.Register:
				GoPlaySdk.Instance.Register(Username,Password,Referral);
			break;

			default:
			break;
			}
		} 


		GUILayout.EndHorizontal();
	}
}
