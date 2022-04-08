using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ClickTutManager {

	private Dictionary<int, ClickTutHandle> nameEvents;
	private const int EQUIP = 1;
	private const int SELECT = 2;
	private const int SHOW_INFO = 3;
	private const int BUY_ITEM = 4;
	private const int SET_ITEM = 5;
	private const int UPGRADE_ITEM = 6;
	private const int BACK_WORLDMAP = 7;

	[PostConstruct]
	public void PostConstruct()
	{
		nameEvents = new Dictionary<int, ClickTutHandle>();
		nameEvents.Add(EQUIP, new EquipItemTut());
		nameEvents.Add(SELECT, new SelectTabTut());
		nameEvents.Add(SHOW_INFO, new ShowInfoTut());
		nameEvents.Add(BUY_ITEM, new BuyItemTut());
		nameEvents.Add (SET_ITEM, new SetItemTut ());
		nameEvents.Add (UPGRADE_ITEM, new UpgradeItemTut());
		nameEvents.Add (BACK_WORLDMAP, new GoToWorldMapTut());
	}

	public void Progress(GameObject go, int key)
	{
		if(nameEvents.ContainsKey(key))
		{
			ClickTutHandle handle = null;
			if(nameEvents.ContainsKey(key))
			{
				nameEvents.TryGetValue(key, out handle);
				handle.ProgressClick (go);
			}
		}
	}
}
