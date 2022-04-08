using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MonsterSkill : BaseSkill
{
	protected BoardSkillInfo skillInfo;

	public int numberOfTurnsToActivate;
	protected int numberOfTurnsToDeactivate;

	public BoardSkillInfo SkillInfo
	{
		get { return skillInfo; }
		set { skillInfo = value;
			numberOfTurnsToActivate = value.numberOfTurnsToActivate;
			numberOfTurnsToDeactivate = value.numberOfTurnsToDeactivate;
		}
	}

	public void ElapseTurns(int numberOfTurns) {
		//Logger.Trace("MonsterSkill elapseTurn ", numberOfTurnsToActivate);
		if (numberOfTurnsToActivate > 0) {
			if (--numberOfTurnsToActivate == 0) {
				AutoActivate();
			}
		}
		else if (numberOfTurnsToDeactivate > 0) {
			if (--numberOfTurnsToDeactivate == 0) {
				AutoDeactivate();
			}
		}
	}

	protected virtual void AutoActivate(){}
	protected virtual void AutoDeactivate(){}
	protected virtual void PlaySoundActivate(){}
	protected virtual void PlaySoundDeactivate(){}
	protected virtual void PlaySoundCastMonster(){}
	protected virtual void PlaySoundCastSkill(){}
	protected virtual void PlaySoundRecovery(){}

	public virtual List<MatchItem> GetAffectedGems()
	{
		return null;
	}

	public virtual void SetAffectedGems(List<MatchItem> gems) {
	}

	public virtual void PlayFxOnGem(){}

	protected override void OnAdd ()
	{
	}

	protected override void ProcessLogicSkill ()
	{
		this.owner.ObjState = ObjectState.Cast;
		PlayMonsterSkillEff();
		this.duration = this.owner.attackDuration;
		//Logger.Trace("duration ", duration);
	}

	protected override void OnFixedUpdate (float dt)
	{
		if (this.duration > 0) {
			//Logger.Trace("monsterSkill duration > 0");
			this.duration -= dt;
			if (duration <= 0) {
				finishCast();
				OnCastFinished();
			}
		}
	}

	protected virtual void OnCastFinished()
	{
		//Logger.Trace("OnCastFinished");
	}

	protected override void OnDestroy ()
	{
	}

	private void PlayMonsterSkillEff ()
	{
		OnPlayMonsterSkillEff ();
	}

	protected virtual void OnPlayMonsterSkillEff ()
	{
		//this.owner.ObjState = ObjectState.Idle;
	}

	public BoardSkillInfo GetSkillInfo ()
	{
		return this.skillInfo;
	}

	protected virtual void RemoveSelf()
	{
		Reset();
		skillsManager.RemoveSkill(this);
	}

	protected virtual void Reset()
	{
		SkillInfo = skillInfo;
	}

	protected Data.TileTypes ParseAffectType(BoardSkillInfo.AffectType affectType)
	{
		return (Data.TileTypes) Enum.Parse(typeof (Data.TileTypes), affectType.ToString());
	}

	public virtual bool AreThereAnyAffectableGems()
	{
		return true;
	}

	public virtual void SetOwner(BattleEntity entity)
	{
		this.owner = entity;
		this.target = battleGameLogic.FindEnemy(owner.Team);
		owner.AddSkill(this);
		this.phase = ActionPhase.END;
	}
}

