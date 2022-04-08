using strange.examples.strangerocks;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;
using strange.extensions.pool.api;
using System.Collections.Generic;

public class CreateTeam1Command : Command
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleLogic { get; set; }

	[Inject]
	public AssetMgr assetMgr { get; set; }
	[Inject]
	public StartGameSignal startGameSignal { get; set; }
	[Inject]
	public CreateObjModel createObjModel { get; set; }
	[Inject]
	public ConfigManager configMgr { get; set; }

	[Inject(DungeonContext.DUNGEON)]
	public GameObject dunObj { get; set; }

	[Inject]
	public ItemService itemService { get; set; }

	[Inject]
	public CharacterHpChangedSignal characterHpChangedSignal { get; set; }

	[Inject]
	public CharacterDefendChangedSignal characterDefendChangedSignal { get; set; }

	[Inject]
	public CharacterSkillChangedSignal characterSkillChangedSignal { get; set; }

	[Inject]
	public bool isRevive { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public TextFxManager textManager { get; set; }

	[Inject]
	public TutorialFirstBattleLogic tutLogic {get;set;}

	[Inject]
	public SoundManager soundMng {get;set;}

	[Inject]
	public GA ga { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public DungeonStateDataSyncHandler dungeonStateDataSyncHandler { get; set; }

	[Inject]
	public PreloadedAssets preloadedAssets { get; set; }

	private ObjectState lastState;

	private string fifthName = "Animation/Fx/GradeFx/FifthGrade/FifthGrade";
	private string tenthName = "Animation/Fx/GradeFx/TenthGrade/TenthGrade";
	public const string simpleObjectPath = "Prefabs/Battle/SimpleObj";
	public const string characterSkeletonPath = "Animation/Character/Character.ske";
	private Dictionary<string, Object> cachedObjects = new Dictionary<string, Object>();

	public override void Execute()
	{
		GameObject simpleGameObject = (GameObject) preloadedAssets.GetAsset(simpleObjectPath);
		GameObject player = GameObject.Instantiate(simpleGameObject) as GameObject;
		player.transform.parent = dunObj.transform;

		CharacterData characterData = configMgr.CharacterCfg.character[configMgr.UserData.CharacterID.ToString()];
		player.name = characterData.Name;

		SkeletonAnimation skeletonAnimation = player.GetComponent<SkeletonAnimation>();
		skeletonAnimation.skeletonDataAsset = (SkeletonDataAsset)preloadedAssets.GetAsset(characterSkeletonPath);
		skeletonAnimation.skeletonDataAsset.Reset();
		skeletonAnimation.Reset();
		skeletonAnimation.skeleton.SetSkin(characterData.Name);
		skeletonAnimation.skeleton.SetSlotsToSetupPose();
		player.transform.position = new Vector2(-180, 105);
		HeroStat hStat = itemService.GetHeroStat(configMgr.UserData);
		BattleEntity entity = new BattleEntity(player);
		entity.Team = BattleGameLogic.TEAM1;
		entity.addStat(hStat);
		if (configMgr.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.START) {
			configMgr.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.BEGIN;
		}
		if (configMgr.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.BEGIN) {
			UserData mUser = configMgr.UserData.CloneBugUser();
			tutLogic.CloneItemTutorial(mUser);
			itemService.WearItemView(skeletonAnimation, mUser.EquippedItemData, configMgr.UserData.GetCharacterName(), mUser.IsActiveSet());
			hStat = itemService.GetHeroStat(mUser);
			entity.addStat(hStat);
			tutLogic.uData = mUser;
			entity.getStat().SubArmor(entity.getStat().GetMaxArmor() / 3);
			if (mUser.IsActiveSet()) {
				ItemCfgImpl config = configMgr.ItemCfg.GetItemByItemId(mUser.EquippedItemData[0].Id);
				BattleGameLogic bLogic = (BattleGameLogic)battleLogic;
				bLogic.setItemSkill = configMgr.itemSetSkillCfg.GetSkillNameById(config.SetSkillId);
			}
		}
		else {
			itemService.WearItemView(skeletonAnimation, configMgr.UserData.EquippedItemData, configMgr.UserData.GetCharacterName(), configMgr.UserData.IsActiveSet());
		}
		player.transform.position = new Vector2(-180, 105);
		entity.OnStatsChangedEvent += new BattleEntity.OnStatsChanged(OnCharacterStatsChanged);
		entity.OnStateChangedEvent += new BattleEntity.OnStateChanged(OnCharacterStateChanged);
		entity.OnStatsElementChangedEvent += new BattleEntity.OnStatsElementChanged(OnCharacterStatsElementChanged);
		battleLogic.AddObject(entity);
		if (configMgr.UserData.currentStepTownTutorial != TutorialFirstBattleLogic.BEGIN) {
			if (configMgr.UserData.IsActiveSet()) {
				ItemCfgImpl config = configMgr.ItemCfg.GetItemByItemId(configMgr.UserData.EquippedItemData[0].Id);
				BattleGameLogic bLogic = (BattleGameLogic)battleLogic;
				bLogic.setItemSkill = configMgr.itemSetSkillCfg.GetSkillNameById(config.SetSkillId);
			}
			else {
				((BattleGameLogic)battleLogic).setItemSkill = typeof(NonItemSetSkill).ToString();
			}
		}
		if (!isRevive) {
			createObjModel.createTeam1Complete = true;
			startGameSignal.Dispatch();
		}
		else {
			skeletonAnimation.state.SetAnimation(0, AnimationName.Resurrect, false);
			float time = skeletonAnimation.state.GetCurrent(0).Animation.Duration;
			routineRunner.StartCoroutine(WaitAndSetAnimationToIdle(time));
		}

		if (configMgr.UserData.restoreDungeonState) {
			dungeonStateDataSyncHandler.RestoreCharacter(entity);
		}
		characterSkillChangedSignal.Dispatch(entity.getStat().CurrentMana / (float)entity.getStat().baseStat.manaPool);
		characterHpChangedSignal.Dispatch(hStat.CurHp, hStat.GetMaxHp());
		characterDefendChangedSignal.Dispatch(hStat.CurArmor, hStat.GetMaxArmor());
	}

	private IEnumerator WaitAndSetAnimationToIdle(float timeToWait)
	{
		yield return new WaitForSeconds(timeToWait);
		((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1)
			.gObject.GetComponent<SkeletonAnimation>()
			.state.SetAnimation(0, AnimationName.Idle, true);
	}

	private void OnCharacterStatsChanged(HeroStat stat)
	{
		characterHpChangedSignal.Dispatch(stat.CurHp, stat.GetMaxHp());
		characterDefendChangedSignal.Dispatch(stat.CurArmor, stat.GetMaxArmor());
		characterSkillChangedSignal.Dispatch(stat.CurrentMana / (float)stat.baseStat.manaPool);
	}

	private void OnCharacterStatsElementChanged(int value, TextState textState, int team)
	{
		textManager.ShowText (value, textState, team);
//		if(textState == TextState.HpAdd)
//		{
//			soundMng.PlaySound(SoundName.CHARACTER_HEAL);
//		}
	}

	private void OnCharacterStateChanged(BattleEntity battleEntity)
	{
		switch(battleEntity.ObjState)
		{
			case ObjectState.Attack:
				soundMng.PlaySound(SoundName.CHARACTER_ATTACK);
				break;
			case ObjectState.BeHit:
				soundMng.PlaySound(SoundName.CHARACTER_BEHIT);
				break;
			case ObjectState.PreAttack:
				soundMng.PlaySound(SoundName.CHARACTER_CHARGE);
				break;
			case ObjectState.Cast:
				soundMng.PlaySound(SoundName.CHARACTER_SKILL);
				break;
			case ObjectState.Run:
//				soundMng.PlaySound(SoundName.CHARACTER_RUN);
				break;
			case ObjectState.Idle:
				if(lastState == ObjectState.Run)
				soundMng.PauseSound();
				break;
			case ObjectState.Dead:
				((MatchLogic)matchLogic).DisableBoardInput();
				battlePhaseManager.CharacterDie();
				ga.TrackFailureInDungeon();
				break;
			default:
				break;
		}
		lastState = battleEntity.ObjState;
	}
}