using System;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween.Core;
using Nfury.Base;
using strange.extensions.pool.api;
using UnityEngine;

public class SkillsManager
{
	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject(Prefabs.fx)]
	public IPool<GameObject> pool { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	public delegate void OnSkillFxFinishPlaying(AbstractBoardSkill skill);
	public event OnSkillFxFinishPlaying OnSkillFxFinishPlayingEvent;

	protected GameObject fxLayer;

	private List<MonsterSkill> skills;

	public SkillsManager()
	{
		skills = new List<MonsterSkill>();
		fxLayer = GameObject.FindWithTag(ObjectTag.fxLayer);
	}

	public void AddSkill(MonsterSkill skill)
	{
		skills.Add(skill);
	}

	public List<MonsterSkill> GetMonsterSkills()
	{
		return this.skills;
	}

	public void RemoveSkill(MonsterSkill skill)
	{
		skills.Remove(skill);
	}

	public void ElapseTurns(int numberOfTurns)
	{
//		Logger.Trace("SkillManager elapseTurn");
		if(skills.Count < 1) return;

		for (int i = 0; i < skills.Count; i++)
		{
			skills[i].ElapseTurns(numberOfTurns);
		}
	}

	public void NotifyMonsterSkillFxFinishPlaying(AbstractBoardSkill skill)
	{
	}
}