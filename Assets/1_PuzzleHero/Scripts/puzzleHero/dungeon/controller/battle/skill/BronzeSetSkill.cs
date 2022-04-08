using UnityEngine;
using AnimationState = Spine.AnimationState;

public class BronzeSetSkill : ItemSetSkill
{
	protected float delay;

	protected override void OnFixedUpdate(float dt)
	{
		base.OnFixedUpdate(dt);
		if (delay > 0)
		{
			delay -= dt;
			if (delay <= 0)
			{
				end();
			}
		}
	}

	protected override void OnDestroy()
	{
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		BattleEntity team2Entity = battleGameLogic.FindEntity(BattleGameLogic.TEAM2);
		SelfBuff();
	}

	protected override void OnAdd()
	{
	}

	protected virtual void SelfBuff()
	{
		if (extrasInfo != null)
		{
			string[] parms = extrasInfo.Split('/');
			int blockCount = int.Parse(parms[0]);
			owner.AddBuff(new BronzeSetBuff(int.MaxValue, effectsManager, blockCount));
			WaitThenEndSkill();
		}
	}

	protected virtual void WaitThenEndSkill()
	{
		delay = 1.5f;
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.BRONZE_SET_SKILL);
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[1];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[2];
	}
}