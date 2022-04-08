using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;

[Serializable]
public class UserData : SubjectOs
{
	public string Name { get; set; }
	public string UID { get; set; }
	public int currentStepTownTutorial { get; set;} 
	public int CharacterID { get; set; }
	public List<ItemBaseData> EquippedItemData { get; set; }
	public ChestModel ChestModel { get; set; }
	public InventoryModel Inventory { get; set; }
	public int CurrentDungeonIdavailable { get; set; }

	private ConfigManager Config ;
	public int energy {get;set;}
	public int currentLevel {get;set;}
	public int curMapId {get;set;}
	public double levelPercent {get;set;}
	public IntegerWrapper gold {get;set;}
	public IntegerWrapper gem {get;set;}
	public IntegerWrapper live {get;set;}
	public long lastTimeUpdate {get;set;}
	public long lastTimeCheckNotifyFB {get;set;}
	public int gainedGem {get;set;}
	public int flagSave { get; set;}
	public int numberItemShop { get; set;}
	public double soundValue { get; set;}
	public double musicValue { get; set;}
	public bool RestoreDungeonState;

	private List<Observer> listObserver = new List<Observer>();

	public bool restoreDungeonState
	{
		get { return RestoreDungeonState; }
		set
		{
			//Logger.Trace(GetType(), "restoreDunState", value);
			RestoreDungeonState = value;
		}
	}
	public DungeonStateData dungeonStateData { get; set; }
	public bool canNotify = false;
	private int firstBringDown; //bit 1
	private int tutOpenChest; //bit 2
	private int tutPoison; //bit 3
	private int tutFire; //bit 4
	private int tutIce; //bit 5
	private int tutRock; //bit 6
	private int tutTransform; //bit 7
	private int tutDestroy; //bit 8
	private int tutShuffle; //bit 9
	private int tutHeal; //bit 10
	private int tutCollectGold; //bit 11
	private int popupUpdate; //bit 12
	private int iconUpdate; //bit 13
	private int notifyFB; //bit 14
	private int firstLoginFB; //bit 15
	private bool defeat = false;
	private bool itemUpdate;

	public void SetConfig(ConfigManager configMgr)
	{
		Config = configMgr;
	}

	public void Init()
	{
		CharacterData charData = Config.CharacterCfg.GetCharacterData(CharacterID);
		Inventory = new InventoryModel();
		Inventory.MaxSlot = charData.InitSlot;
		gold = charData.InitGold;
		gem = charData.InitGem;
		energy = Config.CharacterCfg.GetMaxEnergy();
		ItemBaseData item;
		for(int i = 0; i< charData.InitItems.Count; ++i)
		{
			ItemCfgImpl config = Config.ItemCfg.GetItemByItemId(charData.InitItems[i]);
			item = new ItemBaseData(config);
			item.LevelUpgrade = 0;
			Inventory.AddItem(item);
		}
		ChestModel = new ChestModel();
		ChestModel.chests = new List<ChestData>();
		ChestModel.openedChests = new List<int>();
		EquippedItemData = new List<ItemBaseData> ();
		lastTimeUpdate = 0;
		lastTimeCheckNotifyFB = 0;
		curMapId = 1;
		Name = "user" + UnityEngine.Random.Range(0,1000);

		lastTimeUpdate = Utils.GetCurrentTimeInSecond();
		lastTimeCheckNotifyFB = Utils.GetCurrentTimeInSecond ();
	}

	public void InitTest()
	{
		Inventory = new InventoryModel();
		Inventory.MaxSlot = 100;//Config.ItemCfg.itemSet.Count;
		ItemBaseData item;
		ItemCfgImpl itemCfg;
		foreach (KeyValuePair<string, ItemCfgImpl> pair in Config.ItemCfg.itemSet)
		{
			item = new ItemBaseData(pair.Value);
			item.LevelUpgrade = UnityEngine.Random.Range(0,11);
//			item.LevelUpgrade = 10;
			Inventory.AddItem(item);
		}

		foreach (KeyValuePair<string, ItemCfgImpl> pair in Config.ItemCfg.itemNoneSet)
		{
			item = new ItemBaseData(pair.Value);
//			item.LevelUpgrade = 0;
			item.LevelUpgrade = UnityEngine.Random.Range(0,11);
			item.LevelUpgrade = 5;
			Inventory.AddItem(item);
		}

		GainExp (600828);
		AddGold (1000);
		AddGem(100);

		ChestModel = new ChestModel();
		ChestModel.chests = new List<ChestData>();
		ChestModel.openedChests = new List<int>();
		foreach (KeyValuePair<string, ChestCfgImplement> pair in Config.chestCfg.chests)
		{
			ChestData data = new ChestData();
			data.chestId = int.Parse(pair.Key);
			ChestModel.AddChest(data, ChestSource.NONE_GEM);
			ChestModel.AddChest(data, ChestSource.FIRST_TIME);
			ChestModel.AddChest(data, ChestSource.FROM_GEM);
		}
		curMapId = 38;
		lastTimeUpdate = Utils.GetCurrentTimeInSecond();
		currentStepTownTutorial = 30;
	}

	public string GetCharacterName()
	{
		string characterName = "";
		int characterId = this.CharacterID;
		characterName = this.Config.CharacterCfg.GetCharacterData(characterId).Name;
		return characterName;
	}

	public bool IsActiveSet()
	{
		if (EquippedItemData.Count == 3)
		{
			bool isActiveSet = true;
			for (int i = 1; i < this.EquippedItemData.Count; i++)
			{
				isActiveSet = isActiveSet && EquippedItemData[i].SetId > 1 && EquippedItemData[i].SetId == EquippedItemData[i - 1].SetId;
			}
			return isActiveSet;
		}
		else
		{
			return false;
		}
	}

	public bool EquipItem(ItemBaseData item)
	{
		Notify ();
		if(Inventory.ListItemData.Contains(item))
		{
			ItemBaseData oldItem = null;
			for(int i = 0; i < this.EquippedItemData.Count; ++i)
			{
				if(item.GetSlot() == this.EquippedItemData[i].GetSlot())
				{
					oldItem = this.EquippedItemData[i];
					this.EquippedItemData.RemoveAt(i);
					break;
				}
			}
			
			Inventory.RemoveItem(item);
			EquippedItemData.Add(item);
			if(oldItem != null)
			{
				Inventory.AddItem(oldItem);
			}
			return true;
		}else{
			return false;
		}

	}

	public bool UnequipItem(ItemBaseData item)
	{
		Notify ();
		if(EquippedItemData.Contains(item))
		{
			for(int i = 0; i < this.EquippedItemData.Count; ++i)
			{
				if(item == this.EquippedItemData[i])
				{
					EquippedItemData.RemoveAt(i);
					Inventory.AddItem(item);
					return true;
				}
			}
			return false;
		}else
		{
			return false;
		}

	}

	public bool AddEnergy(int energyToAdd)
	{
		int maxEnergy = Config.CharacterCfg.GetMaxEnergy();
		if (energy + energyToAdd > maxEnergy)
		{
			energy = maxEnergy;
		}
		else
		{
			energy += energyToAdd;
		}
		return true;
	}

	public bool SubEnergy(int energyToSub)
	{
		if (energy - energyToSub < 0)
		{
			return false;
		}
		if (IsFullEnergy ()) 
		{
			lastTimeUpdate = Utils.GetCurrentTimeInSecond();
			canNotify = true;
		}
		energy -= energyToSub;
		return true;
	}

	public bool IsFullEnergy()
	{
		return energy == Config.CharacterCfg.GetMaxEnergy();
	}

	public bool GainExp(double expToGain)
	{
		int expStep = Config.CharacterCfg.GetExpSteps()[currentLevel];
		double currentExp = expStep * levelPercent;
		double totalExpAfterGain = currentExp + expToGain;
		if (totalExpAfterGain >= expStep)
		{
			int maxLevel = Config.CharacterCfg.GetExpSteps().Count - 1;
			if (currentLevel == maxLevel)
			{
				levelPercent = 1;
			}
			else
			{
				currentLevel++;
				levelPercent = 0;
				expToGain = expToGain - (expStep - currentExp);
				GainExp(expToGain);
				energy = Config.CharacterCfg.GetMaxEnergy();
			}
		}
		else
		{
			levelPercent = totalExpAfterGain / expStep;
		}
//		Logger.Trace("level ", currentLevel, " ", levelPercent);
		return true;
	}

	public bool AddGold(int goldToAdd)
	{
		Notify ();
		gold += goldToAdd;
		return true;
	}

	public bool SubGold(int goldToSub)
	{
		Notify ();
		if (gold - goldToSub < 0)
		{
			return false;
		}
		gold -= goldToSub;
		return true;
	}

	public bool AddGem(int gemToAdd)
	{
		Notify ();
		gem += gemToAdd;
		gainedGem += gemToAdd;
		return true;
	}

	public bool SubGem(int gemToSub)
	{
		Notify ();
		if (gem - gemToSub < 0)
		{
			return false;
		}
		gem -= gemToSub;
		return true;
	}

	public bool AddLive(int liveToAdd)
	{
		live += liveToAdd;
		return true;
	}

	public bool SubLive(int liveToSub)
	{
		if (live - liveToSub < 0)
		{
			return false;
		}
		live -= liveToSub;
		return true;
	}

	public int GetWeight()
	{
		return currentLevel * Config.general.WeightPerLevel 
			+ this.Config.CharacterCfg.GetCharacterData(CharacterID).Weight;
	}

	public UserData CloneBugUser()
	{
		UserData myUser = new UserData ();
		myUser.CharacterID = this.CharacterID;
		myUser.EquippedItemData = new List<ItemBaseData>();
		for(int i = 0; i < this.EquippedItemData.Count; i++)
		{
			ItemBaseData item = new ItemBaseData();
			myUser.EquippedItemData.Add(item);
			myUser.EquippedItemData[i].Id = this.EquippedItemData[i].Id;
		}
		myUser.Inventory = new InventoryModel();
		myUser.Inventory.ListItemData = new List<ItemBaseData>();
		for(int i = 0; i < this.Inventory.ListItemData.Count; i++)
		{
			ItemBaseData item = new ItemBaseData();
			myUser.Inventory.ListItemData.Add(item);
			myUser.Inventory.ListItemData[i].Id = this.Inventory.ListItemData[i].Id;
		}
		return myUser;
	}

	public void Update(float dt)
	{
		long curTime = Utils.GetCurrentTimeInSecond();
		while(curTime - lastTimeUpdate > Config.general.EnergyCooldown)
		{
			lastTimeUpdate += Config.general.EnergyCooldown;
			AddEnergy(1);
		}
		while(curTime - lastTimeCheckNotifyFB > Config.general.TimeCheckNotifyFB)
		{
			lastTimeCheckNotifyFB += Config.general.TimeCheckNotifyFB;
			lastTimeCheckNotifyFB = Utils.GetCurrentTimeInSecond ();
			SetNotifyFB(1);
		}
	}

	public void UnlockNextMap(int curMapId)
	{
		if(this.curMapId == curMapId)
			++this.curMapId;
	}

	public void SetUserName(string name)
	{
		Notify ();
		this.Name = name;
	}

	public void NextStep()
	{
		Notify ();
		this.currentStepTownTutorial++;
	}

	public int GetFirstBringDown()
	{
		return this.firstBringDown;
	}

	public void SetFirstBringDown(int firstBringDown)
	{
		this.firstBringDown = firstBringDown;
		SetFlag ();
	}

	public int GetTutOpenChest()
	{
		return this.tutOpenChest;
	}

	public void SetTutOpenChest(int tutOpenChest)
	{
		this.tutOpenChest = tutOpenChest;
		SetFlag ();
	}

	public int GetTutPoison()
	{
		return this.tutPoison;
	}
	
	public void SetTutPoison(int tutPoison)
	{
		this.tutPoison = tutPoison;
		SetFlag ();
	}

	public int GetTutFire()
	{
		return this.tutFire;
	}
	
	public void SetTutFire(int tutFire)
	{
		this.tutFire = tutFire;
		SetFlag ();
	}

	public int GetTutIce()
	{
		return this.tutIce;
	}

	public void SetTutIce(int tutIce)
	{
		this.tutIce = tutIce;
		SetFlag ();
	}

	public int GetTutRock()
	{
		return this.tutRock;
		SetFlag ();
	}

	public void SetTutRock(int tutRock)
	{
		this.tutRock = tutRock;
	}

	public int GetTutTransform()
	{
		return this.tutTransform;
	}

	public void SetTutTransform(int tutTransform)
	{
		this.tutTransform = tutTransform;
		SetFlag ();
	}

	public int GetTutDestroy()
	{
		return this.tutDestroy;
	}

	public void SetTutDestroy(int tutDestroy)
	{
		this.tutDestroy = tutDestroy;
		SetFlag ();
	}

	public int GetTutShuffle()
	{
		return this.tutShuffle;
	}

	public void SetTutShuffle(int tutShuffle)
	{
		this.tutShuffle = tutShuffle;
		SetFlag ();
	}

	public int GetTutHeal()
	{
		return this.tutHeal;
	}

	public void SetTutHeal(int tutHeal)
	{
		this.tutHeal = tutHeal;
		SetFlag ();
	}

	public int GetTutCollectGold()
	{
		return this.tutCollectGold;
	}

	public void SetTutCollectGold(int tutCollectGold)
	{
		this.tutCollectGold = tutCollectGold;
		SetFlag ();
	}

	public int GetPopupUpdate()
	{
		return this.popupUpdate;
	}

	public void SetPopupUpdate(int popupUpdate)
	{
		this.popupUpdate = popupUpdate;
		SetFlag ();
	}

	public int GetIconUpdate()
	{
		return this.iconUpdate;
	}
	
	public void SetIconUpdate(int iconUpdate)
	{
		this.iconUpdate = iconUpdate;
		SetFlag ();
	}

	public int GetNotifyFB()
	{
		return this.notifyFB;
	}
	
	public void SetNotifyFB(int notifyFB)
	{
		this.notifyFB = notifyFB;
		SetFlag ();
	}

	public int GetFirstLoginFB()
	{
		return this.firstLoginFB;
	}
	
	public void SetFirstLoginFB(int firstLoginFB)
	{
		this.firstLoginFB = firstLoginFB;
		SetFlag ();
	}

	private void SetFlag()
	{
		flagSave = firstBringDown + tutOpenChest*2 + tutPoison*4 + tutFire*8
			+ tutIce*16 + tutRock*32 + tutTransform*64 + tutDestroy*128 + tutShuffle*256 + tutHeal*512
			+ tutCollectGold*1024 + popupUpdate*2048 + iconUpdate*4096 + notifyFB*8192 + firstLoginFB * 16384;
	}

	public void GetFlag()
	{
		firstBringDown = Utils.GetFlag (flagSave, 1);
		tutOpenChest = Utils.GetFlag (flagSave, 2);
		tutPoison = Utils.GetFlag (flagSave, 3);
		tutFire = Utils.GetFlag (flagSave, 4);
		tutIce = Utils.GetFlag (flagSave, 5);
		tutRock = Utils.GetFlag (flagSave, 6);
		tutTransform = Utils.GetFlag (flagSave, 7);
		tutDestroy = Utils.GetFlag (flagSave, 8);
		tutShuffle = Utils.GetFlag (flagSave, 9);
		tutHeal = Utils.GetFlag (flagSave, 10);
		tutCollectGold = Utils.GetFlag (flagSave, 11);
		popupUpdate = Utils.GetFlag (flagSave, 12);
		iconUpdate = Utils.GetFlag (flagSave, 13);
		notifyFB = Utils.GetFlag (flagSave, 14);
		firstLoginFB = Utils.GetFlag (flagSave, 15);
	}

	public bool GetDefeat()
	{
		return this.defeat;
	}

	public void SetDefeat(bool defeat)
	{
		this.defeat = defeat;
	}

	public bool GetItemUpdate()
	{
		return this.itemUpdate;
	}
	
	public void SetItemUpdate(bool itemUpdate)
	{
		this.itemUpdate = itemUpdate;
	}

	public void AddObserver(Observer server)
	{
		bool isAdd = false;
		for(int i = 0; i < listObserver.Count; i++)
		{
			if(listObserver[i] == server)
			{
				isAdd = true;
				break;
			}
		}
		if(!isAdd)
			listObserver.Add (server);
	}

	public void RemoveObserver(Observer server)
	{
		listObserver.Remove (server);
	}

	public void Notify()
	{
		for(int i = 0; i < listObserver.Count; i++)
		{

			listObserver[i].Update();
		}
	}
}
