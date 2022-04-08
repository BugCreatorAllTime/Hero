using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;
using strange.examples.strangerocks;
using System.Collections.Generic;

public class Team2StartAttackCommand : Command
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }
	[Inject]
	public FinishBattleSignal finish{ get; set; }
	[Inject]
	public IRoutineRunner routineRunner{ get; set; }
	[Inject]
	public BoardSkillSignal boardSkillSignal{ get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public CrossContextData crossContextData { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public SkillsManager skillsManager { get; set; }

	private BattleGameLogic battleLogic;
	private BattleEntity team2Entity;
	private int totalrate;
	private BaseSkill defaultskill;
	public override void Execute ()
	{
		battlePhaseManager.MonsterAttack ();
		battleLogic = abstractLogic as BattleGameLogic;
		team2Entity = battleLogic.FindEntity (BattleGameLogic.TEAM2);
		routineRunner.StartCoroutine (Action ());
	}
	private IEnumerator Action ()
	{
		BaseSkill skill = CheckRandomSkill ();
		//Logger.Trace("skill ", skill);
		if (skill is MonsterSkill)
		{
			if (!((MonsterSkill) skill).AreThereAnyAffectableGems())
			{
			}
		}
		if (skill != null && skill is MonsterSkill && ((MonsterSkill)skill).AreThereAnyAffectableGems())
		{
			team2Entity.AddSkill(skill);
		}
		team2Entity.AddSkill(CreateDefaultSkill(team2Entity));
		for (int i = 0; i < team2Entity.skills.Count; i++)
		{
			skill = team2Entity.skills[i];
			if (skill.Phase != ActionPhase.READY) continue;
			MonsterSkillAction action = new MonsterSkillAction(null, new object[] { skill, battleLogic.FindEnemy(BattleGameLogic.TEAM2) });
			battlePhaseManager.AddMonsterAction(action);
			skill.OnPhaseChangedEvent += action.OnActionFinished;
		}

		yield return new WaitForSeconds (skill.duration);
	}

	private BaseSkill CheckRandomSkill ()
	{
		List<BaseSkill> monsterSkills = ReadMonsterSkillsFromConfig();
		foreach (BaseSkill skill in monsterSkills) {
			if (skill is DefaultSkill)
				defaultskill = skill;
			else
			{
				totalrate += skill.rate;
			}

		}
		List<BaseSkill> skills = new List<BaseSkill> ();
		int z = -1;
		if (totalrate <= 100) {
			foreach (BaseSkill skill in monsterSkills) {
				if (!(skill is DefaultSkill)) {
					for (int i =1; i<=skill.rate; i++) {
						skills.Add (skill);
					}
				}
			}
			for (int i=1; i<=100-totalrate; i++) {
				skills.Add (defaultskill);
			}
			z = UnityEngine.Random.Range (0, 100);
		} else {
			foreach (BaseSkill skill in monsterSkills) {
				if (!(skill is DefaultSkill)) {
					for (int i =1; i<=skill.rate; i++) {
						skills.Add (skill);
					}
				}
			}
			z = UnityEngine.Random.Range (0, totalrate);
			Debug.Log (skills [z]);
		}
		return skills[z];
	}

	private List<BaseSkill> ReadMonsterSkillsFromConfig()
	{
		DungeonConfImpl data = configManager.DungeonCfg.getDungeon(crossContextData.dungeonId);
		int idMonster = data.IdMonster[crossContextData.monsterIndexInDungeon];
		MonsterCfgImpl cfgData = configManager.MonsterCfg.GetMonsterCfgData(idMonster);

		List<BaseSkill> skills = new List<BaseSkill>();

		for (int i = 0; i <= cfgData.skill_id.Count - 1; i++) {
			int idSkill = cfgData.skill_id[i];
			int skillLv = cfgData.skill_lv[i];
			string skillCfgName = configManager.MonsterSkillCfg.getSkillCfg(idSkill).name;
			Type t = configManager.MonsterSkillCfg.GetType();
			FieldInfo f = t.GetField(skillCfgName);
			if (skillCfgName == "DefaultSkillCfg")
			{
				DefaultSkill skill = CreateDefaultSkill(team2Entity);
				skills.Add(skill);
			}
			else {
				Dictionary<string, BoardSkillInfo> cfg1 = (Dictionary<string, BoardSkillInfo>)f.GetValue(configManager.MonsterSkillCfg);
				Type type = Type.GetType(skillCfgName.Substring(0, skillCfgName.Length - 3));
				//				Logger.Trace("skill ", type);
				skills.Add(CreateMonsterSkill(type, cfg1, skillLv));
			}
		}

		return skills;
	}

	private DefaultSkill CreateDefaultSkill(BattleEntity owner)
	{
		DefaultSkill skill = new DefaultSkill();
		skill.effectsManager = effectsManager;
		skill.damage = owner.getStat().GetDmg();
		return skill;
	}

	private MonsterSkill CreateMonsterSkill(Type type, Dictionary<string, BoardSkillInfo> skillInfo, int skillLevel)
	{
		MonsterSkill skill1 = (MonsterSkill)Activator.CreateInstance(type);
		skill1.SkillInfo = skillInfo[skillLevel.ToString()];
		skill1.SkillInfo.skillType = type.ToString();
		skill1.battleGameLogic = abstractLogic as BattleGameLogic;
		skill1.matchLogic = matchLogic as MatchLogic;
		skill1.effectsManager = effectsManager;
		skill1.skillsManager = skillsManager;
		skill1.rate = skillInfo[skillLevel.ToString()].skillRate;
		return skill1;
	}
}

