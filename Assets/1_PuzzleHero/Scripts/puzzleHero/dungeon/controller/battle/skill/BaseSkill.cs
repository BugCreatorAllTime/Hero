using UnityEngine;
using System;
using Nfury.Base;

public abstract class BaseSkill
{
	public int damage;
	public float duration;
	public int rate;
	public BattleGameLogic battleGameLogic;
	public MatchLogic matchLogic;
	public EffectsManager effectsManager;
	public SkillsManager skillsManager;

	public delegate void OnPhaseChanged(ActionPhase phase);
	public event OnPhaseChanged OnPhaseChangedEvent;

	protected BattleEntity owner;
	protected BattleEntity target;
	protected ActionPhase phase;

	public ActionPhase Phase {
		get {
			return phase;
		}
		set {
			phase = value;
			NotifyPhaseChanged(phase);
		}
	}

	public BaseSkill ()
	{
	}

	public void BeAdded (BattleEntity owner)
	{
		this.owner = owner;
		Phase = ActionPhase.READY;
		OnAdd ();
	}

	public void FixedUpdate (float dt)
	{
		OnFixedUpdate (dt);
	}

	public void Destroy ()
	{
		owner = null;
		target = null;
		OnDestroy ();
	}

	protected abstract void OnFixedUpdate (float dt);
	protected abstract void OnDestroy ();
	protected abstract void ProcessLogicSkill ();
	protected abstract void OnAdd ();

	public bool CanActive ()
	{
		return phase == ActionPhase.READY;
	}

	public virtual void Active (BattleEntity target)
	{
		Phase = ActionPhase.CASTING;
		this.target = target;
		ProcessLogicSkill ();
	}

	public virtual void Active()
	{
		Phase = ActionPhase.CASTING;
		ProcessLogicSkill ();
	}
	public void ChangeTurn ()
	{
		Phase = ActionPhase.READY;
	}

	protected void finishCast ()
	{
		owner.ObjState = ObjectState.Idle;
		Phase = ActionPhase.CAST_FINISH;
	}

	protected void end()
	{
		Phase = ActionPhase.END;
	}

	private void NotifyPhaseChanged(ActionPhase phase)
	{
		if (OnPhaseChangedEvent != null) {
			OnPhaseChangedEvent(phase);
			//Logger.Trace("skill ", this, " phase ", phase);
		}
	}
}


