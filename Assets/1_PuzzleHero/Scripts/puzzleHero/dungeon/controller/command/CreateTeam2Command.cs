using System.Linq;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;
using strange.extensions.pool.api;
using strange.examples.strangerocks;
using System;
using System.Collections.Generic;
using System.Reflection;


public class CreateTeam2Command : Command
{

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic { get; set; }
	[Inject]
	public AssetMgr assetMgr { get; set; }
	[Inject]
	public ConfigManager config { get; set; }
	[Inject]
	public CrossContextData CrossContextData { get; set; }
	[Inject]
	public StartGameSignal startGameSignal { get; set; }
	[Inject]
	public CreateObjModel createObjModel { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public int monsterID { get; set; }

	[Inject(DungeonContext.DUNGEON)]
	public GameObject dunObj { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }
	[Inject]
	public CrossContextData data { get; set; }

	[Inject]
	public MonsterHpChangedSignal monsterHpChangedSignal { get; set; }

	[Inject]
	public MonsterTurnChangedSignal monsterTurnChangedSignal { get; set; }

	[Inject] 
	public DungeonService dService {get; set;}

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public SkillsManager skillsManager { get; set; }

	[Inject(Prefabs.gem)]
	public IPool<GameObject> gemPool { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public ChestModeLogic chestModeLogic { get; set; }

	[Inject]
	public TextFxManager textManager { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public SoundManager soundMng {get;set;}

	[Inject]
	public GA ga { get; set; }

	[Inject]
	public DungeonStateDataSyncHandler dungeonStateDataSyncHandler { get; set; }

	[Inject]
	public PreloadedAssets preloadedAssets { get; set; }

	public const string simpleObjectPath = "Prefabs/Battle/SimpleObj";

	private MonsterCfgImpl cfgData;
	private ObjectState lastState;

	public override void Execute()
	{
		DungeonConfImpl data = config.DungeonCfg.getDungeon(CrossContextData.dungeonId);
		int idMonster = data.IdMonster[CrossContextData.monsterIndexInDungeon];
		cfgData = config.MonsterCfg.GetMonsterCfgData(idMonster);
		int lvMonster = data.dungeonId;

		if (cfgData.Type < 0)
		{
			chestModeLogic.Begin(cfgData);
		}
		else
		{
			ga.SetCurrentBattleMonsterId(idMonster);

			if (cfgData.Type == MonsterCfg.TYPE_BOSS) {
				Warning();
			}

			GameObject simpleGameObject = (GameObject)preloadedAssets.GetAsset(simpleObjectPath);
			GameObject monster = GameObject.Instantiate(simpleGameObject) as GameObject;
			string[] arrStr = cfgData.Ani.Split('/');
			SkeletonAnimation skeletonAnimation = monster.GetComponent<SkeletonAnimation>();
			UnityEngine.Object skeletonData = preloadedAssets.GetAsset(GetMonsterSkeletonPath(config, CrossContextData));
			if (skeletonData == null)
			{
				skeletonData = assetMgr.GetAssetSync<SkeletonDataAsset>(GetMonsterSkeletonPath(config, CrossContextData));
			}
			skeletonAnimation.skeletonDataAsset = (SkeletonDataAsset) skeletonData;
			skeletonAnimation.skeletonDataAsset.Reset();
			skeletonAnimation.Reset();
			if (arrStr.Length > 1) {
				skeletonAnimation.skeleton.SetSkin(arrStr[1]);
				skeletonAnimation.skeleton.SetSlotsToSetupPose();
			}
			skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
					
			monster.transform.parent = dunObj.transform;
			monster.name = cfgData.Name;
			if (config.UserData.currentStepTownTutorial >= 1)
				monster.transform.position = new Vector2(550, 105);
			else monster.transform.position = new Vector2(1500, 105);
					
			HeroStat hStat = new HeroStat();
			hStat.level = lvMonster;
			hStat.catId = cfgData.Type;
			hStat.factionId = cfgData.MonsterId;
			hStat.baseStat = dService.FormularMonsterStat(cfgData, lvMonster);
					
			BattleEntity entity = new BattleEntity(monster);
			entity.Team = BattleGameLogic.TEAM2;
			entity.addStat(hStat);
					
			abstractLogic.AddObject(entity);
			entity.OnStatsChangedEvent += new BattleEntity.OnStatsChanged(OnMonsterStatsChanged);
			entity.OnStateChangedEvent += new BattleEntity.OnStateChanged(OnMonsterStateChanged);
			entity.OnStatsElementChangedEvent += new BattleEntity.OnStatsElementChanged(OnMonsterStatsElementChanged);
			entity.OnTurnChangedEvent += new BattleEntity.OnStateChanged(OnMonsterTurnChanged);
					
			routineRunner.StartCoroutine((abstractLogic as BattleGameLogic).RunToFindEnemy());
					
			if (config.UserData.restoreDungeonState) {
				dungeonStateDataSyncHandler.RestoreMonster(entity);
			}
			monsterHpChangedSignal.Dispatch(entity.getStat().CurHp, entity.getStat().GetMaxHp());
			monsterTurnChangedSignal.Dispatch(entity.CurTurn);
		}
		LoadBoardAndCellBackgrounds();
		if (CrossContextData.monsterIndexInDungeon == 0 || config.UserData.restoreDungeonState)
		{
			new BattleBackgroundLogic(assetMgr, preloadedAssets).Update(config.DungeonCfg.getDungeon(CrossContextData.dungeonId),
				config.battleBackgroundsCfg);
			createObjModel.createTeam2Complete = true;
		}

		startGameSignal.Dispatch();
	}

	public static string GetMonsterSkeletonPath(ConfigManager configManager, CrossContextData crossContextData)
	{
		DungeonConfImpl data = configManager.DungeonCfg.getDungeon(crossContextData.dungeonId);
		int idMonster = data.IdMonster[crossContextData.monsterIndexInDungeon];
		MonsterCfgImpl cfgData = configManager.MonsterCfg.GetMonsterCfgData(idMonster);
		string[] arrStr = cfgData.Ani.Split('/');
		return "Animation/Monster/" + arrStr[0] + "/" + arrStr[0] + ".ske";
	}

	public static BoardBgInfo GetBoardInfo(ConfigManager config, CrossContextData crossContextData)
	{
		DungeonConfImpl dungeonData = config.DungeonCfg.getDungeon(crossContextData.dungeonId);
		for (int i = 0; i < config.boardCfg.BoardBg.Count; i++) {
			KeyValuePair<string, BoardBgInfo> pair = config.boardCfg.BoardBg.ElementAt(i);
			BoardBgInfo boardInfo = pair.Value;
			if ((int)boardInfo.Id == dungeonData.BoardBgId) {
				return boardInfo;
			}
		}
		return null;
	}

	private void OnMonsterStatsChanged(HeroStat stat)
	{
		monsterHpChangedSignal.Dispatch(stat.CurHp, stat.GetMaxHp());
	}

	private void OnMonsterTurnChanged(BattleEntity entity)
	{
		monsterTurnChangedSignal.Dispatch(entity.CurTurn);
	}

	private void OnMonsterStateChanged(BattleEntity battleEntity)
	{
//		if(battleEntity.ObjState == ObjectState.BeHit && battleEntity.ObjState != lastState)
//		{
//			soundMng.PlaySound(cfgData.IdSoundBehit);
//		}
		if (battleEntity.ObjState == ObjectState.Dead)
		{
//			soundMng.PlaySound(cfgData.IdSoundKilled);
			battlePhaseManager.WaitForNewEnemy();
		}
		lastState = battleEntity.ObjState;
	}

	private void LoadBoardAndCellBackgrounds()
	{
		((MatchLogic)matchLogic).boardInfo = GetBoardInfo(config, CrossContextData);
	}

	private void OnMonsterStatsElementChanged(int value, TextState textState, int team)
	{
		textManager.ShowText (value, textState, team);
	}

	private void Warning()
	{
		if(config.UserData.currentStepTownTutorial > 0)
		{
			textManager.Waring ();
			soundMng.PlaySound(SoundName.BOSS_WARNING);
		}
	}
}
