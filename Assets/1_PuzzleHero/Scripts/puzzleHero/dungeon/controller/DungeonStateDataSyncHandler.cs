using System.Collections.Generic;
using Nfury.Base;
using UnityEngine;

public class DungeonStateDataSyncHandler
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleGameLogic { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public CrossContextData crossContextData { get; set; }

	[Inject]
	public DataService dataService { get; set; }

	[Inject]
	public ApplicationDispatcherService applicationDispatcherService { get; set; }

	[Inject]
	public SkillsManager skillsManager { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public TutorialFirstBattleLogic tutorialFirstBattleLogic { get; set; }

	private int dungeonSceneIndex = -1;
	private bool pauseStatus = false;

	[PostConstruct]
	public void PostConstruct()
	{
		//Logger.Trace(GetType(), "::PostConstruct()");
		dataService.PreSavingEvent += OnPause;
		dataService.PreQuitingEvent += OnQuit;
		applicationDispatcherService.dispatcher.OnLevelWasLoadedHandler += OnLevelWasLoaded;
		dungeonSceneIndex = FindDungeonSceneIndex();
	}

	public void Unsync()
	{
		//Logger.Trace(GetType(), "::Unsync");
		configManager.UserData.dungeonStateData = null;
		configManager.UserData.restoreDungeonState = false;
	}

	public void Sync()
	{
		//Logger.Trace(GetType(), "::Sync()");
		if (configManager.UserData.dungeonStateData == null)
		{
			configManager.UserData.dungeonStateData = new DungeonStateData();
		}
		BattleGameLogic battleLogic = ((BattleGameLogic) battleGameLogic);
		DungeonStateData data = configManager.UserData.dungeonStateData;
		data.characterState.stat = new SimplifiedStat(battleLogic.FindEntity(BattleGameLogic.TEAM1).getStat());
		data.monsterState.stat = new SimplifiedStat(battleLogic.FindEntity(BattleGameLogic.TEAM2).getStat());
		data.monsterState.skills = MonsterState.ConvertMonsterSkills(skillsManager.GetMonsterSkills());
		data.monsterState.curTurn = battleLogic.FindEntity(BattleGameLogic.TEAM2).CurTurn;
		data.boardState = new BoardState((MatchLogic) matchLogic);
		data.battleState = new BattleState(dungeonState, crossContextData);
		configManager.UserData.restoreDungeonState = true;
	}

	private int FindDungeonSceneIndex() {
		/*int index = 0;
		foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes) {
			if (S.enabled) {
				string name = S.path.Substring(S.path.LastIndexOf('/') + 1);
				name = name.Substring(0, name.Length - 6);
				if (name.Equals("Dungeon")) return index;
				index++;
			}
		}*/
		//TODO fix hard-coding
		return 2;
	}

	private void OnLevelWasLoaded(int level) {
		//Logger.Trace("level", level, "name", Application.loadedLevelName);
		if (!Application.loadedLevelName.Equals("Dungeon")) {
			//Logger.Trace("do not save");
			configManager.UserData.restoreDungeonState = false;
			Unsubscribe();
		}
	}

	private void Unsubscribe()
	{
		dataService.PreSavingEvent -= OnPause;
		applicationDispatcherService.dispatcher.OnLevelWasLoadedHandler -= OnLevelWasLoaded;
		dataService.PreQuitingEvent -= OnQuit;
	}

	private void OnPause(bool status)
	{
		this.pauseStatus = status;
		//Logger.Trace("pause status", status);
		if (!IsSynchronizable())
		{
			configManager.UserData.restoreDungeonState = false;
			return;
		}
		//Logger.Trace("app level name", Application.loadedLevelName);
		Sync();
	}

	private bool IsSynchronizable()
	{
		bool syncable = true;
		if (!pauseStatus) {
			//Logger.Trace("application is already focused, no need to sync data");
			pauseStatus = false;
			syncable = false;
		}
		if (dungeonState.state != DungeonState.State.Playing) {
			//Logger.Trace("dungeon state is not Playing, no need to sync data");
			syncable = false;
		}
		if (tutorialFirstBattleLogic.CheckTutorialProgress())
		{
			syncable = false;
		}
		//Logger.Trace("sync able", syncable);
		return syncable;
	}

	private void OnQuit()
	{
		//Logger.Trace(GetType(), "::OnQuit");
		if (!Application.loadedLevelName.Equals("Dungeon"))
		{
			configManager.UserData.restoreDungeonState = true;
		}
	}

	public void RestoreCharacter(BattleEntity character)
	{
		//Logger.Trace(GetType(), "::RestoreCharacter");
		configManager.UserData.dungeonStateData.characterState.ToCharacterEntity(character);
	}

	public void RestoreMonster(BattleEntity monster)
	{
		//Logger.Trace(GetType(), "::RestoreMonster");
		//RestoreCharacter(monster);
		MonsterState mState = configManager.UserData.dungeonStateData.monsterState;
		mState.ToMonsterEntity(monster);
	}

	public void RestoreMonsterSkills()
	{
		//Logger.Trace(GetType(), "::RestoreMonsterSkills");
		MonsterState mState = configManager.UserData.dungeonStateData.monsterState;
		/*List<SimplifiedMonsterSkill> skills = configManager.UserData.dungeonStateData.monsterState.skills;
		for (int i = 0; i < skills.Count; i++)
		{
			Logger.Trace(skills[i].ToString());
		}*/
		List<MonsterSkill> mSkills = mState.ToMonsterSkills((BattleGameLogic)battleGameLogic, (MatchLogic)matchLogic,
			effectsManager, skillsManager);
	}

	public void RestoreCrossContextData(CrossContextData data)
	{
		//Logger.Trace(GetType(), "::RestoreCrossContextData");
		configManager.UserData.dungeonStateData.battleState.ToCrossContextData(data);
	}

	public void RestoreDungeonState(DungeonState dunState)
	{
		//Logger.Trace(GetType(), "::RestoreDungeonState");
		configManager.UserData.dungeonStateData.battleState.ToDungeonState(dunState);
	}

	public void RestoreBoard(MatchLogic matchLogic)
	{
		//Logger.Trace(GetType(), "::RestoreBoard");
		configManager.UserData.dungeonStateData.boardState.ToBoard(matchLogic);
	}
}