using System;
using System.Collections;
using System.Linq;
using Nfury.Base;
using strange.examples.strangerocks;
using strange.extensions.signal.impl;
using UnityEngine;
using System.Collections.Generic;

public class BattlePhaseManager
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleLogic { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public Team1FinishAttackSignal team1FinishAttackSignal { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public SkillsManager skillsManager { get; set; }

	[Inject]
	public TutorialFirstBattleLogic tutLogic { get; set;}

	[Inject]
	public TutorialStateSignal tutStateSignal { get; set;}

	[Inject]
	public Handler handler { get; set; }

	[Inject]
	public Team2FinishAttackSignal team2Finish { get; set;}

	public List<BaseSkill> addActionTempSkills = new List<BaseSkill>();

	public enum BattlePhase
	{
		Character, Monster, Neutral
	}

	public delegate void OnBattlePhaseChanged(BattlePhase phase);
	public event OnBattlePhaseChanged OnBattlePhaseChangedEvent;

	private Queue<BattleAction> actionsQueue;
	private Queue<BattleAction> monsterQueue;
	private Queue<BattleAction> neutralQueue;
	private List<BaseSkill> tempSkills;
	private BattleAction runAction;
	private BattleAction deadAction;
	private BattleAction delayAction;
	private BattleAction waitForNewEnemyAction;
	private BattlePhase phase;
	private bool isMatchFromUserInput;
	private bool isCharacterJustAttacked;
	private BattleEntity monster;

	public BattlePhase Phase
	{
		get { return this.phase; }
		set
		{
			this.phase = value;
			NotifyBattlePhaseChanged();
		}
	}

	[PostConstruct]
	public void Init()
	{
		//Logger.Trace("BattlePhaseManager postConstruct");
		actionsQueue = new Queue<BattleAction>();
		monsterQueue = new Queue<BattleAction>();
		neutralQueue = new Queue<BattleAction>();
		tempSkills = new List<BaseSkill>();
		routineRunner.StartCoroutine(CheckInput());
		phase = BattlePhase.Neutral;
		isMatchFromUserInput = false;
		isCharacterJustAttacked = false;
	}

	public void StartAttack(List<BaseSkill> skills, bool isMatchFromUserInput)
	{
		ClearTempSkill();
		BattleEntity team1 = ((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1);
		for (int i = 0; i < skills.Count; i++)
		{
			team1.AddSkill(skills[i]);
			AddTempSkill(skills[i]);
			if (skills[i] is DefaultSkill)
			{
				skills[i].damage += ((MatchLogic) matchLogic).GetBonusDamage();
				//Logger.Trace(i, "start attack bonus damage", ((MatchLogic)matchLogic).GetBonusDamage());
			}
		}
		this.isMatchFromUserInput = isMatchFromUserInput;
		isCharacterJustAttacked = true;
		addActionTempSkills.Clear();
		//Logger.Trace("character just atk");
	}

	public void AddTempSkill(BaseSkill skill)
	{
		tempSkills.Add(skill);
	}

	public void ClearTempSkill()
	{
		tempSkills.Clear();
	}

	private IEnumerator CheckInput()
	{
		while (true)
		{
			if (IsCharacterTurn())
			{
				if (IsBoardStatic())
				{
					if (IsSkillsCreated())
					{
						for (int i = 0; i < tempSkills.Count; i++)
						{
							BaseSkill skill = tempSkills[i];
							BattleEntity monster = ((BattleGameLogic) battleLogic).FindEnemy(BattleGameLogic.TEAM1);
							CharacterSkillAction action = new CharacterSkillAction(null, new object[] {skill, monster});
							skill.OnPhaseChangedEvent += new BaseSkill.OnPhaseChanged(action.OnActionFinished);
							AddAction(action);
						}
						ClearTempSkill();
					}
					//Logger.Trace("actionQueue count ", actionsQueue.Count);
					if (actionsQueue.Count > 0)
					{
						//Logger.Trace("actionsQueue count ", actionsQueue.Count);
						BattleAction action = actionsQueue.Peek();
						if (action.state == BattleAction.ActionState.Waiting)
						{
							effectsManager.EndChargeFx();
							action.Activate();
						}
						if (action.state == BattleAction.ActionState.Finished)
						{
							actionsQueue.Dequeue();
						}

						if (actionsQueue.Count == 0)
						{
							((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1).skills.Clear();
							((MatchLogic) matchLogic).DisableBoardInput();
							continue;
						}
					}
					else
					{
						//Logger.Trace("empty queue, unlock board");
						if (isCharacterJustAttacked)
						{
							if (isMatchFromUserInput)
							{
								//Logger.Trace("current turn is character, begin to switch turn");
								team1FinishAttackSignal.Dispatch();
								skillsManager.ElapseTurns(1);
								ElapseCharacterTurn();
							}
							isCharacterJustAttacked = false;
						}
						Phase = BattlePhase.Neutral;
					}
				}
			}
			if (IsNeutralTurn() && IsBoardStatic())
			{
				//Logger.Trace("actionQueue count ", actionsQueue.Count);
				if (neutralQueue.Count > 0)
				{
					//Logger.Trace("actionsQueue count ", actionsQueue.Count);
					BattleAction action = neutralQueue.Peek();
					if (action.state == BattleAction.ActionState.Waiting)
					{
						action.Activate();
					}
					if (action.state == BattleAction.ActionState.Finished)
					{
						neutralQueue.Dequeue();
					}
				}
				else
				{
					//Logger.Trace("empty queue, unlock board");
					Phase = BattlePhase.Monster;
				}
			}
			if (IsMonsterTurn() && IsBoardStatic())
			{
				//Logger.Trace("monsterQueue count ", monsterQueue.Count);
				if (monsterQueue.Count > 0)
				{
					//Logger.Trace("actionsQueue count ", actionsQueue.Count);
					BattleAction action = monsterQueue.Peek();
					if (action.state == BattleAction.ActionState.Waiting)
					{
						action.Activate();
					}
					//Logger.Trace("check", action.GetSignalObjects()[0].GetHashCode(), "state", action.state);
					if (action.state == BattleAction.ActionState.Finished)
					{
						//Logger.Trace("finished", action.GetSignalObjects()[0].GetHashCode(), "state", action.state);
						tutLogic.SetNameMonsterSkill(action.GetSignalObjects()[0].GetType());
						tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_FIRST_SKILL_MONSTER, tutLogic.GetSkill());
						monsterQueue.Dequeue();
					}

					if (monsterQueue.Count < 1)
					{
						if (monster == null) {
							monster = ((BattleGameLogic)battleLogic).FindEntity(BattleGameLogic.TEAM2);
						}
						team2Finish.Dispatch();
					}
				}
				else
				{
					//Logger.Trace("phase character");
					//Logger.Trace("empty queue, unlock board");
					Phase = BattlePhase.Character;
				}
			}
			if (IsBoardStatic())
			{
				if (IsAllQueuesEmpty() && IsCharacterTurn())
				{
					((MatchLogic) matchLogic).EnableBoardInput();
				}
				else
				{
					((MatchLogic) matchLogic).DisableBoardInput();
				}
			}
			//Logger.Trace("phase", phase, "static", IsBoardStatic());
			yield return new WaitForEndOfFrame();
		}
	}

	private void ElapseCharacterTurn()
	{
		BattleGameLogic battleGameLogic = (BattleGameLogic) battleLogic;
		BattleEntity character = battleGameLogic.FindEntity(BattleGameLogic.TEAM1);
		BattleEntity monster = battleGameLogic.FindEntity(BattleGameLogic.TEAM2);
		ElapseEntityBuffs(character);
		ElapseEntityBuffs(monster);
	}

	private void ElapseEntityBuffs(BattleEntity entity)
	{
		if (entity == null) return;
		Stack<int> indice = new Stack<int>();

		for (int i = 0; i < entity.buffs.Count; i++) {
			AbstractBuff buff = entity.buffs[i];
			if (buff is TurnBasedBuff) {
				TurnBasedBuff turnBasedBuff = (TurnBasedBuff)buff;
				turnBasedBuff.ElapseTurn();
				if (turnBasedBuff.DurationInTurn == 0) {
					indice.Push(i);
				}
			}
		}

		while (indice.Count > 0) {
			int index = indice.Pop();
			entity.buffs.RemoveAt(index);
		}
	}

	private bool IsMonsterTurn()
	{
		return phase == BattlePhase.Monster;
	}

	private bool IsCharacterTurn()
	{
		return phase == BattlePhase.Character;
	}

	private bool IsNeutralTurn()
	{
		return phase == BattlePhase.Neutral;
	}

	private bool IsBoardStatic()
	{
		return ((MatchLogic) matchLogic).IsBoardStatic();
	}

	private bool IsAllQueuesEmpty()
	{
		return actionsQueue.Count + monsterQueue.Count + neutralQueue.Count == 0;
	}

	private bool IsSkillsCreated()
	{
		return tempSkills.Count > 0;
	}

	public BattleAction AddCharacterAction()
	{
		return AddAction(actionsQueue);
	}

	public BattleAction AddNeutralAction()
	{
		return AddAction(neutralQueue);
	}

	public BattleAction AddMonsterAction()
	{
		return AddAction(monsterQueue);
	}

	public void AddMonsterAction(BattleAction action) {
		//Logger.Trace("add monster action");
		AddAction(monsterQueue, action);
	}

	private BattleAction AddAction(Queue<BattleAction> queue)
	{
		BattleAction action = new BattleAction(null, null);
		AddAction(queue, action);
		return action;
	}

	private void AddAction(Queue<BattleAction> queue, BattleAction action)
	{
		queue.Enqueue(action);
	}

	public void AddAction(BattleAction action)
	{
		actionsQueue.Enqueue(action);
	}

	public BattleAction AddAction()
	{
		BattleAction battleAction = new BattleAction(null, null);
		AddAction(battleAction);
		return battleAction;
	}

	public string QueueCount()
	{
		//Logger.Trace("action queue");
		string s = "action queue";
		for (int i = 0; i < actionsQueue.Count; i++)
		{
			if(actionsQueue.ElementAt(i).GetSignalObjects() == null)
				continue;
		//Logger.Trace(actionsQueue.ElementAt(i).GetSignalObjects()[0]);
			s += "\n" + actionsQueue.ElementAt(i).GetSignalObjects()[0];
		}
		//Logger.Trace("monster queue");
		s += "\nmonster queue";
		for (int i = 0; i < monsterQueue.Count; i++) {
			if (monsterQueue.ElementAt(i).GetSignalObjects() == null)
				continue;
			s += "\n" + (monsterQueue.ElementAt(i).GetSignalObjects()[0] + " "+ monsterQueue.ElementAt(i).GetSignalObjects()[0].GetHashCode());
		}
		s += "\n" + "neutral queue";
		for (int i = 0; i < neutralQueue.Count; i++) {
			if (neutralQueue.ElementAt(i).GetSignalObjects() == null)
				continue;
			s += "\n" + (neutralQueue.ElementAt(i).GetSignalObjects()[0]);
		}
		return actionsQueue.Count + "  " + monsterQueue.Count + "  " + neutralQueue.Count + "\n" + s;
	}

	public void StartRunning()
	{
		if (runAction == null)
		{
			runAction = new BattleAction(null, null);
		}
		runAction.state = BattleAction.ActionState.Waiting;
		neutralQueue.Enqueue(runAction);
	}

	public void StopRunning()
	{
		runAction.state = BattleAction.ActionState.Finished;
	}

	public void CharacterDie()
	{
		if (deadAction == null)
		{
			deadAction = new BattleAction(null, null);
		}
		deadAction.state = BattleAction.ActionState.Waiting;
		actionsQueue.Enqueue(deadAction);
	}

	public void CharacterRevive()
	{
		deadAction.state = BattleAction.ActionState.Finished;
	}

	public void MonsterDelayBeforeAttack()
	{
		delayAction = AddNeutralAction ();
	}

	public void MonsterAttack()
	{
		delayAction.state = BattleAction.ActionState.Finished;
	}

	public void WaitForNewEnemy()
	{
		if (waitForNewEnemyAction == null)
		{
			waitForNewEnemyAction=new BattleAction(null, null);
		}
		waitForNewEnemyAction.state = BattleAction.ActionState.Waiting;
		neutralQueue.Enqueue(waitForNewEnemyAction);
	}

	public void FaceNewEnemy()
	{
		if (waitForNewEnemyAction == null) return;
		waitForNewEnemyAction.state = BattleAction.ActionState.Finished;
	}

	private void NotifyBattlePhaseChanged()
	{
		if (OnBattlePhaseChangedEvent != null)
		{
			//Logger.Trace("notify battle phase ", phase);
			OnBattlePhaseChangedEvent(this.phase);
		}
	}
}

public class BattleAction
{
	public enum ActionState
	{
		Waiting,
		Processing,
		Finished
	}

	public ActionState state = ActionState.Waiting;
	public bool BeingActivated { get; set; }

	protected Signal signal;
	protected object[] signalObjects;

	public object[] GetSignalObjects()
	{
		return signalObjects;
	}

	public void SetSignalObjects(object[] objs)
	{
		signalObjects = objs;
	}

	public BattleAction(Signal signal, object[] signalObjects)
	{
		this.signal = signal;
		this.signalObjects = signalObjects;
	}

	public virtual void Activate()
	{
		state = ActionState.Processing;
		BeingActivated = true;
	}

	public void OnActionFinished(ActionPhase phase)
	{
		if (phase == ActionPhase.END)
		{
			state = ActionState.Finished;
		}
	}
}

public class CharacterSkillAction : BattleAction
{
	public CharacterSkillAction(Signal signal, object[] signalObjects) : base(signal, signalObjects)
	{
	}

	public override void Activate()
	{
		base.Activate();
		BaseSkill skill = signalObjects[0] as BaseSkill;
		BattleEntity monster = signalObjects[1] as BattleEntity;
		skill.Active(monster);
	}
}

public class MonsterSkillAction : BattleAction
{
	public MonsterSkillAction(Signal signal, object[] signalObjects) : base(signal, signalObjects)
	{
	}

	public override void Activate()
	{
		base.Activate();
		BaseSkill skill = signalObjects[0] as BaseSkill;
		BattleEntity character = signalObjects[1] as BattleEntity;
		skill.Active(character);
	}
}