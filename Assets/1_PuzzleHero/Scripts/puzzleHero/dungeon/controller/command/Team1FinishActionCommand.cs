using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;
using strange.examples.strangerocks;

public class Team1FinishActionCommand : Command
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }
	[Inject]
	public Team2StartAttackSignal team2Action{ get; set; }
	[Inject]
	public FinishBattleSignal finish{ get; set; }
	[Inject]
	public TutorialFirstBattleLogic tutLogic { get; set;}
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set;}

	private BattleEntity team2Entity;
	public override void Execute ()
	{
		team2Entity = (abstractLogic as BattleGameLogic).FindEntity (BattleGameLogic.TEAM2);
		if (team2Entity != null&&team2Entity.getStat().CurHp>0) {
			team2Entity.CurTurn--;
			tutLogic.AddTurn();
//			Logger.Trace("monster turn ", team2Entity.CurTurn);
			if(team2Entity.CurTurn == 0)
			{
				battlePhaseManager.MonsterDelayBeforeAttack();
				routineRunner.StartCoroutine(MonsterAttack(1f));
			}
		}
		finish.Dispatch();
	}

	private IEnumerator MonsterAttack(float duration)
	{
		yield return new WaitForSeconds(duration);
		team2Action.Dispatch ();
	}
}
