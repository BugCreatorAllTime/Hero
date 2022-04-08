using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using AnimationState = Spine.AnimationState;

public class DestroyGemSkill : MonsterSkill
{
	Dictionary<TilePoint, Data.TileTypes> stack;

	protected override void OnPlayMonsterSkillEff ()
	{
		base.OnPlayMonsterSkillEff ();
	}

	protected override void ProcessLogicSkill()
	{
		base.ProcessLogicSkill();
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		Cell[,] cells = matchLogic.Cells;
		stack = new Dictionary<TilePoint, Data.TileTypes>();
		int numberOfAffectedGems = skillInfo.numberOfAffectedGems;
		for (int x = 0; x < Data.tileWidth; x++) {
			for (int y = 0; y < Data.tileHeight; y++) {
				Cell thiscell = cells[x, y];
				if (numberOfAffectedGems <= 0) {
					break;
				}
				Data.TileTypes affectedType = skillInfo.Convert(skillInfo.Parse(skillInfo.affectedGemType));
				if (thiscell.cellType != affectedType) {
					continue;
				}
				numberOfAffectedGems--;
				if (!stack.ContainsKey(new TilePoint(x, y))) {
					TilePoint point = new TilePoint(x, y);
					stack.Add(point, cells[x, y].cellType);
					GameObject fxObject =
						effectsManager.CreateSpineEffect(Effects.Setup(skillInfo.animation, skillInfo.affectedGemType.ToString()),
							FxAnimationName.active, null);
					MatchItem gem = matchLogic.FindTile(point);
					gem.GetComponent<UISprite>().spriteName = "";
					fxObject.transform.localPosition = new Vector3(gem.gameObject.transform.localPosition.x,
						gem.gameObject.transform.localPosition.y, 0);
					SkeletonAnimation anim = fxObject.GetComponent<SkeletonAnimation>();
					if (numberOfAffectedGems == skillInfo.numberOfAffectedGems - 1) {
						anim.state.End += new AnimationState.StartEndDelegate(OnAnimationEnd);
					}
				}
			}
		}
	}

	protected void OnAnimationEnd(AnimationState state, int trackIndex) {
//		Logger.Trace("destroyGemSkill animation end");
		matchLogic.SetMatch(stack);
		end();
		RemoveSelf();
	}
}
