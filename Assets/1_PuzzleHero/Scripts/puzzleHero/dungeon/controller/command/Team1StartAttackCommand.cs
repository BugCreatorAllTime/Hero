using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.command.impl;
using Nfury.Base;
using strange.examples.strangerocks;

public class Team1StartAttackCommand : Command
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }
	[Inject]
	public IRoutineRunner routineRunner{ get; set; }
	[Inject]
	public Team1FinishAttackSignal finish{ get; set; }
	[Inject]
	public SkillsManager skillsManager { get; set; }
	[Inject]
	public bool isMatchFromUserInput { get; set; }
	[Inject]
	public EnableBoardInputSignal enableBoardInputSignal { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	private BattleGameLogic battleLogic;
	private BattleEntity entity;
	public override void Execute ()
	{
		//Logger.Trace("team1 execute");
		battleLogic = abstractLogic as BattleGameLogic;
		entity = battleLogic.FindEntity (BattleGameLogic.TEAM1);
		battlePhaseManager.StartAttack(battlePhaseManager.addActionTempSkills, isMatchFromUserInput);
	}

	private void CastSkills()
	{
		//Logger.Trace("skill count ", entity.skills.Count);
		if (entity.skills.Count > 0)
		{
			for (int i = 0; i < entity.skills.Count; i++)
			{
				if (entity.skills[i].CanActive())
				{
					effectsManager.EndChargeFx();
					BaseSkill skill = entity.skills[i];
					skill.OnPhaseChangedEvent += new BaseSkill.OnPhaseChanged(OnSkillPhaseChanged);
					skill.Active(battleLogic.FindEnemy(BattleGameLogic.TEAM1));
					//Logger.Trace("castSkill");
					return;
				}
			}
		}
		//Logger.Trace("finish");
		finish.Dispatch();
		if (isMatchFromUserInput)
		{
			skillsManager.ElapseTurns(1);
		}
		entity.skills.Clear();

	}

	private void OnSkillPhaseChanged(ActionPhase phase)
	{
		if (phase == ActionPhase.END && entity.skills.Count > 0)
		{
			if (entity.skills.Count > 0)
			{
				CastSkills();
			}
		}
		if(entity.skills.Count==0) {
			if (isMatchFromUserInput) {
			}
		}
		
	}

	private IEnumerator Action ()
	{
		if (entity.skills.Count > 0) {
			foreach (BaseSkill skill in entity.skills) {
				skill.Active (battleLogic.FindEnemy (BattleGameLogic.TEAM1));
				yield return new WaitForSeconds (skill.duration + 3);
			}
			entity.skills.Clear ();
			if (isMatchFromUserInput)
			{
				finish.Dispatch ();
			}
		} else {
			yield return new WaitForSeconds (0);
			if (isMatchFromUserInput)
			{
				finish.Dispatch ();
			}
		}
		if (isMatchFromUserInput)
		{
			skillsManager.ElapseTurns (1);
		}
	}
}
