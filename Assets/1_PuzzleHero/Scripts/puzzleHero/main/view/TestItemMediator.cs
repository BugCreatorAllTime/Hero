using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class TestItemMediator : Mediator
{
	[Inject]
	public ConfigManager configManager { get; set; }

	private bool showChooseItem;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

//	void OnGUI()
//	{
//		GUI.color = Color.red;
//		if (GUI.Button(new Rect(0, 0, 120, 60), "Choose Item"))
//		{
//			showChooseItem = !showChooseItem;
//		}
//		GUI.color = Color.green;
//		if(showChooseItem)
//		{
//			if (GUI.Button(new Rect(0, 80, 120, 60), "Kinght Weapon"))
//			{
//				ChangeItem(13);
//			}
//			if (GUI.Button(new Rect(120, 80, 120, 60), "Kinght Arror"))
//			{
//				ChangeItem(14);
//			}
//			if (GUI.Button(new Rect(240, 80, 120, 60), "Kinght Shield"))
//			{
//				ChangeItem(15);
//			}
//			if (GUI.Button(new Rect(0, 160, 120, 60), "Elven Weapon"))
//			{
//				ChangeItem(16);
//			}
//			if (GUI.Button(new Rect(120, 160, 120, 60), "Elven Arror"))
//			{
//				ChangeItem(17);
//			}
//			if (GUI.Button(new Rect(240, 160, 120, 60), "Elven Shield"))
//			{
//				ChangeItem(18);
//			}
//			if (GUI.Button(new Rect(0, 240, 120, 60), "Bronze Weapon"))
//			{
//				ChangeItem(10);
//			}
//			if (GUI.Button(new Rect(120, 240, 120, 60), "Bronze Arror"))
//			{
//				ChangeItem(11);
//			}
//			if (GUI.Button(new Rect(240, 240, 120, 60), "Bronze Shield"))
//			{
//				ChangeItem(12);
//			}
//			if (GUI.Button(new Rect(0, 320, 120, 60), "Hunter Weapon"))
//			{
//				ChangeItem(7);
//			}
//			if (GUI.Button(new Rect(120, 320, 120, 60), "Hunter Arror"))
//			{
//				ChangeItem(8);
//			}
//			if (GUI.Button(new Rect(240, 320, 120, 60), "Hunter Shield"))
//			{
//				ChangeItem(9);
//			}
//			if (GUI.Button(new Rect(0, 400, 120, 60), "Adventure Weapon"))
//			{
//				ChangeItem(4);
//			}
//			if (GUI.Button(new Rect(120, 400, 120, 60), "Adventure Arror"))
//			{
//				ChangeItem(5);
//			}
//			if (GUI.Button(new Rect(240, 400, 120, 60), "Adventure Shield"))
//			{
//				ChangeItem(6);
//			}
//			if (GUI.Button(new Rect(0, 480, 120, 60), "Default Weapon"))
//			{
//				ChangeItem(1);
//			}
//			if (GUI.Button(new Rect(120, 480, 120, 60), "Default Arror"))
//			{
//				ChangeItem(2);
//			}
//			if (GUI.Button(new Rect(240, 480, 120, 60), "Default Shield"))
//			{
//				ChangeItem(3);
//			}
//		}
//	}
//
//	void ChangeItem(int index)
//	{
//		ItemCfgImpl config = configManager.ItemCfg.GetItemByItemId(index + 1000);
//		ItemBaseData data = new ItemBaseData(config);
//		configManager.UserData.EquipItem(data);
//	}
}
