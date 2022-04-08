using System.Linq;
using UnityEngine;
using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using strange.examples.strangerocks;

public class ConfigManager
{
	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject] 
	public DataService dataService {get; set;}
	

	[Inject]
	public IRoutineRunner routinerunner { get; set; }

	public static readonly string[] preloadConfig = {	"Config/ItemCfg",
		"Config/MonsterSkillCfg",
		"Config/MonsterCfg",
		"Config/CharacterCfg",
		"Config/DungeonCfg",
		"Config/BoardCfg",
		"Config/Constant",
		"Config/MiniMapCfg",
		"Config/Shop",
		"Config/ChestCfg",
		"Config/UserData",
		"Config/BattleBackgrounds",
		"Config/TutorialCfg",
		"Config/MatchTutorial",
		"Config/GridTutorial",
		"Config/SoundCfg",
		"Config/TextCombo",
		"Config/Hint",
		"Config/SetBannerInfoCfg",
		"Config/ItemIndexCfg",
		"Config/ItemSetSkill"};

	public UserData UserData;

	public ItemCfg ItemCfg;
	public CharacterCfg CharacterCfg;
	public MonsterSkillCfg MonsterSkillCfg;
	public MonsterCfg MonsterCfg;
	public DungeonCfg DungeonCfg;
	public BoardCfg boardCfg;
	public MiniMapCfg miniMapCfg;
	public GlobalConfig general;
	public ChestCfg chestCfg;
	public BattleBackgroundsCfg battleBackgroundsCfg;
	public ShopCfg shopCfg;
	public ItemSetSkillCfg itemSetSkillCfg;
	public HomeTownTutorialCfg townTutorialCfg;
	public MatchTutorialCfg matchTutCfg;
	public GridTutorialCfg gridTutorialCfg;
	public SoundCfg soundCfg;
	public TextComboCfg comboCfg;
	public HintCfg hintCfg;
	public SetBannerItemCfg setBannerCfg;
	public TextCollectionCfg text;
	public ItemIndexCfg itemIndexCfg;

	[PostConstruct]
	public void PostConstruct()
	{
		for (int i = 0; i < preloadConfig.Length; ++i)
		{
			string name = preloadConfig[i];
			TextAsset asset = assetMgr.GetAssetSync<TextAsset>(name);
			if (name.Contains("ItemCfg")) {
				this.ReadItemConfig(asset.text);
			}
			else if (name.Contains("MonsterSkillCfg")) {
				this.ReadMonsterSkillCfg(asset.text);
			}
			else if (name.Contains("MonsterCfg")) {
				this.ReadMonsterConfig(asset.text);
			}
			else if (name.Contains("CharacterCfg")) {
				this.ReadCharacterConfig(asset.text);
			}
			else if (name.Contains("BoardCfg")) {
				this.ReadBoardConfig(asset.text);
			}
			else if (name.Contains("DungeonCfg")) {
				this.ReadDungeonConfig(asset.text);
			}
			else if (name.Contains("UserData")) {
				this.ReadUserDataFromServer(asset.text);
			}
			else if (name.Contains("Constant")) {
				this.ReadGeneralConfig(asset.text);
			}
			if (name.Contains("MiniMapCfg")) {
				this.ReadMiniMapConfig(asset.text);
			}
			else if (name.Contains("BattleBackgrounds")) {
				ReadBattleBackgroundsConfig(asset.text);
			}
			else if (name.Contains("Shop")) {
				ReadShopCfg(asset.text);
			}
			else if (name.Contains("ItemSetSkill")) {
				ReadItemSetSkillCfg(asset.text);
			}
			else if (name.Contains("ChestCfg")) {
				ReadChestCfg(asset.text);
			}
			else if (name.Contains("TutorialCfg")) {
				ReadTownTutorialConfig(asset.text);
			}
			else if (name.Contains("MatchTutorial")) {
				ReadMatchTutCfg(asset.text);
			}
			else if (name.Contains("GridTutorial")) {
				ReadGridTutCfg(asset.text);
			}
			else if (name.Contains("SoundCfg")) {
				ReadSoundCfg(asset.text);
			}
			else if (name.Contains("TextCombo")) {
				ReadComboCfg(asset.text);
			}
			else if (name.Contains("Hint")) {
				ReadHintCfg(asset.text);
			}
			else if (name.Contains("SetBannerInfoCfg")) {
				ReadSetBannerCfg(asset.text);
			}
			else if (name.Contains("ItemIndexCfg")) {
				ReadItemIndexCfg(asset.text);
			}
		}
		this.AddConfigToUserData();
		routinerunner.StartCoroutine(Update());
	}

	IEnumerator Update ()
	{
		while (true) {
			yield return new WaitForEndOfFrame ();
			UserData.Update(Time.deltaTime);
		}
	}

	void ReadChestCfg (string text)
	{
		chestCfg = JsonMapper.ToObject<ChestCfg>(text);
	}

	void ReadShopCfg(string text)
	{
		shopCfg = JsonMapper.ToObject<ShopCfg>(text);
	}

	void ReadGeneralConfig(string text)
	{
		ConfigManager configMgr = JsonMapper.ToObject<ConfigManager>(text);
		this.general = configMgr.general;
		this.text = configMgr.text;
	}

	private void ReadCharacterConfig(string txt)
	{
		this.CharacterCfg = JsonMapper.ToObject<CharacterCfg>(txt);
		foreach (KeyValuePair<string, CharacterData> pair in this.CharacterCfg.character)
		{
			pair.Value.Id = Int32.Parse(pair.Key.ToString());
		}
	}

	private void ReadDungeonConfig(string txt)
	{
		JsonData json = JsonMapper.ToObject(txt);
		this.DungeonCfg = JsonMapper.ToObject<DungeonCfg>(txt);
		foreach (KeyValuePair<string, DungeonConfImpl> pair in this.DungeonCfg.dungeon)
		{
			pair.Value.dungeonId = Int32.Parse(pair.Key);
			try{
				JsonData node = json[pair.Key];
				pair.Value.confDropItem = JsonMapper.ToObject<Dictionary<string, DungeonConfDropItemImpl>>(node.ToJson());
			}catch(Exception ex){
			}
		}

	}
	private void ReadMonsterConfig(string txt)
	{
		this.MonsterCfg = JsonMapper.ToObject<MonsterCfg>(txt);
		foreach (KeyValuePair<string, MonsterCfgImpl> pair in this.MonsterCfg.monster)
		{
			pair.Value.MonsterId = Int32.Parse(pair.Key.ToString());
		}
	}
	private void ReadMonsterSkillCfg(string txt)
	{
		this.MonsterSkillCfg = JsonMapper.ToObject<MonsterSkillCfg>(txt);
	}
	private void ReadBoardConfig(string text)
	{
		this.boardCfg = JsonMapper.ToObject<BoardCfg>(text);
		foreach (KeyValuePair<string, BoardBgInfo> pair in this.boardCfg.BoardBg)
		{
			pair.Value.Id = Int32.Parse(pair.Key.ToString());
		}
	}

	private void ReadItemConfig(string text)
	{
		this.ItemCfg = JsonMapper.ToObject<ItemCfg>(text);
		foreach (KeyValuePair<string, ItemCfgImpl> pair in this.ItemCfg.itemSet)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}

		foreach (KeyValuePair<string, ItemCfgImpl> pair in this.ItemCfg.itemNoneSet)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
	}

	private void ReadMiniMapConfig(string text)
	{
		this.miniMapCfg = JsonMapper.ToObject<MiniMapCfg>(text);
		foreach (KeyValuePair<string, MiniMapData> pair in this.miniMapCfg.minimap)
		{
			pair.Value.ID = Int32.Parse(pair.Key);
		}
	}

	private void ReadUserDataFromServer(string txt)
	{
		//this.UserData = JsonMapper.ToObject<UserData>(txt);
		this.UserData = dataService.Load<UserData>(DataService.USER_KEY);
		if(UserData == null)
		{
			UserData = JsonMapper.ToObject<UserData>(txt);
			UserData.SetConfig(this);
			UserData.GetFlag();
			UserData.Init();
			dataService.Save(DataService.USER_KEY,UserData);
//			UserData.InitTest();
		}else
		{
			UserData.SetConfig(this);
			UserData.GetFlag();
		}
//		UserData.curMapId = 38;
//		UserData.currentStepTownTutorial = 35;
	}

	private void AddConfigToUserData()
	{
		//this.UserData.Config = this;
		//this.UserData.Init();
//		this.UserData.InitTest();
	}

	private void ReadBattleBackgroundsConfig(string text)
	{
		battleBackgroundsCfg = JsonMapper.ToObject<BattleBackgroundsCfg>(text);
		for (int i = 0; i < battleBackgroundsCfg.BattleBg.Count; i++)
		{
			KeyValuePair<string, BattleBackgrounds> pair = battleBackgroundsCfg.BattleBg.ElementAt(i);
			pair.Value.Id = Int32.Parse(pair.Key);
		}
	}

	private void ReadItemSetSkillCfg(string text)
	{
		itemSetSkillCfg = JsonMapper.ToObject<ItemSetSkillCfg>(text);
		/*foreach (KeyValuePair<string, SetSkill> pair in itemSetSkillCfg.SetSkill)
		{
			Logger.Trace(pair.Value.Id, " ", pair.Value.SkillName);
		}*/
	}

	private void ReadTownTutorialConfig(string text)
	{
		this.townTutorialCfg = JsonMapper.ToObject<HomeTownTutorialCfg>(text);
		foreach (KeyValuePair<string, HomeTownTutorialData> pair in this.townTutorialCfg.homeTown)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
		
	}

	private void ReadMatchTutCfg(string text)
	{
		this.matchTutCfg = JsonMapper.ToObject<MatchTutorialCfg>(text);
		foreach (KeyValuePair<string, MatchTutorialData> pair in this.matchTutCfg.matchtutorial)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
		
	}

	private void ReadGridTutCfg(string text)
	{
		this.gridTutorialCfg = JsonMapper.ToObject<GridTutorialCfg>(text);
		foreach (KeyValuePair<string, GridTutorialData> pair in this.gridTutorialCfg.gridtutorial)
		{
			pair.Value.ID = Int32.Parse(pair.Key);
		}
		
	}

	private void ReadSoundCfg(string text)
	{
		this.soundCfg = JsonMapper.ToObject<SoundCfg>(text);
		foreach (KeyValuePair<string, SoundData> pair in this.soundCfg.sound)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
		
	}

	private void ReadComboCfg(string text)
	{
		this.comboCfg = JsonMapper.ToObject<TextComboCfg>(text);
		foreach (KeyValuePair<string, TextComboData> pair in this.comboCfg.combo)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
	}

	private void ReadHintCfg(string text)
	{
		this.hintCfg = JsonMapper.ToObject<HintCfg>(text);
		foreach (KeyValuePair<string, HintData> pair in this.hintCfg.hint)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
	}

	private void ReadSetBannerCfg(string text)
	{
		this.setBannerCfg = JsonMapper.ToObject<SetBannerItemCfg>(text);
		foreach (KeyValuePair<string, SetBannerInfoData> pair in this.setBannerCfg.set)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
	}

	private void ReadItemIndexCfg (string text)
	{
		this.itemIndexCfg = JsonMapper.ToObject<ItemIndexCfg>(text);
		foreach (KeyValuePair<string, ItemIndexData> pair in this.itemIndexCfg.WeaponIndex)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}

		foreach (KeyValuePair<string, ItemIndexData> pair in this.itemIndexCfg.ArmorIndex)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}

		foreach (KeyValuePair<string, ItemIndexData> pair in this.itemIndexCfg.ShieldIndex)
		{
			pair.Value.Id = Int32.Parse(pair.Key);
		}
		

	}
}
