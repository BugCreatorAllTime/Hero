using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facebook.MiniJSON;
using strange.examples.strangerocks;
using strange.extensions.signal.impl;
using UnityEngine;

public class FbHandler
{
	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public ShowInfoFbSignal showInfoFbSignal { get; set; }

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public InfoUserService infoFBService { get; set;}

	public const int avatarWidth = 60;
	public const int avatarHeight = 60;
	public const int friendLimit = 25;
	public string username;
	public Texture userTexture;

	public delegate void OnMyPictureLoaded(Texture texture);
	public delegate void OnLoginFaceBook();
	public delegate void OnLogoutFaceBook ();
	public delegate void OnGetListFriend();

	public event OnMyPictureLoaded OnMyPictureLoadedEvent;
	public event OnLoginFaceBook OnLoginFaceBookEvent;
	public event OnLogoutFaceBook OnLogoutFaceBookEvent;
	public event OnGetListFriend OnGetListFriendEvent;

	private int friendsRetrievedCount = 0;
	private static List<object> friends = null;
	private static Dictionary<string, string> profile = null;
	private static List<object> scores = null;
	private static Dictionary<string, Texture> friendImages = new Dictionary<string, Texture>();
	private static Dictionary<string, int> friendScores = new Dictionary<string, int>();
	private bool haveUserPicture = false;
	private LoadingState loadingState = LoadingState.WAITING_FOR_INIT;
	private bool isClickLogFacebook = false;

	enum LoadingState
	{
		WAITING_FOR_INIT,
		WAITING_FOR_INITIAL_PLAYER_DATA,
		DONE
	};

	public void Init()
	{
		FB.Init(SetInit, OnHideUnity);
	}

	public void LogIn()
	{
		if (!FB.IsLoggedIn)
		{
			isClickLogFacebook = true;
			FB.Login("public_profile,user_friends,email,publish_actions", LoginCallback);
		}
	}

	public void LogOut()
	{
		if (FB.IsLoggedIn)
		{
			isClickLogFacebook = true;
			FB.Logout();
			ResetState();
			showInfoFbSignal.Dispatch(new List<InfoUserFBData>());
			NotifyLogoutFaceBook ();
		}
	}

	private void ResetState()
	{
		friendImages.Clear();
		friendScores.Clear();
		username = null;
		userTexture = null;
		friendsRetrievedCount = 0;
		if(profile != null)
			profile.Clear();
		haveUserPicture = false;
	}

	private void OnInitComplete()
	{
		LogIn();
	}

	public bool IsDataReady()
	{
		return loadingState == LoadingState.DONE;
	}

	public void SendScore(int score)
	{
		WWWForm data = new WWWForm();
		data.AddField("score", score);
		FB.API("/v2.0/me/scores", Facebook.HttpMethod.POST, null, data);
	}

	private UIAtlas PackTextures()
	{
		Texture2D[] atlasTextures = new Texture2D[friendImages.Count];
		for (int i = 0; i < atlasTextures.Length; i++)
		{
			atlasTextures[i] = (Texture2D) friendImages.ElementAt(i).Value;
		}
		Rect[] rects;
		Texture2D atlas = new Texture2D(512, 512);
		rects = atlas.PackTextures(atlasTextures, 1, 512);
		List<UISpriteData> uiSpriteDatas = new List<UISpriteData>();
		for (int i = 0; i < rects.Length; i++)
		{
			UISpriteData item = new UISpriteData();
			Rect r = rects[i];
			//Logger.Trace("rect", r);
//			r = NGUIMath.ConvertToPixels(r, atlasTextures[i].width, atlasTextures[i].height, true);
			int x = (int) (r.x * atlas.width);
			int y = atlas.height - avatarHeight - (int)(r.y * atlas.height);
			int width = (int) (r.width * atlas.width);
			int height = (int) (r.height * atlas.height);
			item.SetRect(x, y, width, height);
			item.name = friendImages.ElementAt(i).Key;
			uiSpriteDatas.Add(item);
		}

		GameObject uiAtlasObject = new GameObject();
		GameObject.DontDestroyOnLoad(uiAtlasObject);
		UIAtlas uiAtlas = uiAtlasObject.AddComponent<UIAtlas>();
		uiAtlas.spriteList = uiSpriteDatas;
		Material m = new Material(Shader.Find("Unlit/Transparent Colored"));
		m.SetTexture("_MainTex", atlas);
		uiAtlas.spriteMaterial = m;
		return uiAtlas;
	}

	private void SetInit()
	{
//		Utils.Log("SetInit");
		if (FB.IsLoggedIn)
		{
//			Utils.Log("Already logged in");
			OnLoggedIn();
			loadingState = LoadingState.WAITING_FOR_INITIAL_PLAYER_DATA;
		}
		else
		{
			loadingState = LoadingState.DONE;
		}
	}

	private void OnHideUnity(bool isGameShown)
	{
//		Utils.Log("OnHideUnity");
		if (!isGameShown)
		{
			// pause the game - we will need to hide
			Time.timeScale = 0;
		}
		else
		{
			// start the game back up - we're getting focus again
			Time.timeScale = 1;
		}
	}

	void LoginCallback(FBResult result)
	{
		if (FB.IsLoggedIn)
		{
			OnLoggedIn();
		} else {
			isClickLogFacebook = false;
		}
	}

	string meQueryString = "/v2.0/me?fields=id,first_name";
		//,friends.limit(100).fields(first_name,id,picture.width(70).height(70)),invitable_friends.limit(100).fields(first_name,id,picture.width(70).height(70))";

	void OnLoggedIn()
	{
		infoFBService.DestroyAtlas ();
//		Utils.Log("Logged in. ID: " + FB.UserId);
		// Reqest player info and profile picture
		FB.API(meQueryString, Facebook.HttpMethod.GET, APICallback);
		LoadPictureAPI(Utils.GetPictureURL("me", avatarWidth, avatarHeight), MyPictureCallback);
		// Load high scores
		QueryScores();
		NotifyLoginFaceBook ();
	}

	public void QueryScores()
	{
		FB.API("/app/scores?fields=score,user.limit(" + friendLimit + ")", Facebook.HttpMethod.GET, ScoresCallback);
	}

	void APICallback(FBResult result)
	{
//		Utils.Log("APICallback");
		if (result.Error != null)
		{
//			Utils.LogError(result.Error);
			// Let's just try again
			FB.API(meQueryString, Facebook.HttpMethod.GET, APICallback);
			return;
		}
		profile = Utils.DeserializeJSONProfile(result.Text);
		//GameStateManager.Username = profile["first_name"];
		username = profile["first_name"];
		friends = Utils.DeserializeJSONFriends(result.Text);
		checkIfUserDataReady();
	}

	void MyPictureCallback(Texture texture)
	{
//		Utils.Log("MyPictureCallback");
		if (texture == null)
		{
			// Let's just try again
			LoadPictureAPI(Utils.GetPictureURL("me", avatarWidth, avatarHeight), MyPictureCallback);
			return;
		}
		userTexture = texture;
		haveUserPicture = true;
		NotifyMyPictureLoaded();
		checkIfUserDataReady();
	}

	private int getScoreFromEntry(object obj)
	{
		Dictionary<string, object> entry = (Dictionary<string, object>) obj;
		return Convert.ToInt32(entry["score"]);
	}

	void ScoresCallback(FBResult result)
	{
		if (result.Error != null)
		{
//			Utils.LogError(result.Error);
			return;
		}
		scores = new List<object>();
		ResetState ();
		List<InfoUserFBData> data = new List<InfoUserFBData>();
		List<object> scoresList = Utils.DeserializeScores(result.Text);
		foreach (object score in scoresList)
		{
			var entry = (Dictionary<string, object>) score;
			var user = (Dictionary<string, object>) entry["user"];
			int userData = getScoreFromEntry(entry);
			string userId = (string) user["id"];
			string username = (string) user["name"];
			friendScores[userId] = userData;
			if (string.Equals(userId, FB.UserId))
			{
				// This entry is the current player
				int playerHighScore = getScoreFromEntry(entry);
//				Utils.Log("Local players score on server is " + playerHighScore);
			}
			scores.Add(entry);
			int friendData = friendScores[userId];
			UIAtlas atlas = infoFBService.GetAtlas();
			InfoUserFBData item = PackData(friendData, username, atlas, userId);
			if(item != null)
			{
				if (!friendImages.ContainsKey(userId))
				{
					// We don't have this players image yet, request it now
					LoadPictureAPI(Utils.GetPictureURL(userId, avatarWidth, avatarHeight), pictureTexture =>
					               {
						if (pictureTexture != null)
						{
							friendImages[userId] = pictureTexture;
						}
						friendsRetrievedCount++;
						if (friendsRetrievedCount == scoresList.Count)
						{
							atlas = PackTextures();
							infoFBService.SetAtlas(atlas);
							for (int i = 0; i < data.Count; i++)
							{
								data[i].atlas = atlas;
							}
							NotifyGetListFriend();
						}
					});
				}
				data.Add(item);
			}
		}
		showInfoFbSignal.Dispatch(data);
	}

	InfoUserFBData PackData(int friendData, string username, UIAtlas atlas, string id)
	{
		string fData = friendData + "";
		char[] data = fData.ToCharArray ();
		InfoUserFBData item = null;
		List<int> itemListFriend = new List<int>();
		if(data.Length > 9)
		{
			string s = "" + data [0];
			int gender = Convert.ToInt32 (s) - 1;
			s = "" + data [1] + data [2] + data [3];
			int friendScore = Convert.ToInt32 (s);
			int[] idItem = new int[3];
			s = "" + data [4] + data [5];
			idItem[0] = Convert.ToInt32 (s);
			s = "" + data [6] + data [7];
			idItem[1] = Convert.ToInt32 (s);
			s = "" + data [8] + data [9];
			idItem[2] = Convert.ToInt32 (s);
			if(idItem[0] > 0)
			{
				itemListFriend.Add(config.itemIndexCfg.getWeaponIndex(idItem[0]));
			}
			if(idItem[1] > 0)
			{
				itemListFriend.Add(config.itemIndexCfg.getArmorIndex(idItem[1]));
			}
			if(idItem[2] > 0)
			{
				itemListFriend.Add(config.itemIndexCfg.getShieldIndex(idItem[2]));
			}
			item = new InfoUserFBData(friendScore, atlas, id, username, gender, itemListFriend);
		}
		else {
			item = new InfoUserFBData(friendData, atlas, id, username, 0, itemListFriend);
		}
		return item;
	}

	void checkIfUserDataReady()
	{
//		Utils.Log("checkIfUserDataReady");
		if (loadingState == LoadingState.WAITING_FOR_INITIAL_PLAYER_DATA && haveUserPicture && !string.IsNullOrEmpty(username))
		{
//			Utils.Log("user data ready");
			loadingState = LoadingState.DONE;
		}
	}

	public static void FriendPictureCallback(Texture texture)
	{
		//GameStateManager.FriendTexture = texture;
	}

	delegate void LoadPictureCallback(Texture texture);

	IEnumerator LoadPictureEnumerator(string url, LoadPictureCallback callback)
	{
		WWW www = new WWW(url);
		yield return www;
		try
		{
			callback(www.texture);
		}
		catch (Exception e)
		{
			Logger.Warning(e);
		}
	}

	void LoadPictureAPI(string url, LoadPictureCallback callback)
	{
		FB.API(url, Facebook.HttpMethod.GET, result =>
		{
			if (result.Error != null)
			{
				Utils.LogError(result.Error);
				return;
			}
			var imageUrl = Utils.DeserializePictureURLString(result.Text);
			routineRunner.StartCoroutine(LoadPictureEnumerator(imageUrl, callback));
		});
	}

	void LoadPictureURL(string url, LoadPictureCallback callback)
	{
		routineRunner.StartCoroutine(LoadPictureEnumerator(url, callback));
	}

	private void NotifyMyPictureLoaded()
	{
		if (OnMyPictureLoadedEvent != null)
		{
			OnMyPictureLoadedEvent(userTexture);
		}
	}

	private void NotifyLoginFaceBook()
	{
		if(OnLoginFaceBookEvent != null)
		{
			isClickLogFacebook = false;
			OnLoginFaceBookEvent();
		}
	}

	private void NotifyLogoutFaceBook()
	{
		if(OnLogoutFaceBookEvent != null)
		{
			isClickLogFacebook = false;
			OnLogoutFaceBookEvent();
		}
	}

	private void NotifyGetListFriend()
	{
		if(OnGetListFriendEvent != null)
		{
			OnGetListFriendEvent();
		}
	}

	public void SendScore()
	{
		string data = "" + (config.UserData.CharacterID+1);
		data += Utils.ReturnNumbToString (config.UserData.curMapId, 3);
		int indexWeapon = 0, indexArmor = 0, indexShield = 0;
		int idWeapon = 0, idArmor = 0, idShield = 0;
		for(int i = 0; i < config.UserData.EquippedItemData.Count; i++)
		{
			switch(config.UserData.EquippedItemData[i].GetSlot())
			{
				case ItemCfg.WEAPON_SLOT:
					idWeapon = config.UserData.EquippedItemData[i].Id;
					break;
				case ItemCfg.ARMOR_SLOT:
					idArmor = config.UserData.EquippedItemData[i].Id;
					break;
				case ItemCfg.SHIELD_SLOT:
					idShield = config.UserData.EquippedItemData[i].Id;
					break;
			}
		}
		indexWeapon = config.itemIndexCfg.getWeaponId (idWeapon);
		indexArmor = config.itemIndexCfg.getArmorId (idArmor);
		indexShield = config.itemIndexCfg.getShieldId (idShield);
		data += Utils.ReturnNumbToString (indexWeapon, 2);
		data += Utils.ReturnNumbToString (indexArmor, 2);
		data += Utils.ReturnNumbToString (indexShield, 2);
		SendScore(Convert.ToInt32(data));
	}

	public bool GetIsClickButtonLogFaceBook()
	{
		return isClickLogFacebook;
	}
}