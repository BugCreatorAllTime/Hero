using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;

public class Team2FinishAttackCommand : Command {

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }

	private BattleEntity team2Entity;

	public override void Execute ()
	{
		team2Entity = (abstractLogic as BattleGameLogic).FindEntity (BattleGameLogic.TEAM2);
		if (team2Entity != null&&team2Entity.getStat().CurHp>0) {
			if (team2Entity.CurTurn <= 0) {
				team2Entity.CurTurn = team2Entity.getStat ().baseStat.turn;
			}
		}
	}
}
