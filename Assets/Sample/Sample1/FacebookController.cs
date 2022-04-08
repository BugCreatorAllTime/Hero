using UnityEngine;
using System.Collections;

public class FacebookController : MonoBehaviour {
	static FacebookController __sharedInstance = null;
	public delegate void LoginCallback(string acceesToken);

	static public FacebookController Instance {
		get{
			return __sharedInstance;
		}
	}
	
	void Awake(){
		__sharedInstance = this;

		if (!FB.IsInitialized) {
			FB.Init(delegate{
				Debug.Log("FB Init");
			});
		}
	}

	public void Login(LoginCallback loginDelegate){
		if (!FB.IsLoggedIn) {
			FB.Login ("", delegate(FBResult result) {
				if (FB.IsLoggedIn) {
					loginDelegate (FB.AccessToken);
				} else {
					Debug.Log ("Login Facebook Fail!");
				}
			});
		} else {
			loginDelegate (FB.AccessToken);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
