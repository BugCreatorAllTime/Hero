using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PaladinSetSkill : ItemSetSkill
{
	private Dictionary<TilePoint, Data.TileTypes> affectedGems;
	private List<MatchItem> counteredGems;
	private float hpPercent;
	private float armorPercent;

	protected override void OnDestroy()
	{
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.PALADIN_SET_SKILL);
	}

	protected override void OnAdd()
	{
		string[] parms = extrasInfo.Split('/');
		hpPercent = float.Parse(parms[0]);
		armorPercent = float.Parse(parms[1]);
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();

		PlayCastFx();
	}

	private void PlayCastFx()
	{
		GameObject fxGameObject = effectsManager.CreatePaladinSkillEffect();
		float duration = fxGameObject.GetComponent<SkeletonAnimation>().state.GetCurrent(0).Animation.Duration - 1.2f;
		matchLogic.routineRunner.StartCoroutine(WaitThenActivateLogic(duration));
	}

	private IEnumerator WaitThenActivateLogic(float delay)
	{
		yield return new WaitForSeconds(delay);
		PlayCastToBoardEffect();
		SelfBuff();
		if (affectedGems.Count < 1)
		{
			matchLogic.routineRunner.StartCoroutine(WaitThenEnd(1f));
		}
	}

	private void PlayCastToBoardEffect()
	{
		affectedGems = FindMatchingGems();
		if (affectedGems.Count > 0)
		{
			counteredGems = new List<MatchItem>();
			for (int i = 0; i < affectedGems.Count; i++)
			{
				MatchItem gem = matchLogic.FindTile(affectedGems.ElementAt(i).Key);
				counteredGems.Add(gem);
			}
		}
		else
		{
			return;
		}
		float longestDuration = 0;
		for (int i = 0; i < counteredGems.Count; i++)
		{
			Vector3 p0 = EffectsManager.team1PaladinSetSkillPosition2;
			Vector3 p1 = EffectsManager.boardTopCenter;
			Vector3 p2 = counteredGems[i].transform.localPosition;
			Vector3[] path = new Vector3[] {p0, p1, p2};
			float duration = effectsManager.CreateCastFlyEffect(this.GetType(), path);

			matchLogic.routineRunner.StartCoroutine(PlayCounterEffect(duration, i));
			if (duration >= longestDuration) longestDuration = duration;
		}
		matchLogic.routineRunner.StartCoroutine(WaitForCounterEffectFinish(longestDuration));
		SoundManager.intance.PlaySound(SoundName.CAST_SKILL_TO_BOARD);
		//		Logger.Trace("duration ", duration);
	}

	private IEnumerator PlayCounterEffect(float delay, int fxIndex)
	{
		yield return new WaitForSeconds(delay);
		MatchItem gem = counteredGems[fxIndex];
		GameObject fxObject = effectsManager.CreateSpineEffect(Effects.counter, FxAnimationName.begin, null, false,
			null, true);
		fxObject.transform.localPosition = new Vector3(gem.gameObject.transform.localPosition.x,
			gem.gameObject.transform.localPosition.y, 0);
	}

	private IEnumerator WaitForCounterEffectFinish(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		matchLogic.Counter(affectedGems);
		end();
	}

	private IEnumerator WaitThenEnd(float delay)
	{
		yield return new WaitForSeconds(delay);
		end();
	}

	private void SelfBuff()
	{
		float hp = owner.getStat().GetMaxHp() * hpPercent;
		owner.AddBuff(new HealBuff(hp));

		float armor = owner.getStat().GetMaxArmor() * armorPercent;
		owner.AddBuff(new ArmorBuff(armor));
	}

	private Dictionary<TilePoint, Data.TileTypes> FindMatchingGems()
	{
		Cell[,] cells = matchLogic.Cells;
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		for (int x = 0; x < Data.tileWidth; x++)
		{
			for (int y = 0; y < Data.tileHeight; y++)
			{
				TilePoint point = new TilePoint(x, y);
				if (!matchLogic.FindTile(point).IsBeingAffected())
				{
					continue;
				}
				stack.Add(point, cells[x, y].cellType);
			}
		}
		return stack;
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[2];
	}


	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[3];
	}
}