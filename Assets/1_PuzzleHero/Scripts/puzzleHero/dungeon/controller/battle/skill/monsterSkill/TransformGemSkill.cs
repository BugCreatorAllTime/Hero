using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;
using System.Collections;

public class TransformGemSkill : MonsterSkill
{
	protected override void OnPlayMonsterSkillEff ()
	{
		base.OnPlayMonsterSkillEff ();
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		//		Logger.Trace(skillInfo.skillType, " activated ", "from ", skillInfo.fromGemType, " to ", skillInfo.toGemType, " numberOfAffectedGem ", skillInfo.numberOfAffectedGems);
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		Cell[,] cells = matchLogic.Cells;
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		int numberOfAffectedGems = skillInfo.numberOfAffectedGems;
		for (int x = 0; x < Data.tileWidth; x++) {
			for (int y = 0; y < Data.tileHeight; y++) {
				Cell thiscell = cells[x, y];
				if (numberOfAffectedGems <= 0) {
					//					Logger.Trace("no of affected gem <=0");
					break;
				}
				if (thiscell.cellType != skillInfo.Convert(skillInfo.Parse(skillInfo.fromGemType))) {
					continue;
				}
				numberOfAffectedGems--;
				float hideDuration = 0;
				if (!stack.ContainsKey(new TilePoint(x, y))) {
					TilePoint point = new TilePoint(x, y);
					stack.Add(point, cells[x, y].cellType);
				}
			}
		}
		//TODO End skill right away if there is no affected gem

		TransformGems(stack, skillInfo.Convert(skillInfo.Parse(skillInfo.toGemType)));
	}

	private void TransformGems(Dictionary<TilePoint, Data.TileTypes> stack, Data.TileTypes toType) {
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(matchLogic.FindTile(item.Key));
		}

		float delay = 0;
		for (int i = 0; i < destroyList.Count; i++)
		{
			MatchItem item = destroyList[i];
			TweenParms parms = new TweenParms().Prop("localScale", Vector3.zero).Ease(EaseType.EaseOutSine);
			HOTween.To(item.transform, 0, parms);
			string skin = item.cell.cellType.ToString();
			GameObject fxGameObject = effectsManager.CreateSpineEffect(Effects.transform, FxAnimationName.active, skin, false, null, true);
			fxGameObject.transform.localPosition = item.transform.localPosition;
			SkeletonAnimation anim = fxGameObject.GetComponent<SkeletonAnimation>();
			delay = anim.state.GetCurrent(0).Animation.Duration;
		}
		matchLogic.routineRunner.StartCoroutine(TransformIn(destroyList, delay + 0.05f, toType));
	}

	private IEnumerator TransformIn(List<MatchItem> destroyList, float delay, Data.TileTypes toType) {
		yield return new WaitForSeconds(delay);
		float duration = 0;
		for (int i = 0; i < destroyList.Count; i++) {
			MatchItem item = destroyList[i];
			item.cell.cellType = toType;
			item.GetComponent<UISprite>().spriteName = toType.ToString();
			GameObject fxGameObject = effectsManager.CreateSpineEffect(Effects.transform, FxAnimationName.end, toType.ToString(), false, null, true);
			fxGameObject.transform.localScale = MatchLogic.cellScale;
			fxGameObject.transform.localPosition = item.transform.localPosition;
			duration = fxGameObject.GetComponent<SkeletonAnimation>().state.GetCurrent(0).Animation.Duration;
			TweenParms parms = new TweenParms().Prop("localScale", MatchLogic.cellScale).Ease(EaseType.EaseOutSine);
			HOTween.To(destroyList[i].transform, duration, parms);
		}
		matchLogic.routineRunner.StartCoroutine(matchLogic.CheckMatch3TileOnly(duration + .05f));
		matchLogic.routineRunner.StartCoroutine(OnAnimationEnd(duration + .05f));
	}

	private IEnumerator OnAnimationEnd(float delay) {
		yield return new WaitForSeconds(delay);
		//		Logger.Trace(state.GetCurrent(0).Animation.Name, " end");
		end();
		RemoveSelf();
	}
}

