using System.Collections.Generic;
using UnityEngine;

public class AssassinSetSkill : ItemSetSkill
{
	private List<GameObject> fxGameObjects;
	private int attackCount;
	private float[] damageRange;
	private float[] durations;
	protected override void OnDestroy()
	{
	}

	protected override void OnAdd()
	{
		fxGameObjects = new List<GameObject>();
		string[] parms = extrasInfo.Split('/');
		attackCount = int.Parse(parms[0]);
		damageRange = new[] { float.Parse(parms[1]), float.Parse(parms[2]) };
	}

	protected override void ProcessLogicSkill()
	{
		if (attackCount < 1)
		{
			end();
			return;
		}

		PlayAnimation(0);
		durations = new float[attackCount];
		for (int i = 0; i < durations.Length; i++)
		{
			durations[i] = owner.attackDuration;
		}
		owner.DoEmptyEnergy();
		SoundManager.intance.PlaySound (SoundName.ASSASIN_SET_SKILL);
	}

	protected override void OnFixedUpdate(float dt)
	{
		if(durations == null) return;
		int index = 0;
		for (int i = 0; i < durations.Length; i++, index++)
		{
			//Logger.Trace("duration", durations[i]);
			if (durations[i] > 0)
			{
				durations[i] -= dt;
				if (durations[i] <= 0)
				{
					DealDmg();
					if (i < durations.Length - 1)
					{
						PlayAnimation(i+1);
					}
					if (i == durations.Length - 1)
					{
						OnAttackEnd();
					}
				}
				break;
			}
		}
		
	}

	private void PlayAnimation(int order)
	{
		//Logger.Trace("order", order);
		this.owner.ObjState = ObjectState.Attack;
		GameObject fx = effectsManager.CreateAssassinSetSkillEffect(order);
	}

	private void OnAttackEnd()
	{
		//Logger.Trace("end assassin skill");
		finishCast();
		end();
	}

	private void DealDmg()
	{
		float damagePercent = (float) UnityEngine.Random.Range((int) (damageRange[0] * 100), (int) (damageRange[1] * 100)) / 100f;
		//Logger.Trace("damagePercent", damagePercent);
		int damage = Mathf.CeilToInt(owner.getStat().GetDmg() * damagePercent);
		owner.DealDmgTo(battleGameLogic.FindEnemy(owner.Team), damage);
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[3];
	}


	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[4];
	}
}