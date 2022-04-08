using System.Collections;
using UnityEngine;

public class IceLockGemSkill : PoisonSkill
{
	protected override void AutoActivate()
	{
	}

	protected override void OnBeginEffectFinish()
	{
		if (matchLogic.FindHint().Count > 0)
		{
			end();
		}
		else
		{
			matchLogic.textFxManager.NoMoreMatches(delegate()
			{
				matchLogic.Counter(matchLogic.tiles);
				matchLogic.Shuffle();
				matchLogic.routineRunner.StartCoroutine(WaitThenEnd(Shuffle.totalShufflingTime));
			});
		}
	}

	private IEnumerator WaitThenCounter(float delay)
	{
		yield return new WaitForSeconds(delay);
		matchLogic.Counter(matchLogic.tiles);
		matchLogic.Shuffle();
	}

	private IEnumerator WaitThenEnd(float wait)
	{
		yield return new WaitForSeconds(wait);
		//Logger.Trace("end", GetHashCode());
		end();
		Destroy();
	}

	protected override void OnGemPicked(MatchItem gem)
	{
		base.OnGemPicked(gem);
		gem.SetLocked(true);
	}

	protected override void OnGemDropped(MatchItem gem) {
		base.OnGemDropped(gem);
		gem.SetLocked(false);
	}
}