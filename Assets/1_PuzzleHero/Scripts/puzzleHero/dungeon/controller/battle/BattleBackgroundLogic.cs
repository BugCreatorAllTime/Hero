using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleBackgroundLogic
{
	public const string backgroundPrefab = "Prefabs/General/Sprite";

	private const int width = 640;
	private List<GameObject> backgrounds;
	private GameObject map;
	private AssetMgr assetMgr;
	private GameObject bgPrefab;
	private PreloadedAssets preloadedAssets;

	public BattleBackgroundLogic(AssetMgr assetMgr, PreloadedAssets preloadedAssets)
	{
		backgrounds = new List<GameObject>();
		map = GameObject.Find("Map");
		this.assetMgr = assetMgr;
		this.preloadedAssets = preloadedAssets;
		bgPrefab = assetMgr.GetAssetSync<GameObject>(backgroundPrefab);
	}

	public void Update(DungeonConfImpl dungeonData, BattleBackgroundsCfg config)
	{
		BattleBackgrounds bgs = MatchData(dungeonData, config);
		int i = 0;
		foreach (string background in bgs.Backgrounds)
		{
			GameObject bg = GameObject.Instantiate(bgPrefab) as GameObject;
			bg.transform.parent = map.transform;
			bg.transform.localPosition = new Vector3(i * width, 0, 0);
			backgrounds.Add(bg);
			SpriteRenderer sprite = bg.GetComponent<SpriteRenderer>();
			sprite.sprite = (Sprite)preloadedAssets.GetAsset(background);
			i++;
		}
		Scroller scroller = map.GetComponent<Scroller>();
		scroller.Setup();
	}

	public static string[] GetBattleBackgroundPaths(ConfigManager configManager, CrossContextData crossContextData)
	{
		DungeonConfImpl dungeonData = configManager.DungeonCfg.getDungeon(crossContextData.dungeonId);
		return MatchData(dungeonData, configManager.battleBackgroundsCfg).Backgrounds.ToArray();
	}

	private static BattleBackgrounds MatchData(DungeonConfImpl dungeonData, BattleBackgroundsCfg config)
	{
		for (int i = 0; i < config.BattleBg.Count; i++)
		{
			KeyValuePair<string, BattleBackgrounds> pair = config.BattleBg.ElementAt(i);
			if (pair.Value.Id == dungeonData.BattleBgId)
			{
				return pair.Value;
			}
		}
		return null;
	}
}