using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;

public class GoToHouseManager
{
	[Inject]
	public AssetMgr assetMgr { get; set; }
	[Inject]
	public ConfigManager config { get; set;}
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public TutorialService tutorialService{ get; set;}

	public GameObject home;
	public GameObject shop;
	public GameObject main;
	public GameObject smith;
	private LoadInfoTab loadTab;
	public const string MAIN = "MainGUI";
	public const string HomeButton = "HomeButton";
	public const string ShopButton = "ShopButton";
	public const string BackHome = "ButtonHome";
	public const string SmithButon = "BlackSmithButton";
	private bool touch = true;

	[PostConstruct]
	public void PostConstruct()
	{
		main = GameObject.Find (MAIN);
		loadTab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
	}

	private void LoadBlackSmithObject(GameObject go)
	{
		smith = GameObject.Instantiate(go) as GameObject;
		smith.name = "BlackSmith";
		HideHouseInWorldMap ();
		main.SetActive (false);
		smith.SetActive (true);
		config.UserData.SetItemUpdate (false);
		loadTab.checkNotice = true;
		routineRunner.StartCoroutine(loadTab.ProgressHideLoading());
	}

	private void LoadHomeObject(GameObject go)
	{
		home = GameObject.Instantiate(go) as GameObject;
		home.name = "Home";
		HideHouseInWorldMap ();
		main.SetActive (false);
		home.SetActive (true);
		config.UserData.SetItemUpdate (false);
		loadTab.checkNotice = true;
		routineRunner.StartCoroutine(loadTab.ProgressHideLoading());
	}

	private void LoadShopObject(GameObject go)
	{
		shop = GameObject.Instantiate(go) as GameObject;
		shop.name = "Shop";
		if(config.UserData.GetIconUpdate() == 1)
			config.UserData.SetIconUpdate(0);
		HideHouseInWorldMap ();
		main.SetActive (false);
		shop.SetActive (true);
		config.UserData.SetItemUpdate (false);
		loadTab.checkNotice = true;
		routineRunner.StartCoroutine(loadTab.ProgressHideLoading());
	}

	public void GoHouse (string nameHouse)
	{
		routineRunner.StartCoroutine(GoHouseProgress(nameHouse));
	}

	private IEnumerator GoHouseProgress(string nameHouse)
	{
		if(touch)
		{
			touch = false;
			bool isCreate = true;
			switch(nameHouse)
			{
				case HomeButton:
					if(home == null)
					{
						loadTab.ShowLoading();
						assetMgr.GetAsset<GameObject>("Prefabs/GUI/Home", LoadHomeObject);
						isCreate = false;
					}
					break;
				case ShopButton:
					if(shop == null)
					{
						loadTab.ShowLoading();
						assetMgr.GetAsset<GameObject>("Prefabs/GUI/Shop", LoadShopObject);
						isCreate = false;
					}
					break;
				case SmithButon:
					if(smith == null)
					{
						loadTab.ShowLoading();
						assetMgr.GetAsset<GameObject>("Prefabs/GUI/BlackSmith", LoadBlackSmithObject);
						isCreate = false;
					}
					break;
				default:
					break;
			} 
			if(isCreate)
			{
				switch(nameHouse)
				{
					case HomeButton:
						GoToHome();
						break;
					case BackHome:
						GoToWorld();
						break;
					case ShopButton:
						GoToShop();
						break;
					case SmithButon:
						GoToBlackSmith();
						break;
					default:
						break;
				}
			}
			if(loadTab == null)
			{
				loadTab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
			}
			loadTab.CheckTypeTutorial(nameHouse);
		}
		yield return  new WaitForSeconds(0.5f);
		touch = true;
	}

	void GoToWorld()
	{
		if(config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.GO_WM)
		{
			HideHouseInWorldMap ();
			main.SetActive (true);
			if(loadTab == null)
			{
				loadTab = GameObject.Find("Tab").GetComponent<LoadInfoTab>();
			}
			loadTab.CheckHomeUpdate ();
		}
	}

	void GoToBlackSmith()
	{
		HideHouseInWorldMap ();
		main.SetActive (false);
		smith.SetActive (true);
		smith.GetComponent<LoadInfoBlacksmith>().RefreshScreen();
	}

	void GoToShop()
	{
		if(config.UserData.GetIconUpdate() == 1)
			config.UserData.SetIconUpdate(0);
		HideHouseInWorldMap ();
		main.SetActive (false);
		shop.SetActive (true);
		shop.GetComponent<LoadInfoShop>().RefreshScreen();
	}

	void GoToHome()
	{
		HideHouseInWorldMap ();
		main.SetActive (false);
		home.SetActive (true);
		home.GetComponent<LoadInfoCharacter>().RefreshScreen();
		config.UserData.SetItemUpdate (false);
	}

	void HideHouseInWorldMap()
	{
		if(shop != null && shop.activeInHierarchy)
			shop.SetActive (false);
		if(home != null && home.activeInHierarchy)
			home.SetActive (false);
		if(smith != null && smith.activeInHierarchy)
			smith.SetActive (false);
	}

}
