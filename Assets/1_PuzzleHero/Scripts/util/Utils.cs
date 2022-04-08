
using Facebook.MiniJSON;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Security.Cryptography;
using System;
using strange.extensions.pool.api;
using Random = UnityEngine.Random;

public class Utils
{
	public static TweenPosition TweenPos (GameObject obj, Vector3 From, Vector3 To, float duration)
	{
		TweenPosition Tween = obj.AddComponent<TweenPosition> ();
		Tween.from = From;
		Tween.to = To;
		Tween.duration = duration;
		Tween.style = UITweener.Style.Once;
		Tween.method = TweenPosition.Method.EaseIn;
		Tween.ignoreTimeScale = false;
		return Tween;
	}

	public static TweenRotation TweenRotation (GameObject obj, Vector3 From, Vector3 To, float duration)
	{
		TweenRotation Tween = obj.AddComponent<TweenRotation> ();
		Tween.from = From;
		Tween.to = To;
		Tween.duration = duration;
		Tween.style = UITweener.Style.Once;
		Tween.method = TweenPosition.Method.EaseIn;
		Tween.ignoreTimeScale = false;
		return Tween;
	}

	public static void TweenNumber (GameObject obj, float from, float to, float duration)
	{
		TweenNumber Tween = obj .AddComponent<TweenNumber> ();
		Tween.from = from;
		Tween.to = to;
		Tween.duration = duration;
		Tween.style = UITweener.Style.Once;
		Tween.method = UITweener.Method.EaseIn;
		Tween.ignoreTimeScale = false;

	}

	public static Sprite CreateSprite (Texture2D text2d)
	{
		return Sprite.Create (text2d, new Rect (0, 0, text2d.width, text2d.height), new Vector2 (0, 0));
	}

	public static int Sign(float value) {
		return value < 0? -1 : 1;
	}

	public static void Shuffle<T>(IList<T> list) {
		var provider = new RNGCryptoServiceProvider();
		int n = list.Count;
		while (n > 1) {
			var box = new byte[1];
			do provider.GetBytes(box);
			while (!(box[0] < n * (Byte.MaxValue / n)));
			var k = (box[0] % n);
			n--;
			var value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	public static void FillDataItem(GameObject itemContent, ItemBaseData itemInfo, ItemService itemService, ConfigManager config)
	{
		ItemContentManager content = itemContent.GetComponent<ItemContentManager>();
		content.imgContent.MakePixelPerfect();
		content.item = itemInfo;
		content.nameItem.alignment = NGUIText.Alignment.Left;
		content.contentIcon.gameObject.SetActive (true);
		content.btInfo.gameObject.SetActive (true);
		content.buttonSell.gameObject.SetActive (true);
		content.iconWeight.gameObject.SetActive (true);
		string levelUpdate = null;
		if(itemInfo.LevelUpgrade > 0) levelUpdate = "+"+itemInfo.LevelUpgrade;
		content.numberWeight.text = itemService.GetItemWeight(config.ItemCfg.GetItemByItemId(itemInfo.Id)).ToString();
		content.nameItem.text = config.ItemCfg.GetItemByItemId(itemInfo.Id).Name+levelUpdate;
		content.icon.spriteName = config.ItemCfg.GetItemByItemId(itemInfo.Id).Icon.ToString();
		content.nameDes.text = content.nameItem.text;
		content.description.text = config.ItemCfg.GetItemByItemId (itemInfo.Id).Description;
		content.icon.MakePixelPerfect();
		switch(config.ItemCfg.GetItemByItemId(itemInfo.Id).Color)
		{
		case 0:
			content.theme.spriteName = config.text.NormalItem;
			content.nameItem.color = Color.white;
			content.nameDes.color = Color.white;
			break;
		case 1:
			content.theme.spriteName = config.text.RareItem;
			content.nameItem.color = new Color32(0,127,248,255);
			content.nameDes.color = new Color32(0,127,248,255);
			break;
		case 2:
			content.theme.spriteName = config.text.LegendItem;
			content.nameItem.color = new Color32(255,0,227,255);
			content.nameDes.color = new Color32(255,0,227,255);
			break;
		}
		content.icon.gameObject.SetActive (true);
		content.nameItem.width = 200;
		content.nameItem.height = 30;
		content.nameItem.transform.localPosition = new Vector3(-16, 38, 0);
		float x = -105;
		int count = 0;
		Stat iStat = itemService.GetItemStat(content.item);
		content.stat = iStat;
		Stat cStat = iStat;
		if(content.item.LevelUpgrade > 0)
		{
			ItemBaseData itemClone = content.item.CloneStart ();
			cStat = itemService.GetItemStat(itemClone);
		}
		if(iStat.damage > cStat.damage)
			content.numberDame.text = cStat.damage+"(+"+(iStat.damage-cStat.damage)+")";
		else content.numberDame.text = iStat.damage.ToString();
		if(iStat.armor > cStat.armor)
			content.numberArmor.text = cStat.armor+"(+"+(iStat.armor-cStat.armor)+")";
		else content.numberArmor.text = iStat.armor.ToString();
		if(iStat.hp > cStat.hp)
			content.numberHp.text = cStat.hp+"(+"+(iStat.hp-cStat.hp)+")";
		else content.numberHp.text = iStat.hp.ToString();
		
		if(iStat.damage > 0)
		{
			content.iconDame.transform.localPosition = new Vector3(x + count * 150,-35,0);
			count++;				
			content.iconDame.gameObject.SetActive(true);
			content.numberDame.gameObject.SetActive(true);
		} else{
			content.iconDame.gameObject.SetActive(false);
			content.numberDame.gameObject.SetActive(false);
		}
		if(iStat.hp > 0)
		{
			content.iconHp.transform.localPosition = new Vector3(x + count * 150,-35,0);
			count++;				
			content.iconHp.gameObject.SetActive(true);
			content.numberHp.gameObject.SetActive(true);
		} else{
			content.iconHp.gameObject.SetActive(false);
			content.numberHp.gameObject.SetActive(false);
		}
		if(iStat.armor > 0)
		{
			content.iconArmor.transform.localPosition = new Vector3(x + count * 150,-35,0);
			count++;				
			content.iconArmor.gameObject.SetActive(true);
			content.numberArmor.gameObject.SetActive(true);
		} else{
			content.iconArmor.gameObject.SetActive(false);
			content.numberArmor.gameObject.SetActive(false);
		}
		int index = 0;
		switch(itemInfo.GetSlot())
		{
			case ItemCfg.WEAPON_SLOT:
				index = 1;
				break;
			case ItemCfg.ARMOR_SLOT:
				index = 2;
				break;
			case ItemCfg.SHIELD_SLOT:
				index = 0;
				break;
		}
		content.type = (itemInfo.GetSlot()+2)%3;
		int indexCompare = -1;
		if(itemInfo.SetId != -1)
		{
			content.iconWeaponSet.gameObject.SetActive (true);
			content.iconWeaponSet.spriteName = "WeaponDeactive";
			content.iconArmorSet.gameObject.SetActive (true);
			content.iconArmorSet.spriteName = "ArmorDeactive";
			content.iconShieldSet.gameObject.SetActive (true);
			content.iconShieldSet.spriteName = "ShieldDeactive";
		} else {
			content.iconWeaponSet.gameObject.SetActive (false);
			content.iconArmorSet.gameObject.SetActive (false);
			content.iconShieldSet.gameObject.SetActive (false);
		}
		int countItemSet = 0;
		for(int i = 0; i < config.UserData.EquippedItemData.Count; i++)
		{
			if(config.UserData.EquippedItemData[i].SetId == itemInfo.SetId && itemInfo.SetId != -1)
			{
				switch(config.UserData.EquippedItemData[i].GetSlot())
				{
					case ItemCfg.WEAPON_SLOT:
						countItemSet++;
						content.iconWeaponSet.spriteName = "WeaponActive";
						break;
					case ItemCfg.ARMOR_SLOT:
						countItemSet++;
						content.iconArmorSet.spriteName = "ArmorActive";
						break;
					case ItemCfg.SHIELD_SLOT:
						countItemSet++;
						content.iconShieldSet.spriteName = "ShieldActive";
						break;
					default:
						break;
				}
			}
			if(index == config.UserData.EquippedItemData[i].GetSlot())
			{
				indexCompare = i;
			}
		}
		int indexChangeColor = -1;
		string textDes = content.description.text;
		for(int i = 0; i < textDes.Length; i++)
		{
			if(textDes[i] == '[')
			{
				indexChangeColor = i;
				break;
			}
		}
		if(countItemSet == 3)
		{
			content.description.text = textDes.Substring(0,indexChangeColor)+"[07AB0F]"+textDes.Substring(indexChangeColor) ;
		} else if(indexChangeColor != -1){
			content.description.text = textDes.Substring(0,indexChangeColor)+"[8E8E8E]"+textDes.Substring(indexChangeColor) ;
		}
		content.description.text = content.description.text.Replace(";",",");
		if(indexCompare != -1)
		{
			Stat eStat = itemService.GetItemStat(config.UserData.EquippedItemData[indexCompare]);
			if(iStat.damage > eStat.damage)
				content.numberDame.color = Color.green;
			if(iStat.damage < eStat.damage)
				content.numberDame.color = Color.red;
			if(iStat.hp > eStat.hp)
				content.numberHp.color = Color.green;
			if(iStat.hp < eStat.hp)
				content.numberHp.color = Color.red;
			if(iStat.armor > eStat.armor)
				content.numberArmor.color = Color.green;
			if(iStat.armor < eStat.armor)
				content.numberArmor.color = Color.red;
			if(iStat.damage == eStat.damage)
				content.numberDame.color = Color.white;
			if(iStat.hp == eStat.hp)
				content.numberHp.color = Color.white;
			if(iStat.armor == eStat.armor)
				content.numberArmor.color = Color.white;
		} else {
			content.numberDame.color = Color.green;
			content.numberHp.color = Color.green;
			content.numberArmor.color = Color.green;
		}
	}

	public static void RemoveEvent(ItemContentManager content)
	{
		content.buttonSell.onClick.Clear ();
		content.btContent.onClick.Clear ();
		content.btInfo.onClick.Clear ();
		content.btContent.hoverSprite = null;
		content.btContent.pressedSprite = null;
		content.contentDes.transform.localPosition = new Vector3(565, content.btContent.transform.localPosition.y,0);
		content.btInfo.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
		content.contentDes.gameObject.SetActive (false);
	}

	public static GameObject ButtonNullSlot(GameObject parent, IPool<GameObject> contentPool, string image, int x, int y, string text, bool setPos = true)
	{
		GameObject item = contentPool.GetInstance();
		item.SetActive(true);
		item.name = "Item";
		item.transform.parent = parent.transform;
		ItemContentManager content = item.GetComponent<ItemContentManager>();
		content.btContent.normalSprite = image;
		content.btContent.hoverSprite = image;
		content.btContent.pressedSprite = image;
		content.nameItem.width = 500;
		content.nameItem.height = 120;
		content.nameItem.transform.localPosition = new Vector3(-16, 5, 0);
		if(setPos)
			item.transform.localPosition = new Vector3(x, y,0);
		content.buttonSell.gameObject.SetActive(false);
		content.nameItem.color = Color.white;
		content.contentIcon.gameObject.SetActive (false);
		content.iconArmor.gameObject.SetActive (false);
		content.iconDame.gameObject.SetActive (false);
		content.iconHp.gameObject.SetActive (false);
		content.iconWeight.gameObject.SetActive (false);
		content.btInfo.gameObject.SetActive (false);
		item.transform.localScale = Vector3.one;
		content.nameItem.text = text;
		content.iconWeaponSet.gameObject.SetActive (false);
		content.iconArmorSet.gameObject.SetActive (false);
		content.iconShieldSet.gameObject.SetActive (false);
		content.nameItem.alignment = NGUIText.Alignment.Center;
		return item;
	}
	public static long GetCurrentTimeInSecond()
	{
		long ret = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
		return ret;
	}

	public static Vector3 GetPosition2D(Transform obj)
	{
		Vector3 pos = new Vector3 (0,0,0);
		Transform parent = obj;
		while(parent.name != "UI Root" || parent == null)
		{
			pos += parent.localPosition;
			parent = parent.parent;
		}
		pos = new Vector3 ((int)pos.x,(int)pos.y,pos.z);
		return pos;
	}

	public static int GetFlag(int flag, int bit)
	{
		int count = 0;
		int result = 0;
		while (count < bit)
		{
			count++;
			result = flag%2;
			flag = flag/2;
		}
		return result;
	}

	public static string GetPictureURL(string facebookID, int? width = null, int? height = null, string type = null) {
		string url = string.Format("/{0}/picture", facebookID);
		string query = width != null ? "&width=" + width.ToString() : "";
		query += height != null ? "&height=" + height.ToString() : "";
		query += type != null ? "&type=" + type : "";
		query += "&redirect=false";
		if (query != "") url += ("?g" + query);
		return url;
	}
	public static Dictionary<string, string> RandomFriend(List<object> friends) {
		var fd = ((Dictionary<string, object>)(friends[Random.Range(0, friends.Count)]));
		var friend = new Dictionary<string, string>();
		friend["id"] = (string)fd["id"];
		friend["first_name"] = (string)fd["first_name"];
		var pictureDict = ((Dictionary<string, object>)(fd["picture"]));
		var pictureDataDict = ((Dictionary<string, object>)(pictureDict["data"]));
		friend["image_url"] = (string)pictureDataDict["url"];
		return friend;
	}
	public static Dictionary<string, string> DeserializeJSONProfile(string response) {
		var responseObject = Json.Deserialize(response) as Dictionary<string, object>;
		object nameH;
		var profile = new Dictionary<string, string>();
		if (responseObject.TryGetValue("first_name", out nameH)) {
			profile["first_name"] = (string)nameH;
		}
		return profile;
	}
	public static List<object> DeserializeScores(string response) {
		var responseObject = Json.Deserialize(response) as Dictionary<string, object>;
		object scoresh;
		var scores = new List<object>();
		if (responseObject.TryGetValue("data", out scoresh)) {
			scores = (List<object>)scoresh;
		}
		return scores;
	}
	public static List<object> DeserializeJSONFriends(string response) {
		var responseObject = Json.Deserialize(response) as Dictionary<string, object>;
		object friendsH;
		var friends = new List<object>();
		if (responseObject.TryGetValue("invitable_friends", out friendsH)) {
			friends = (List<object>)(((Dictionary<string, object>)friendsH)["data"]);
		}
		if (responseObject.TryGetValue("friends", out friendsH)) {
			friends.AddRange((List<object>)(((Dictionary<string, object>)friendsH)["data"]));
		}
		return friends;
	}
	public static string DeserializePictureURLString(string response) {
		return DeserializePictureURLObject(Json.Deserialize(response));
	}
	public static string DeserializePictureURLObject(object pictureObj) {
		var picture = (Dictionary<string, object>)(((Dictionary<string, object>)pictureObj)["data"]);
		object urlH = null;
		if (picture.TryGetValue("url", out urlH)) {
			return (string)urlH;
		}
		return null;
	}
	public static void DrawActualSizeTexture(Vector2 pos, Texture texture, float scale = 1.0f) {
		Rect rect = new Rect(pos.x, pos.y, texture.width * scale, texture.height * scale);
		GUI.DrawTexture(rect, texture);
	}
	public static void DrawSimpleText(Vector2 pos, GUIStyle style, string text) {
		Rect rect = new Rect(pos.x, pos.y, Screen.width, Screen.height);
		GUI.Label(rect, text, style);
	}
	private static void JavascriptLog(string msg) {
		Application.ExternalCall("console.log", msg);
	}
	public static void Log(string message) {
		Debug.Log(message);
		//if (Application.isWebPlayer)
			//JavascriptLog(message);
	}
	public static void LogError(string message) {
		Debug.LogError(message);
		//if (Application.isWebPlayer)
		//	JavascriptLog(message);
	}

	public static string ReturnNumbToString (int numb, int sum)
	{
		string s = "" + numb;
		int sub = sum - s.Length;
		for(int i = 0; i < sub; i++)
		{
			s = "0" + s;
		}
		return s;
	}
}
