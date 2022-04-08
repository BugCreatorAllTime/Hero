using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Microsoft.Win32;
using Spine;
using UnityEngine;

public class ShuffleMatchGemSkill : PoisonSkill
{
	private Dictionary<TilePoint, Data.TileTypes> matchableGems;
	private List<GameObject> fxObjects = new List<GameObject>();

	protected override void AutoActivate()
	{
	}

	protected override void OnCastFinished()
	{
		List<MatchItem> destinations = FindDestinationGems();
		for (int i = 0; i < affectedGems.Count; i++)
		{
//			Logger.Trace("affected ", affectedGems[i].point, "  dest  ", destinations[i].point);
			Vector3 affectedPos = Vector3.zero + affectedGems[i].transform.localPosition;
			CreateSwapFx(affectedPos);
			Vector3 destinationPos = Vector3.zero + destinations[i].transform.localPosition;
			CreateSwapFx(destinationPos);
			TweenParms parms = new TweenParms();
			parms.Prop("localPosition", destinationPos).Ease(EaseType.EaseInOutCubic);
			HOTween.To(affectedGems[i].transform, 1.5f, parms);

			parms.NewProp("localPosition", affectedPos).Ease(EaseType.EaseInOutCubic);
			HOTween.To(destinations[i].transform, 1.5f, parms);

			matchLogic.DoSwapTile(affectedGems[i], destinations[i]);
		}
		matchLogic.routineRunner.StartCoroutine(OnSwapMotionComplete(1.6f));
	}

	private void CreateSwapFx(Vector3 gemPos)
	{
		Vector3 v = new Vector3(0, MatchLogic.cellHeight / 2 - 5, 0);
		GameObject go = effectsManager.CreateSwapFx();
		go.transform.localPosition = gemPos - v;
		fxObjects.Add(go);
	}

	private void EndSwapFx()
	{
		float animDuration = 0;
		for (int i = 0; i < fxObjects.Count; i++)
		{
			Spine.AnimationState s = fxObjects[i].GetComponent<SkeletonAnimation>().state;
			s.SetAnimation(0, FxAnimationName.end, false);
			animDuration = s.GetCurrent(0).Animation.Duration;
		}
		matchLogic.routineRunner.StartCoroutine(ReturnFxToPool(animDuration));
	}

	private IEnumerator ReturnFxToPool(float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < fxObjects.Count; i++)
		{
			GameObject go = fxObjects[i];
			go.SetActive(false);
			effectsManager.spinePool.ReturnInstance(go);
		}
	}

	private IEnumerator OnSwapMotionComplete(float delay)
	{
		yield return new WaitForSeconds(delay);
		EndSwapFx();
		matchLogic.routineRunner.StartCoroutine(matchLogic.CheckMatch3TileOnly(0));
		end();
		RemoveSelf();
	}

	private List<MatchItem> FindDestinationGems()
	{
		List<TilePoint> affectableGems = FindAffectableGems();
		int numberOfAffectedGems = affectableGems.Count > 2 ? 2 : affectableGems.Count;
		List<TilePoint> unavailableDestinations = new List<TilePoint>();
		List<TilePoint> availableDestinations = new List<TilePoint>();

		List<TilePoint> randomizedGems = new List<TilePoint>();
		List<MatchItem> destinations = new List<MatchItem>();

		Cell[,] cells = matchLogic.Cells;

		for (int i = 0; i < numberOfAffectedGems; i++)
		{
			availableDestinations.Clear();
//			Logger.Trace("for gem ", affectableGems[i], ", available are");
			affectedGems.Add(matchLogic.FindTile(affectableGems[i]));
			for (var x = 0; x < Data.tileWidth; x++) {
				for (var y = 0; y < Data.tileHeight; y++) {
					Cell[,] clone = new Cell[Data.tileWidth, Data.tileHeight];
					System.Array.Copy(cells, clone, Data.tileWidth * Data.tileHeight);
					var thiscell = clone[x, y];
					clone[x, y] = clone[affectableGems[i].x, affectableGems[i].y];
					clone[affectableGems[i].x, affectableGems[i].y] = thiscell;

					TilePoint pointSrc = new TilePoint(x, y);
					MatchItem tileSrc = matchLogic.FindTile(pointSrc);
					TilePoint pointDst = new TilePoint(affectableGems[i].x, affectableGems[i].y);
					MatchItem tileDst = matchLogic.FindTile(pointDst);

					Dictionary<TilePoint, Data.TileTypes> st = new Dictionary<TilePoint, Data.TileTypes>();
					st = matchLogic.FindMatch(clone);
//					Logger.Trace("gem ", new TilePoint(x, y), " has match count ", st.Count);
					if (st.Count == 0 && (x != affectableGems[i].x || y != affectableGems[i].y)
						&& tileSrc.cell.cellType != tileDst.cell.cellType) {
						availableDestinations.Add(new TilePoint(x, y));
//						Logger.Trace(new TilePoint(x, y));
					}
				}
			}

			
				TilePoint tilePoint;
				do {
					int index = new System.Random().Next(0, availableDestinations.Count);
					//				Logger.Trace("random index ", index);
					tilePoint = availableDestinations[index];
				} while (Contains(randomizedGems, tilePoint));
				randomizedGems.Add(tilePoint);
				MatchItem gem = matchLogic.FindTile(tilePoint);
				destinations.Add(gem);
			
		}

		
		return destinations;
	}

	private bool Contains(List<TilePoint> list, TilePoint point)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].x == point.x && list[i].y == point.y)
			{
				return true;
			}
		}
		return false;
	}

	private List<MatchItem> CloneTiles(List<MatchItem> tiles)
	{
		List<MatchItem> clone = new List<MatchItem>();
		for (int i = 0; i < tiles.Count; i++)
		{
			MatchItem item = new MatchItem();
			item.point = new TilePoint(tiles[i].point.x, tiles[i].point.y);
			Cell cell = new Cell();
			cell.cellType = tiles[i].cell.cellType;
			item.cell = cell;
			clone.Add(item);
		}
		return clone;
	}

	public MatchItem FindTile(List<MatchItem> tiles, TilePoint point) {
		foreach (MatchItem tile in tiles) {
			if (tile.point.Equals(point)) return tile;
		}
		return null;
	}

	protected override List<TilePoint> FindAffectableGems()
	{
		string str = "";
		Cell[,] cells = matchLogic.Cells;
		List<TilePoint> affectableGems = new List<TilePoint>();
		for (int x = 0; x < Data.tileWidth; x++)
		{
			for (int y = 0; y < Data.tileHeight; y++)
			{
				Cell thiscell = cells[x, y];
				BoardSkillInfo.AffectType type = skillInfo.Parse(skillInfo.affectedGemType);
				if (!IsThisGemTypePickable(thiscell.cellType, type))
				{
					continue;
				}
				TilePoint point = new TilePoint(x, y);
				MatchItem tile = matchLogic.FindTile(point);
				if (tile.IsBeingAffected())
				{
					continue;
				}
				if (!IsThisGemMatchable(tile))
				{
					continue;
				}
				affectableGems.Add(point);
				str += point + ", ";

			}
		}
//		Logger.Trace("affectable gem ", str);
		return affectableGems;
	}

	private bool IsThisGemMatchable(MatchItem gem)
	{
		if (matchableGems == null)
		{
			matchableGems = matchLogic.FindHint();
		}
		for (int i = 0; i < matchableGems.Count; i++)
		{
			KeyValuePair<TilePoint, Data.TileTypes> pair = matchableGems.ElementAt(i);
			if (pair.Key.x == gem.point.x && pair.Key.y == gem.point.y)
			{
//				Logger.Trace("gem ", gem.point.x, ", ", gem.point.y, " matchable");
				return true;
			}
		}

		return false;
	}

	public override bool AreThereAnyAffectableGems()
	{
		return FindAffectableGems().Count > 0;
	}
}