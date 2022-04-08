
using System;
using UnityEngine;

public class DefaultSkill : BaseSkill
{
	private const  float DELAY = 1f;


	private float countDelay = -1;
	private float delayFn = -1;
	private int enemyTeam = -1;

	public DefaultSkill ()
	{
	}

	protected override void OnAdd ()
	{
		if(this.damage == 0)
			this.damage = this.owner.getStat ().GetDmg ();
	}
	protected override void ProcessLogicSkill ()
	{
		this.owner.ObjState = ObjectState.Attack;
		countDelay = this.owner.halfOfAttackDur;
		delayFn = this.owner.attackDuration;
		enemyTeam = target.Team;
		PlaySlashEffect();
	}

	private void PlayBeHitEffect()
	{
		effectsManager.CreateBeHitEffect(enemyTeam);
	}

	private void PlaySlashEffect()
	{
		if (owner.Team == BattleGameLogic.TEAM1) {
			GameObject fxGameObject = effectsManager.CreateSpineEffect(Effects.slashVertically, FxAnimationName.active, "normal", false, null, true);
			fxGameObject.GetComponent<SkeletonAnimation>().timeScale = battleGameLogic.GameSpeed;
			fxGameObject.transform.localPosition = new Vector2(135, 590);
		}
	}

	protected override void OnFixedUpdate (float dt)
	{
		if (countDelay > 0) {
			countDelay -= dt;
			if (countDelay <= 0) {
				this.owner.DealDmgTo (target, (int)damage);
				PlayBeHitEffect();
			}
		}

		if(delayFn > 0)
		{
			delayFn -=dt;
			if(delayFn <= 0)
			{
				finishCast();
				end();
			}
		}
	}

	protected override void OnDestroy ()
	{

	}
}


