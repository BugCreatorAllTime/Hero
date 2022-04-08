using System;
using System.Collections.Generic;
using strange.examples.strangerocks;
using Nfury.Base;
using UnityEngine;
using System.Collections;
using strange.extensions.pool.api;


public class BattleGameLogic : AbstractGameLogic
{
	[Inject]
	public WinNodeSignal winSignal { get; set; }

	[Inject]
	public LoseNodeSignal loseSignal { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public DungeonStateDataSyncHandler dungeonStateDataSyncHandler { get; set; }

	[Inject]
	public CharacterStartRunningSignal characterStartRunningSignal { get; set; }

	[Inject]
	public CharacterStopRunningSignal characterStopRunningSignal { get; set; }

	[Inject]
	public ConfigManager config {get;set;}

	[Inject]
	public TutorialFirstBattleLogic tutlogic {get;set;}

	[Inject]
	public TutorialStateSignal tutStateSignal { get; set;}

	public delegate void OnEnemySpotted(BattleEntity enemy);

	public event OnEnemySpotted OnEnemySpottedEvent;

	public string setItemSkill;

	public const int MAX_TEAM = 2;
	public const int TEAM1 = 0;
	public const int TEAM2 = 1;
	public float[] initPosX = { 100, 500 };
	public float[] initPosY = { 0, 0 };
	protected int curTurn = TEAM1;

	public int CurTurn
	{
		get
		{
			return curTurn;
		}
	}

	public BattleGameLogic()
	{
	}

	protected override void OnInit()
	{
		GameSpeed = 1.5f;
	}

	protected override void OnFixedUpdate(float dt)
	{

	}

	protected override void OnUpdate(float dt)
	{
	}

	public override void OnAddObject(Entity obj)
	{
	}


	public void OnProcessDealDmg(BattleEntity dealer, BattleEntity target, int realDmg, int absorbDmg)
	{

	}

	public override void OnRemoveObject(Entity obj)
	{
		BattleEntity be = obj as BattleEntity;
		if (be.Team == TEAM1)
		{
			Lose();
		}
		else
		{
			HeroStat stat = be.getStat();
			dungeonState.DefeatMonster(stat.factionId, stat.level);
			objList.Remove(be);
			Win();
		}
	}

	public void Win()
	{
		winSignal.Dispatch();
	}

	protected void Lose()
	{
		loseSignal.Dispatch ();
	}

	public IEnumerator RunToFindEnemy()
	{
		characterStartRunningSignal.Dispatch();

		int i = 0;
		FindEntity(BattleGameLogic.TEAM1).ObjState = ObjectState.Run;
		GameObject enemy = FindEntity (BattleGameLogic.TEAM2).gObject;
		while (FindEntity(BattleGameLogic.TEAM2).gObject.transform.position.x > 180)
		{
			tutlogic.FindToAlchemist();
			if(tutlogic.CheckRun(config.UserData))
				enemy.transform.position -= new Vector3(250*Time.deltaTime, 0, 0);
			yield return new WaitForEndOfFrame();
		}
		if (OnEnemySpottedEvent != null)
		{
			OnEnemySpottedEvent(FindEntity(BattleGameLogic.TEAM2));
		}
		while(tutlogic.CheckPreStartBattle(config.UserData))
		{
			tutlogic.StartSceneTwo();
			yield return new WaitForSeconds(6.5f);
			tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_SHOW, tutlogic.GetSkill());
		}
		StopToFindEnemy();
	}
	public void StopToFindEnemy()
	{
		characterStopRunningSignal.Dispatch();
		FindEntity(BattleGameLogic.TEAM1).ObjState = ObjectState.Idle;
	}
	public BattleEntity FindEnemy(int team)
	{
		for (int i = 0; i < objList.Count; ++i)
		{
			BattleEntity be = objList[i] as BattleEntity;
			if (be.Team != team)
			{
				return be;
			}
		}
		return null;
	}
	public BattleEntity FindEntity(int team)
	{
		for (int i = 0; i < objList.Count; ++i)
		{
			BattleEntity be = objList[i] as BattleEntity;
			if (be.Team == team)
			{
				return be;
			}
		}
		return null;
	}
}


