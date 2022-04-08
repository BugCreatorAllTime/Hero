using System;
using System.Collections.Generic;
using strange.examples.strangerocks;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;

public class AddActionCommand : Command
{
	[Inject]
	public Action action{ get; set; }
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }
	[Inject]
	public Team1PrepareAttackSignal Team1Prepare{ get; set; }

	[Inject]
	public BoardSkillSignal signal { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public SkillsManager skillsManager { get; set; }

	[Inject]
	public DungeonService dServer {get;set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public TextFxManager textManager { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	[Inject]
	public TutorialFirstBattleLogic tutLogic { get; set;}

	private BattleGameLogic battleLogic;
	private BattleEntity Team1Entity;

	public override void Execute ()
	{
		battleLogic = abstractLogic as BattleGameLogic;
		Team1Entity = battleLogic.FindEntity (BattleGameLogic.TEAM1);
		if (Team1Entity == null) return;
		if (Team1Entity.ObjState == ObjectState.Dead) return;
		bool skillJustAdded = false;
		List<BaseSkill> addActionTempSkills = battlePhaseManager.addActionTempSkills;
		switch (action.type)
		{
			case Data.TileTypes.Attack:
				DefaultSkill attack = null;
				for (int i = 0; i < addActionTempSkills.Count; i++)
				{
					if (addActionTempSkills[i] is DefaultSkill)
					{
						attack = (DefaultSkill)addActionTempSkills[i];
					}
				}
				if (attack == null)
				{
					skillJustAdded = true;
					attack = new DefaultSkill();
					SetupParams(attack);
					addActionTempSkills.Add(attack);
				}
				attack.damage += dServer.GetDamageFromMatch(action, Team1Entity.getStat().GetDmg());
				MatchLogic myMatchLogic = (MatchLogic) matchLogic;
				if (myMatchLogic.IsUserFirstMatch())
				{
					myMatchLogic.StoreFirstMatchDamage(attack.damage);
				}
				break;
			case Data.TileTypes.Defend:
				ArmorBuff armor = new ArmorBuff (dServer.GetArmorFromMatch(action, Team1Entity.getStat().GetMaxArmor()));
				Team1Entity.AddBuff (armor);
				break;
			case Data.TileTypes.Heal:
				HealBuff heal = new HealBuff (dServer.GetHpFromMatch(action, Team1Entity.getStat().GetMaxHp()));
				Team1Entity.AddBuff (heal);
				break;
			case Data.TileTypes.Skill:
				EnergyBuff energyBuff =
					new EnergyBuff(dServer.GetManaFromMatch(action, Team1Entity.getStat().baseStat.manaPool));
				Team1Entity.AddBuff(energyBuff);
				float currentMana = Team1Entity.getStat().CurrentMana + energyBuff.getValue();
				if (currentMana < Team1Entity.getStat().baseStat.manaPool || battleLogic.setItemSkill == null)
				{
					break;
				}
				ItemSetSkill skill = null;
				addActionTempSkills = battlePhaseManager.addActionTempSkills;
				for (int i = 0; i < addActionTempSkills.Count; i++)
				{
					if (addActionTempSkills[i] is ItemSetSkill)
					{
						skill = (ItemSetSkill)addActionTempSkills[i];
					}
				}
				if (skill != null)
				{
				}
				else
				{
					skillJustAdded = true;
					object obj = System.Activator.CreateInstance(Type.GetType(battleLogic.setItemSkill));
//					Logger.Trace(obj);
					skill = (ItemSetSkill) obj;
					SetupParams(skill);
					addActionTempSkills.Add(skill);
					skill.PlayChargeFx();
				}
				break;
			case Data.TileTypes.Gold:
				int gold = dServer.GetGoldFromMatch(action);
				dungeonState.CollectGold(gold);
				textManager.SetGold (gold, action.count);
				break;
		}
		if (addActionTempSkills.Count == 1 && skillJustAdded)
		{
			Team1Entity.ObjState = ObjectState.PreAttack;
			routineRunner.StartCoroutine(PlayChargeFx(Team1Entity.preAttackDuration));
		}
		if (action.type == Data.TileTypes.Attack) {
			effectsManager.GrowChargeFx();
		}
	}

	private IEnumerator PlayChargeFx(float delay)
	{
		yield return new WaitForSeconds(delay);
		effectsManager.PlayChargeFx();
	}

	private void SetupParams(BaseSkill skill)
	{
		skill.battleGameLogic = battleLogic as BattleGameLogic;
		skill.matchLogic = matchLogic as MatchLogic;
		skill.effectsManager = effectsManager;
		skill.skillsManager = skillsManager;
		if (skill is ItemSetSkill)
		{
			((ItemSetSkill)skill).extrasInfo = GetSkillExtras();
		}
	}

	private string GetSkillExtras()
	{
		UserData uData = configManager.UserData;
		if(configManager.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.START+1)
			uData = tutLogic.uData;
		if (!uData.IsActiveSet()) return null;
		ItemCfgImpl config = configManager.ItemCfg.GetItemByItemId(uData.EquippedItemData[0].Id);
		return configManager.itemSetSkillCfg.GetSkillExtrasById(config.SetSkillId);
	}
}
