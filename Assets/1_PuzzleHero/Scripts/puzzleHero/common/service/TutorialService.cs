
using System;
using UnityEngine;

public class TutorialService {

	[Inject]
	public GoToHouseManager goHouseManager {get;set;}

	[Inject]
	public ConfigManager config { get; set;}

	public bool CheckTownTutorial(UserData uData)
	{
		return uData.currentStepTownTutorial <= config.townTutorialCfg.homeTown.Count;
	}

	public bool CheckInLocation(string name)
	{
		switch(name)
		{
			case "Home":
				return CheckInHome();
				break;
			case "Shop":
				return CheckInShop();
				break;
			case "BlackSmith":
				return CheckInBlackSmith();
				break;
			case "WorldMap":
				return CheckInWorldMap();
				break;
			default:
				return false;
				break;
		}
	}

//	public bool CheckLoadComplete()
//	{
//		return goHouseManager.CheckLoadComplete ();
//	}

	public void SetUserName(string name)
	{
		config.UserData.SetUserName (name);
	}

	public void NextStep(UserData uData)
	{
		uData.NextStep ();
	}

	public Vector3 GetObjectPositioning(string name)
	{
		Vector3 v = new Vector3(-999,-999,0);
		if(name == "empty")
		{
			return v;
		} else {
			GameObject go = GameObject.Find(name);
			if(go == null) return v;
			else return go.transform.localPosition;
		}
	}

	private bool CheckInWorldMap()
	{
		return goHouseManager.main.activeInHierarchy;
	}

	private bool CheckInHome()
	{
		if(goHouseManager.home == null) return false;
		return goHouseManager.home.activeInHierarchy;
	}

	private bool CheckInShop()
	{
		if(goHouseManager.shop == null) return false;
		return goHouseManager.shop.activeInHierarchy;
	}

	private bool CheckInBlackSmith()
	{
		if(goHouseManager.smith == null) return false;
		return goHouseManager.smith.activeInHierarchy;
	}

}
