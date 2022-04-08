using System;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;
using AnimationState = Spine.AnimationState;

public class ElvenSetSkill : ItemSetSkill
{
	private Data.TileTypes fromTypes;
	private Data.TileTypes toTypes;
	private Dictionary<TilePoint, Data.TileTypes> stack;

	protected override void OnDestroy()
	{
	}

	protected override void OnAdd()
	{
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
		SoundManager.intance.PlaySound (SoundName.ELF_SET_SKILL);
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();

		string[] parms = extrasInfo.Split('/');
		fromTypes = (Data.TileTypes)Enum.Parse(typeof(Data.TileTypes), parms[0], true);
		toTypes = (Data.TileTypes)Enum.Parse(typeof(Data.TileTypes), parms[1], true);

		stack = FindMatchingGem();
		if (stack.Count < 1)
		{
			end();
			return;
		}
		PlayCastFx();
	}

	private void PlayCastFx()
	{
		GameObject fxGameObject = effectsManager.CreateElvenSetSkillEffect();
		fxGameObject.GetComponent<SkeletonAnimation>().state.End += delegate(AnimationState state, int trackingIndex)
		{
			TransformGems(stack, toTypes);
		};
		
	}

	private Dictionary<TilePoint, Data.TileTypes> FindMatchingGem()
	{
		Cell[,] cells = matchLogic.Cells;
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		for (int x = 0; x < Data.tileWidth; x++) {
			for (int y = 0; y < Data.tileHeight; y++) {
				Cell thiscell = cells[x, y];
				if (thiscell.cellType != fromTypes) {
					continue;
				}
				if (!stack.ContainsKey(new TilePoint(x, y))) {
					TilePoint point = new TilePoint(x, y);
					stack.Add(point, cells[x, y].cellType);
				}
			}
		}
		return stack;
	}

	private void TransformGems(Dictionary<TilePoint, Data.TileTypes> stack, Data.TileTypes toType) {
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(matchLogic.FindTile(item.Key));
		}

		for (int i = 0; i < destroyList.Count; i++) {
			TweenParms parms = new TweenParms().Prop("localScale", Vector3.zero).Ease(EaseType.EaseOutSine);
			HOTween.To(destroyList[i].transform, 0.2f, parms);
		}
		matchLogic.routineRunner.StartCoroutine(TransformIn(destroyList, 0.25f, toType));
	}

	private IEnumerator TransformIn(List<MatchItem> destroyList, float delay, Data.TileTypes toType) {
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < destroyList.Count; i++) {
			destroyList[i].cell.cellType = toType;
			destroyList[i].GetComponent<UISprite>().spriteName = toType.ToString();
			TweenParms parms = new TweenParms().Prop("localScale", Vector3.one).Ease(EaseType.EaseOutSine);
			HOTween.To(destroyList[i].transform, 0.3f, parms);
		}
		matchLogic.routineRunner.StartCoroutine(matchLogic.CheckMatch3TileOnly(0.35f));
		matchLogic.routineRunner.StartCoroutine(OnAnimationEnd(0.35f));
	}

	private IEnumerator OnAnimationEnd(float delay) {
		yield return new WaitForSeconds(delay);
		//		Logger.Trace(state.GetCurrent(0).Animation.Name, " end");
		end();
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[2];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[3];
	}
}