using System.Collections;
using Spine;
using UnityEngine;
using Animation = UnityEngine.Animation;
using AnimationState = UnityEngine.AnimationState;

public class SamuraiSetSkill : ItemSetSkill
{
	private GameObject fxGameObject;
	private float damagePercent;
	private float skillPercent;

	protected override void OnDestroy()
	{
	}

	protected override void OnAdd()
	{
		string[] parms = extrasInfo.Split('/');
		damagePercent = float.Parse(parms[0]);
		skillPercent = float.Parse(parms[1]);
	}

	protected override void ProcessLogicSkill()
	{
		owner.ObjState = ObjectState.PreAttack;
		fxGameObject = effectsManager.CreateSamuraiSetSkillEffect();
		matchLogic.routineRunner.StartCoroutine(WaitThenActivateLogic(0.5f));
//		matchLogic.routineRunner.StartCoroutine(WaitThenRestore(0.2f));
		owner.DoEmptyEnergy();
		SoundManager.intance.PlaySound (SoundName.SAMURAI_SET_SKILL);
	}

	private IEnumerator WaitThenRestore(float delay)
	{
		yield return new WaitForSeconds(delay);
		RestoreSkill();
	}

	private IEnumerator WaitThenActivateLogic(float delay)
	{
		yield return new WaitForSeconds(delay);
		owner.ObjState = ObjectState.Attack;
		matchLogic.routineRunner.StartCoroutine(WaitThenDealDmg(0.3f));
	}

	private IEnumerator WaitThenDealDmg(float delay)
	{
		yield return new WaitForSeconds(delay);
		DealDmg();

		RestoreSkill();
	}

	private void DealDmg() {
		
		int damage = (int)(owner.getStat().GetDmg() * damagePercent);
		owner.DealDmgTo(battleGameLogic.FindEnemy(owner.Team), damage);
		finishCast();
		end();
	}

	private void RestoreSkill()
	{
		float time = effectsManager.CreateSpreadFx();
		GameObject beHit = effectsManager.CreateSpineEffect(Effects.beHitCharacter, FxAnimationName.active, null, false, null, true);
		beHit.transform.localPosition = EffectsManager.team2BeHitPosition;
		matchLogic.routineRunner.StartCoroutine(AddMana(time));
	}

	private IEnumerator AddMana(float delay)
	{
		yield return new WaitForSeconds(delay);
		float mana = skillPercent * owner.getStat().baseStat.manaPool;
		owner.AddMana((int)mana);
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[2];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[3];
	}
}