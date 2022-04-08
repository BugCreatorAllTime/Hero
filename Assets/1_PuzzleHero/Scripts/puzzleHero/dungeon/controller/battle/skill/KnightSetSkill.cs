using Spine;
using UnityEngine;
using AnimationState = Spine.AnimationState;
using Animation = Spine.Animation;

public class KnightSetSkill : OneTimeSkill {
	private GameObject fxGameObject;

	protected override void OnPlayCastFx(out GameObject go, out float duration)
	{
		go = effectsManager.CreateKnightSetSkillEffect();
		fxGameObject = go;
		SkeletonAnimation animation = go.GetComponent<SkeletonAnimation>();
		SkeletonData skeletonData = animation.skeletonDataAsset.GetSkeletonData(true);
		Animation begin = skeletonData.FindAnimation(FxAnimationName.begin);
		Animation end = skeletonData.FindAnimation(FxAnimationName.end);
		duration = begin.Duration + end.Duration;
		//Logger.Trace("duration", begin.Duration, end.Duration);

		animation.state.End += OnAnimationEnd;
	}

	protected override void OnCastFxEnd() {
		base.OnCastFxEnd();
		effectsManager.ReturnSpineAnimationToPool(fxGameObject);
	}

	private void OnAnimationEnd(AnimationState state, int trackIndex) {
		//Logger.Trace("anim name", state.GetCurrent(trackIndex).Animation.Name);
		if (state.GetCurrent(trackIndex).Animation.Name.Equals(FxAnimationName.begin))
		{
			DealDmg();
		}
	}

	private void DealDmg()
	{
		string[] parms = extrasInfo.Split('/');
		float damagePercent = float.Parse(parms[0]);
		int damage = (int)(owner.getStat().GetDmg() * damagePercent);
		owner.DealDmgTo(battleGameLogic.FindEnemy(owner.Team), damage);
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.KNIGHT_SET_SKILL);
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[1];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[2];
	}
}