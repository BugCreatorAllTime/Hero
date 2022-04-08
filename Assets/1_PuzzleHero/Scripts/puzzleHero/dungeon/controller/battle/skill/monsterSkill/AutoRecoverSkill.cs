using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class AutoRecoverSkill : PoisonSkill
{
	protected override void OnActiveEffectFinish()
	{
		for (int i = 0; i < fxObjects.Count; i++) {
			SkeletonAnimation anim = fxObjects.ElementAt(i).Value.GetComponent<SkeletonAnimation>();
			effectsManager.ReturnSpineAnimationToPool(anim.state);
		}
		BuffsFlyToOwner();
	}

	private void BuffsFlyToOwner()
	{
		float longestDuration = 0;
		for (int i = 0; i < affectedGems.Count; i++) {
			Vector3 p0 = affectedGems[i].transform.localPosition;
			Vector3 p1 = EffectsManager.boardTopCenter;
			Vector3 p2 = EffectsManager.monsterCastPosition;
			Vector3[] path = new Vector3[] { p0, p1, p2 };
			float duration = effectsManager.CreateAutoRecoverBackFx(path);

//			matchLogic.routineRunner.StartCoroutine(AddBuff(duration));
			if (duration >= longestDuration) longestDuration = duration;
		}
		matchLogic.routineRunner.StartCoroutine(WaitThenEnd(longestDuration));
//		PlaySoundCastMonster ();
	}

//	protected override void PlaySoundCastMonster()
//	{
//		SoundManager.intance.PlaySound(SoundName.CAST_SKILL_RECOVERY);
//	}

	private IEnumerator WaitThenEnd(float delay)
	{
		yield return new WaitForSeconds(delay);
		//TODO Read constant from config instead of hard-coding
		float recoverPercent = float.Parse(skillInfo.extras.Split('/')[0]);
		float hpBuff = affectedGems.Count * (owner.getStat().GetMaxHp() * recoverPercent);
		owner.AddBuff(new HealBuff(hpBuff));
		GameObject fxGameObject = effectsManager.CreateAutoRecoverSkillEffect();
		RemoveCallbacks();
		Destroy();
//		PlaySoundRecovery ();
	}

//	protected override void PlaySoundRecovery()
//	{
//		SoundManager.intance.PlaySound(SoundName.RECOVERY_HP);
//	}
}