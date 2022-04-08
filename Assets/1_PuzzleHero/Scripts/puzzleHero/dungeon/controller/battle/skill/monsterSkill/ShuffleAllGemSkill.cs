using UnityEngine;
using System.Collections;

public class ShuffleAllGemSkill : MonsterSkill
{
	private float shuffleAllWaitTime = 0;

	protected override void OnPlayMonsterSkillEff()
	{
		base.OnPlayMonsterSkillEff();
	}

	protected override void OnCastFinished()
	{
		matchLogic.Shuffle();
		WaitAndEndSkill();
	}

	protected override void OnFixedUpdate(float dt)
	{
		base.OnFixedUpdate(dt);
		if (shuffleAllWaitTime > 0)
		{
			shuffleAllWaitTime -= dt;
			if (shuffleAllWaitTime <= 0)
			{
				end();
				RemoveSelf();
			}
		}
	}

	private void WaitAndEndSkill()
	{
		shuffleAllWaitTime = Shuffle.totalShufflingTime;
	}

	private IEnumerator WaitAndEndSkill(float delay)
	{
		yield return new WaitForSeconds(delay);
	}
}