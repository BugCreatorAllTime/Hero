using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using Nfury.Base;

public class Team1PrepareAttackCommand :Command
{
	public const int TEAM = BattleGameLogic.TEAM1;
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic{ get; set; }
	[Inject]
	public Team1StartAttackSignal startAction{ get; set; }
	private BattleGameLogic battleLogic;
	private BattleEntity Team1Entity;
	private int value;
	public override void Execute ()
	{

	}
	

}
