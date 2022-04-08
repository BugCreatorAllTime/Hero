using System.Collections.Generic;
using UnityEngine;

public class PreloadedAssets : DungeonAssetsRegister.IRegistration
{
	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public CrossContextData crossContextData { get; set; }

	private Dictionary<string, Object> cachedObjects = new Dictionary<string, Object>();

	private string monsterSkeleton;
	private string[] battleBgs;
	private string boardBgTexture;

	public void RegisterAsset(DungeonAssetsRegister register)
	{
		register.Register(CreateTeam1Command.simpleObjectPath, typeof (GameObject));
		register.Register(CreateTeam1Command.characterSkeletonPath, typeof(SkeletonDataAsset));

		monsterSkeleton = CreateTeam2Command.GetMonsterSkeletonPath(configManager, crossContextData);
		register.Register(monsterSkeleton, typeof(SkeletonDataAsset));

		battleBgs = BattleBackgroundLogic.GetBattleBackgroundPaths(configManager, crossContextData);
		for (int i = 0; i < battleBgs.Length; i++)
		{
			register.Register(battleBgs[i], typeof(Sprite));
		}

		register.Register(BoardBackgroundLogic.boardBg, typeof(GameObject));
		register.Register(BoardBackgroundLogic.boardBgMaterial, typeof(Material));
		BoardBgInfo boardBgInfo = CreateTeam2Command.GetBoardInfo(configManager, crossContextData);
		boardBgTexture = BoardBackgroundLogic.GetBgTexturePath(boardBgInfo);
		register.Register(boardBgTexture, typeof(Texture2D));
	}

	public void LoadAsset(AssetMgr assetMgr, DungeonAssetsRegister.OnAssetLoadedDelegate callback)
	{
		assetMgr.GetAsset<GameObject>(CreateTeam1Command.simpleObjectPath, o => CacheObject(CreateTeam1Command.simpleObjectPath, o, callback));
		assetMgr.GetAsset<SkeletonDataAsset>(CreateTeam1Command.characterSkeletonPath, o => CacheObject(CreateTeam1Command.characterSkeletonPath, o, callback));
		assetMgr.GetAsset<SkeletonDataAsset>(monsterSkeleton, o => CacheObject(monsterSkeleton, o, callback));
		string bgPath = battleBgs[0];
		assetMgr.GetAsset<Sprite>(bgPath, o => CacheObject(bgPath, o, callback));
		string bgPath2 = battleBgs[1];
		assetMgr.GetAsset<Sprite>(bgPath2, o => CacheObject(bgPath2, o, callback));
		string bgPath3 = battleBgs[2];
		assetMgr.GetAsset<Sprite>(bgPath3, o => CacheObject(bgPath3, o, callback));

		assetMgr.GetAsset<GameObject>(BoardBackgroundLogic.boardBg, o => CacheObject(BoardBackgroundLogic.boardBg, o, callback));
		assetMgr.GetAsset<Material>(BoardBackgroundLogic.boardBgMaterial, o => CacheObject(BoardBackgroundLogic.boardBgMaterial, o, callback));
		assetMgr.GetAsset<Texture2D>(boardBgTexture, o => CacheObject(boardBgTexture, o, callback));
	}

	public Object GetAsset(string path)
	{
		Object obj = null;
		cachedObjects.TryGetValue(path, out obj);
		return obj;
	}

	private void CacheObject(string key, Object objectToCache, DungeonAssetsRegister.OnAssetLoadedDelegate callback)
	{
		cachedObjects.Add(key, objectToCache);
		callback();
	}
}