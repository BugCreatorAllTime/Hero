using System.Collections.Generic;
using UnityEngine;

public class AdventurerSetSkillHp : OneTimeSkill
{
	private GameObject fxGameObject;

	protected override void OnPlayCastFx(out GameObject go, out float duration)
	{
		go = effectsManager.CreateAdventurerSetSkillEffect();
		fxGameObject = go;
		duration = fxGameObject.GetComponent<SkeletonAnimation>().state.GetCurrent(0).Animation.Duration;
//		SoundManager.intance.PlaySound(SoundName.RECOVERY_HP);
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.ADVENTURE_SET_SKILL);
	}

	protected override void OnCastFxEnd()
	{
		base.OnCastFxEnd();
		string[] parms = extrasInfo.Split('/');
		float hpPercent = float.Parse(parms[0]);
		int recoveredHp = (int) (hpPercent * owner.getStat().GetMaxHp());
		owner.Heal(recoveredHp);
	}

	protected override string GetPreSkillTexture()
	{
		string[] parms = extrasInfo.Split('/');
		return parms[1];
	}

	protected override string GetPreSkillColorsString()
	{
		return extrasInfo.Split('/')[2];
	}
}