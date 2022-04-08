
using System;
using UnityEngine;

public class NonItemSetSkill : ItemSetSkill
{
	private const  float DELAY = 1f;
	private float countDelay = -1;
	private const string slashSkin = "default skill";

	public NonItemSetSkill()
	{
	}

	protected override void OnAdd ()
	{
		if(this.damage == 0)
			this.damage = this.owner.getStat ().GetDmg () * 2;
	}

	protected override float PlayPreSkillFx()
	{
		return 0;
	}

	protected override void ProcessLogicSkill ()
	{
		this.owner.ObjState = ObjectState.Attack;
		countDelay = this.owner.attackDuration;
		PlaySlashEffect();
		owner.DoEmptyEnergy();
	}

	private void PlaySlashEffect()
	{
		if (owner.Team == BattleGameLogic.TEAM1) {
			GameObject fxGameObject = effectsManager.CreateSpineEffect(Effects.slashVertically, FxAnimationName.active, slashSkin, false, null, true);
			fxGameObject.transform.localPosition = new Vector2(135, 590);
		}
	}

	protected override void OnFixedUpdate (float dt)
	{
		if (countDelay > 0) {
			countDelay -= dt;
			if (countDelay <= 0) {
				this.owner.DealDmgTo (target, (int)damage);
				effectsManager.CreateBeHitEffect(target.Team);
				finishCast();
				end();
			}
		}
	}

	protected override void OnDestroy ()
	{

	}
}


