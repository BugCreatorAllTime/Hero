using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using Nfury.Base;

public class FinishBattleCommand : Command
{
	[Inject]
	public GameLoop gameLoop{ get; set; }

	[Inject]
	public EnableBoardInputSignal resumeBoardInputSignal { get; set; }

	private BattleGameLogic battleLogic ;
	private BattleEntity Team1Entity ;
	private BattleEntity Team2Entity;
	public override void Execute ()
	{
		battleLogic = gameLoop.battleLogic as BattleGameLogic;
		Team1Entity = battleLogic.FindEntity (BattleGameLogic.TEAM1);
		Team2Entity = battleLogic.FindEntity (BattleGameLogic.TEAM2);
		if (Team1Entity != null) {
			Team1Entity.skills.Clear ();
			resumeBoardInputSignal.Dispatch();
		}

	}
}
