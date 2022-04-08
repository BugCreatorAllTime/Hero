using System.Collections.Generic;
using strange.extensions.pool.impl;
using UnityEngine;
using strange.extensions.pool.api;

public abstract class AbstractBoardSkill : Entity
{
	public BoardSkillInfo skillInfo;
	public MatchLogic matchLogic;
	public EffectsManager effectsManager;
	public SkillsManager skillsManager;
	//	public IPool<GameObject> pool;
//	public GameObject fxLayer;

//	protected List<GameObject> fx;
//	protected Dictionary<Spine.AnimationState, GameObject> fx;
	protected SkillState state;
	protected int numberOfTurnsToActivate;
	protected int numberOfTurnsToDeactivate;

	public AbstractBoardSkill()
	{
//		skillInfo = new BoardSkillInfo();
//		fx = new List<GameObject>();
//		fx = new Dictionary<Spine.AnimationState, GameObject>();
	}

	public void ElapseTurns(int numberOfTurns)
	{
//		Logger.Trace("AbstractBoardSkill elapseTurn");
		if (numberOfTurnsToActivate > 0)
		{
//			skillInfo.numberOfTurnsToActivate--;
			if (--numberOfTurnsToActivate == 0)
			{
				Activate();
			}
		}
		else if (numberOfTurnsToDeactivate > 0)
		{
//			skillInfo.numberOfTurnsToDeactivate--;
			if (--numberOfTurnsToDeactivate == 0)
			{
				Deactivate();
			}
		}
	}

	public virtual void Begin()
	{
		this.state = SkillState.Begin;
		this.numberOfTurnsToActivate = skillInfo.numberOfTurnsToActivate;
		this.numberOfTurnsToDeactivate = skillInfo.numberOfTurnsToDeactivate;
//		Logger.Trace("abstractBoardSkill begin");
	}

	public virtual void Activate()
	{
		if (state == SkillState.Active)
		{
			return;
		}
		this.state = SkillState.Active;
	}

	public virtual void Deactivate()
	{
		if (state == SkillState.Deactive)
		{
			return;
		}
		this.state = SkillState.Deactive;
	}
}