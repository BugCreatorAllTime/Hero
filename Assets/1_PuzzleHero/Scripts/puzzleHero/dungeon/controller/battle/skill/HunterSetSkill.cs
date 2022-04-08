using System.Collections;
using UnityEngine;

public class HunterSetSkill : OneTimeSkill
{
	private GameObject fxGameObject;

	protected override void OnPlayCastFx(out GameObject go, out float duration)
	{
		go = effectsManager.CreateHunterSetSkillEffect();
		fxGameObject = go;
		SkeletonAnimation animation = go.GetComponent<SkeletonAnimation>();
		duration = animation.state.GetCurrent(0).Animation.Duration;
		matchLogic.routineRunner.StartCoroutine(PlayBeHitFx(duration - 0.25f));
	}

	private IEnumerator PlayBeHitFx(float delay)
	{
		yield return new WaitForSeconds(delay);
		effectsManager.CreateHunterSetSkillBeHitFx();
	}

	protected override void OnCastFxEnd() {
		base.OnCastFxEnd();
		effectsManager.ReturnSpineAnimationToPool(fxGameObject);
		string[] parms = extrasInfo.Split('/');
		float damagePercent = float.Parse(parms[0]);
		int damage = (int) (owner.getStat().GetDmg() * damagePercent);
		owner.DealDmgTo(battleGameLogic.FindEnemy(owner.Team), damage);
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.HUNTER_SET_SKILL);
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[1];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[2];
	}
}