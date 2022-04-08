using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using strange.examples.strangerocks;
using UnityEngine;

public class Matches
{
	public enum Shape
	{
		Three, Four, Five, Tl, Undefined
	}

	private const float gemWaitTime = 0.115f;
	private MatchLogic matchLogic;
	private EffectsManager effectsManager;
	private float delayFillEmpty = 0;
	private List<Transform> cams;

	public Matches(MatchLogic matchLogic, EffectsManager effectsManager)
	{
		this.matchLogic = matchLogic;
		this.effectsManager = effectsManager;
	}

	public bool IsMatchComplex(List<MatchItem> matchedGems, out List<List<TilePoint>> outPartitionedMatches)
	{
		outPartitionedMatches = null;
		if (matchedGems.Count < 4)
		{
			return false;
		}

		List<List<MatchItem>> columns = FindMatchedColumns(matchedGems);
		List<List<MatchItem>> rows = FindMatchedRows(matchedGems);
		List<UnitedMatch> unitedMatches = new List<UnitedMatch>();
		List<List<TilePoint>> partitionedMatches = new List<List<TilePoint>>();

		MergeFromColumns(unitedMatches, columns, rows);
		MergeFromRows(unitedMatches, columns, rows);

		for (int i = 0; i < unitedMatches.Count; i++)
		{
			UnitedMatch unitedMatch = unitedMatches[i];
			List<TilePoint> partitionedMatch = new List<TilePoint>();
			partitionedMatches.Add(partitionedMatch);
			for (int k = 0; k < unitedMatch.columns.Count; k++)
			{
				int columnIndex = unitedMatch.columns[k];
				List<MatchItem> column = columns[columnIndex];
				for (int l = 0; l < column.Count; l++)
				{
					MatchItem itemColumn = column[l];
					if (!partitionedMatch.Contains(itemColumn.point)) partitionedMatch.Add(itemColumn.point);
				}
			}

			for (int k = 0; k < unitedMatch.rows.Count; k++)
			{
				int rowIndex = unitedMatch.rows[k];
				List<MatchItem> row = rows[rowIndex];
				for (int l = 0; l < row.Count; l++)
				{
					MatchItem itemRow = row[l];
					if(!partitionedMatch.Contains(itemRow.point)) partitionedMatch.Add(itemRow.point);
				}
			}
		}

		/*for (int i = 0; i < partitionedMatches.Count; i++) {
			Logger.Trace("index ", i);
			List<TilePoint> partitionMatch = partitionedMatches[i];
			string s = "";
			for (int k = 0; k < partitionMatch.Count; k++) {
				s += partitionMatch[k] + ", ";
			}
			Logger.Trace("partitioned points", s);
		}*/
		for (int i = 0; i < partitionedMatches.Count; i++)
		{
			List<TilePoint> partitionMatch = partitionedMatches[i];
			if (partitionMatch.Count >= 4)
			{
				outPartitionedMatches = partitionedMatches;
				return true;
			}
			if (i == partitionedMatches.Count - 1) return false;
		}

		return false;
	}

	private bool IsBoth5AndTlMatch(List<TilePoint> partitionMatch)
	{
		if (!IsTLMatch(partitionMatch)) return false;

		for (int i = 0; i < partitionMatch.Count; i++)
		{
			TilePoint point1 = partitionMatch[i];
			for (int k = 0; k < partitionMatch.Count; k++)
			{
				TilePoint point2 = partitionMatch[k];
				if (point1.y == point2.y && Mathf.Abs(point1.x - point2.x) >= 4) return true;
				if (point1.x == point2.x && Mathf.Abs(point1.y - point2.y) >= 4) return true;
			}
		}
		return false;
	}

	private bool IsTLMatch(List<TilePoint> partitionMatch)
	{
		List<int> rows = new List<int>();
		List<int> columns = new List<int>();
		int rowCount = 0;
		int columnCount = 0;
		for (int i = 0; i < partitionMatch.Count; i++)
		{
			TilePoint point = partitionMatch[i];
			if(!rows.Contains(point.y)) rows.Add(point.y);
			if(!columns.Contains(point.x)) columns.Add(point.x);
		}
		if (rows.Count >= 3 && columns.Count >= 3)
		{
			return true;
		}
		return false;
	}

	private bool IsTLMatch(List<MatchItem> partitionMatch) {
		List<TilePoint> points = new List<TilePoint>();
		for (int i = 0; i < partitionMatch.Count; i++)
		{
			points.Add(partitionMatch[i].point);
		}
		return IsTLMatch(points);
	}

	private List<MatchItem> ConvertToMatch5(List<TilePoint> partitionMatch)
	{
		List<TilePoint> result = new List<TilePoint>();
		for (int i = 0; i < partitionMatch.Count; i++) {
			TilePoint point1 = partitionMatch[i];
			for (int k = 0; k < partitionMatch.Count; k++) {
				TilePoint point2 = partitionMatch[k];

				if (point1.y == point2.y)
				{
					int distanceX = Mathf.Abs(point1.x - point2.x);
					for (int m = Data.tileWidth - 1; m >= 4; m--)
					{
						if (distanceX == m)
						{
							for (int p = 0; p < partitionMatch.Count; p++)
							{
								TilePoint point3 = partitionMatch[p];
								if (point3.y == point1.y && !result.Contains(point3))
								{
									result.Add(point3);
								}
							}
						}
					}
				}
				else if (point1.x == point2.x)
				{
					int distanceY = Mathf.Abs(point1.y - point2.y);
					for (int m = Data.tileHeight - 1; m >= 4; m--)
					{
						if (distanceY == m)
						{
							for (int p = 0; p < partitionMatch.Count; p++)
							{
								TilePoint point3 = partitionMatch[p];
								if (point3.x == point1.x && !result.Contains(point3))
								{
									result.Add(point3);
								}
							}
						}
					}
				}

			}
		}
		return FromPointsToMatchItems(result);
	}

	private bool IsUnitedMatchesContains(List<UnitedMatch> unitedMatches, UnitedMatch match)
	{
		for (int i = 0; i < match.columns.Count; i++)
		{
			int column = match.columns[i];
			for (int k = 0; k < unitedMatches.Count; k++)
			{
				if(unitedMatches[k].columns.Contains(column)) return true;
			}
		}

		for (int i = 0; i < match.rows.Count; i++) {
			int row = match.rows[i];
			for (int k = 0; k < unitedMatches.Count; k++) {
				if (unitedMatches[k].rows.Contains(row)) return true;
			}
		}

		return false;
	}

	private void MergeToUnitedMatches(List<UnitedMatch> unitedMatches, UnitedMatch match)
	{
		for (int i = 0; i < unitedMatches.Count; i++)
		{
			UnitedMatch item = unitedMatches[i];
			if (match.PartialyContains(item))
			{
				for (int k = 0; k < match.columns.Count; k++)
				{
					if(item.columns.Contains(match.columns[k])) continue;
					item.columns.Add(match.columns[k]);
				}
				for (int k = 0; k < match.rows.Count; k++)
				{
					if (item.rows.Contains(match.rows[k])) continue;
					item.rows.Add(match.rows[k]);
				}
				return;
			}
		}
	}

	private void MergeFromRows(List<UnitedMatch> unitedMatches, List<List<MatchItem>> columns, List<List<MatchItem>> rows)
	{
		for (int i = 0; i < rows.Count; i++) {
			List<MatchItem> row = rows[i];
			UnitedMatch unitedMatch = new UnitedMatch();
			if (!unitedMatch.rows.Contains(i)) unitedMatch.rows.Add(i);
			for (int l = 0; l < row.Count; l++) {
				MatchItem itemRow = row[l];

				for (int k = 0; k < columns.Count; k++) {
					List<MatchItem> column = columns[k];
					for (int m = 0; m < column.Count; m++) {
						MatchItem itemColumn = column[m];
						if (itemRow.point.Equals(itemColumn.point)) {
							if(!unitedMatch.columns.Contains(k)) unitedMatch.columns.Add(k);
							if(!unitedMatch.rows.Contains(i)) unitedMatch.rows.Add(i);
						}
					}
				}

			}

			if (!IsUnitedMatchesContains(unitedMatches, unitedMatch))
			{
				unitedMatches.Add(unitedMatch);
			}
			else
			{
				MergeToUnitedMatches(unitedMatches, unitedMatch);
			}
		}
	}

	private void MergeFromColumns(List<UnitedMatch> unitedMatches, List<List<MatchItem>> columns, List<List<MatchItem>> rows) {
		for (int i = 0; i < columns.Count; i++) {
			List<MatchItem> column = columns[i];
			UnitedMatch unitedMatch = new UnitedMatch();
			if (!unitedMatch.columns.Contains(i)) unitedMatch.columns.Add(i);
			for (int l = 0; l < column.Count; l++) {
				MatchItem itemColumn = column[l];

				for (int k = 0; k < rows.Count; k++) {
					List<MatchItem> row = rows[k];
					for (int m = 0; m < row.Count; m++) {
						MatchItem itemRow = row[m];
						if (itemColumn.point.Equals(itemRow.point)) {
							if(!unitedMatch.columns.Contains(i)) unitedMatch.columns.Add(i);
							if(!unitedMatch.rows.Contains(k)) unitedMatch.rows.Add(k);
						}
					}
				}

			}

			if (!IsUnitedMatchesContains(unitedMatches, unitedMatch)) {
				unitedMatches.Add(unitedMatch);
			}
			else {
				MergeToUnitedMatches(unitedMatches, unitedMatch);
			}
		}
	}

	public static List<List<MatchItem>> FindMatchedColumns(List<MatchItem> matchedGems)
	{
		List<List<MatchItem>> columns = new List<List<MatchItem>>();
		Dictionary<int, List<MatchItem>> filteredColumns = new Dictionary<int, List<MatchItem>>();
		for (int i = 0; i < matchedGems.Count; i++)
		{
			MatchItem gem = matchedGems[i];
			MatchItem topGem = gem;
			List<MatchItem> completedColumn = new List<MatchItem>();
			MatchItem[] column = new MatchItem[Data.tileHeight];
			for (int k = 0; k < matchedGems.Count; k++)
			{
				if (i == k) continue;
				MatchItem gem2 = matchedGems[k];
				if (gem.cell.cellType != gem2.cell.cellType) continue;
				if (gem.point.x != gem2.point.x) continue;
				if (gem.point.y < gem2.point.y) continue;
				if(Mathf.Abs(gem.point.y - gem2.point.y) > 1) continue;

				topGem = gem2;
			}

			column[0] = topGem;
			for (int k = 0; k < matchedGems.Count; k++) {
				if (i == k) continue;
				MatchItem gem2 = matchedGems[k];
				if (column[0].cell.cellType != gem2.cell.cellType) continue;
				if (column[0].point.x != gem2.point.x) continue;
				if (column[0].point.y > gem2.point.y) continue;

				column[gem2.point.y - column[0].point.y] = gem2;
			}

			if (column[0] == null) continue;
			completedColumn.Add(column[0]);
			for (int k = 1; k < column.Length; k++)
			{
				MatchItem item = column[k];
				if (item != null)
				{
					completedColumn.Add(item);
				}
				else
				{
					break;
				}
			}
			if (completedColumn.Count >= 3)
			{
				columns.Add(completedColumn);
			}
		}
		/*for (int i = 0; i < columns.Count; i++)
		{
			List<MatchItem> column = columns[i];
			string s = "";
			for (int k = 0; k < column.Count; k++)
			{
				s += column[k].point + ", ";
			}
			Logger.Trace("columns collection ", s);
		}*/
		return columns;
	}

	public static List<List<MatchItem>> FindMatchedRows(List<MatchItem> matchedGems)
	{
		List<List<MatchItem>> rows = new List<List<MatchItem>>();
		Dictionary<int, List<MatchItem>> filteredRows = new Dictionary<int, List<MatchItem>>();
		for (int i = 0; i < matchedGems.Count; i++)
		{
			MatchItem gem = matchedGems[i];
			MatchItem mostLeftGem = gem;
			List<MatchItem> completedRow = new List<MatchItem>();
			MatchItem[] row = new MatchItem[Data.tileWidth];
			for (int k = 0; k < matchedGems.Count; k++)
			{
				if (i == k) continue;
				MatchItem gem2 = matchedGems[k];
				if (gem.cell.cellType != gem2.cell.cellType) continue;
				if (gem.point.y != gem2.point.y) continue;
				if (gem.point.x < gem2.point.x) continue;
				if (Mathf.Abs(gem.point.x - gem2.point.x) > 1) continue;

				mostLeftGem = gem2;
			}

			row[0] = mostLeftGem;
			for (int k = 0; k < matchedGems.Count; k++) {
				if (i == k) continue;
				MatchItem gem2 = matchedGems[k];
				if (row[0].cell.cellType != gem2.cell.cellType) continue;
				if (row[0].point.y != gem2.point.y) continue;
				if (row[0].point.x > gem2.point.x) continue;

				row[gem2.point.x - row[0].point.x] = gem2;
			}

			if (row[0] == null) continue;
			completedRow.Add(row[0]);
			for (int k = 1; k < row.Length; k++)
			{
				MatchItem item = row[k];
				if (item != null)
				{
					completedRow.Add(item);
				}
				else
				{
					break;
				}
			}
			if (completedRow.Count >= 3)
			{
				rows.Add(completedRow);
			}
		}
		/*for (int i = 0; i < rows.Count; i++)
		{
			List<MatchItem> row = rows[i];
			string s = "";
			for (int k = 0; k < row.Count; k++)
			{
				s += row[k].point + ", ";
			}
			Logger.Trace("rows collection ", s);
		}*/
		return rows;
	}

	public void PerformComplexMatches(List<List<TilePoint>> partitionedMatches, MatchItem a, MatchItem b)
	{
		List<TilePoint> destroyList = new List<TilePoint>();
		for (int i = 0; i < partitionedMatches.Count; i++) {
			List<TilePoint> partitionedMatch = partitionedMatches[i];
			Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
			List<MatchItem> match = new List<MatchItem>();
			for (int k = 0; k < partitionedMatch.Count; k++)
			{
				MatchItem tile = matchLogic.FindTile(partitionedMatch[k]);
				match.Add(tile);
				stack.Add(tile.point, tile.cell.cellType);
			}

			List<MatchItem> gems = null;
			if (partitionedMatch.Count == 4)
			{
				gems = CheckMatch4(match, a, b);
				matchLogic.PlaySound(SoundName.MATCH_FOUR);
			}
			else if (IsBoth5AndTlMatch(partitionedMatch))
			{
				gems = CheckMatch5(ConvertToMatch5(partitionedMatch));
				matchLogic.PlaySound(SoundName.MATCH_FIVE);
			}
			else if (IsTLMatch(partitionedMatch))
			{
				gems = this.CheckMatchTl(match);
				matchLogic.PlaySound(SoundName.MATCH_TL);
			}
			else if (partitionedMatch.Count >= 5)
			{
				gems = this.CheckMatch5(match);
				matchLogic.PlaySound(SoundName.MATCH_FIVE);
			}
			else
			{
				gems = CheckMatch3(match);
			}
			for (int k = 0; k < gems.Count; k++)
			{
				if (!destroyList.Contains(gems[k].point))
				{
					destroyList.Add(gems[k].point);
				}
			}
		}
		matchLogic.routineRunner.StartCoroutine(SetEmpty(destroyList, delayFillEmpty - 0.1f));
		//matchLogic.routineRunner.StartCoroutine(matchLogic.FillEmpty(delayFillEmpty + 1f));
		delayFillEmpty = 0;
	}

	private List<MatchItem> CheckMatch4(List<MatchItem> matchedGems, MatchItem a, MatchItem b)
	{
		//Logger.Trace("check match 4");
		MatchItem centerGem;
		if (a == null || b == null)
		{
			centerGem = matchedGems[1];
		}
		else
		{
			centerGem = (a.cell.cellType == matchedGems[0].cell.cellType) ? a : b;
		}
		//centerGem.labelString = "Center";
		MatchItem gem0 = matchedGems[0];
		MatchItem gem1 = matchedGems[1];
		List<TilePoint> matchedPoints = FromMatchItemsToPoints(matchedGems);
		List<TilePoint> undefinedPoints = new List<TilePoint>();
		List<TilePoint> destroyPoints = new List<TilePoint>();

		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			MatchItem gem = matchLogic.tiles[i];
			if (matchedPoints.Contains(gem.point))
			{
				destroyPoints.Add(gem.point);
				//gem.labelString = "DESTROY";
				continue;
			}
			if (Mathf.Abs(gem.point.x - centerGem.point.x) <= 1 && Mathf.Abs(gem.point.y - centerGem.point.y) <= 1)
			{
				//Logger.Trace("center gem", centerGem.point, "N8", gem.point);
				//gem.labelString = gem.point.ToString();
				undefinedPoints.Add(gem.point);
				destroyPoints.Add(gem.point);
				//gem.labelString = "DESTROY";
			}
		}
		float delay = 0;

		effectsManager.CreateMatch4Fx(centerGem);
		List<MatchItem> destroyGems = FromPointsToMatchItems(destroyPoints);
		CreateMatchFx(destroyGems);
		matchLogic.NotifyMatchedGems(destroyGems);

		DispatchActionSignals(matchedGems, FromPointsToMatchItems(undefinedPoints), Shape.Four);
		delay += 0.5f;
		delayFillEmpty = delayFillEmpty >= delay ? delayFillEmpty : delay;
		ShakeCamera();
		return destroyGems;
	}

	private void ShakeCamera()
	{
		if (cams == null)
		{
			Transform mainCamTransform = Camera.main.transform;
			Transform nguiCamTransform = GameObject.Find("UI Root").transform.GetComponentInChildren<UICamera>().transform;
			cams = new List<Transform>();
			cams.Add(mainCamTransform);
			cams.Add(nguiCamTransform);
		}
		//Logger.Trace("cams count", cams.Count);
		Shaker.Shake((RoutineRunner)matchLogic.routineRunner, cams);
	}

	private List<MatchItem> CheckMatch5(List<MatchItem> matchedGems)
	{
//		Logger.Trace("check match 5");
		List<int> columns = new List<int>();
		List<int> rows = new List<int>();
		List<TilePoint> undefinedGems = new List<TilePoint>();
		List<TilePoint> explosionGems = new List<TilePoint>();
		List<TilePoint> destroyGems = new List<TilePoint>();

		bool verticalMatch = false;
		MatchItem gem0 = matchedGems[0];
		MatchItem gem1 = matchedGems[1];
		if (gem0.point.x == gem1.point.x) verticalMatch = true;

		for (int i = 0; i < matchedGems.Count; i ++)
		{
			MatchItem gem = matchedGems[i];
			if (verticalMatch)
			{
				rows.Add(gem.point.y);
			}
			else
			{
				columns.Add(gem.point.x);
			}
		}

		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			TilePoint point = matchLogic.tiles[i].point;
			if (verticalMatch)
			{
				if (rows.Contains(point.y) && !destroyGems.Contains(point))
				{
					destroyGems.Add(point);
				}
				if (point.x != gem0.point.x && rows.Contains(point.y) && !undefinedGems.Contains(point))
				{
					undefinedGems.Add(point);
				}
			}
			else
			{
				if (columns.Contains(point.x) && !destroyGems.Contains(point)) {
					destroyGems.Add(point);
				}
				if (point.y != gem0.point.y && columns.Contains(point.x) && !undefinedGems.Contains(point)) {
					undefinedGems.Add(point);
				}
			}
		}

		float delay = 0;
		if (verticalMatch)
		{
			for (int i = 0; i < matchedGems.Count; i++)
			{
				//Logger.Trace(matchedGems[i]);
				MatchItem gem = matchedGems[i];
				float leftDelay = CreateLeftFx(gem);
				delay = leftDelay >= delay ? leftDelay : delay;

				float rightDelay = CreateRightFx(gem);
				delay = rightDelay >= delay ? rightDelay : delay;
			}
		}
		else
		{
			for (int i = 0; i < matchedGems.Count; i++) {
				//Logger.Trace(matchedGems[i]);
				MatchItem gem = matchedGems[i];
				float topDelay = CreateTopFx(gem);
				delay = topDelay >= delay ? topDelay : delay;

				float bottomDelay = CreateBottomFx(gem);
				delay = bottomDelay >= delay ? bottomDelay : delay;
			}
		}
		//Logger.Trace("destroyGems count", destroyGems.Count);
		List<MatchItem> destroyMatchItems = FromPointsToMatchItems(destroyGems);
		matchLogic.NotifyMatchedGems(destroyMatchItems);

		DispatchActionSignals(matchedGems, FromPointsToMatchItems(undefinedGems), Shape.Five);
		delay += 0.5f;
		delayFillEmpty = delayFillEmpty >= delay ? delayFillEmpty : delay;
		return destroyMatchItems;
	}

	public List<MatchItem> CheckMatchTl(List<MatchItem> matchedGems)
	{
		List<int> columns = new List<int>();
		List<int> rows = new List<int>();
		List<TilePoint> tlPoints = new List<TilePoint>();
		List<MatchItem> undefinedGems = new List<MatchItem>();
		for (int i = 0; i < matchedGems.Count; i++)
		{
			MatchItem gem = matchedGems[i];
			tlPoints.Add(gem.point);
			int sameRowGemCount = 0;
			int sameColumnGemCount = 0;
			for (int j = 0; j < matchedGems.Count; j++)
			{
				if (j != i)
				{
					MatchItem gem2 = matchedGems[j];
					if (gem.point.x == gem2.point.x)
					{
						sameColumnGemCount++;
					}
					if (gem.point.y == gem2.point.y)
					{
						sameRowGemCount++;
					}
				}
			}
			//			Logger.Trace("gem ", gem.point.x, " ", gem.point.y, " same col gem count ", sameColumnGemCount, " same row gem count ", sameRowGemCount);
			if (sameColumnGemCount >= 2)
			{
				if (!columns.Contains(gem.point.x))
				{
					columns.Add(gem.point.x);
				}
			}
			if (sameRowGemCount >= 2)
			{
				if (!rows.Contains(gem.point.y))
				{
					rows.Add(gem.point.y);
				}
			}
			//			if (sameColumnGemCount >= 2 || sameRowGemCount >= 2)
			//			{
			//				Logger.Trace(gem.point, " same col gem count ", sameColumnGemCount, " same row gem count ", sameRowGemCount);
			//			}
		}

		List<TilePoint> explosionGems= new List<TilePoint>();
		List<MatchItem> destroyList = new List<MatchItem>();
		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			MatchItem tile = matchLogic.tiles[i];
			if (columns.Contains(tile.point.x))
			{
				destroyList.Add(tile);
				if (!tlPoints.Contains(tile.point))
				{
					undefinedGems.Add(matchLogic.FindTile(tile.point));
				}
				if (rows.Contains(tile.point.y) && !explosionGems.Contains(tile.point))
				{
					explosionGems.Add(tile.point);
				}
			}
			else if (rows.Contains(tile.point.y))
			{
				destroyList.Add(tile);
				if (!tlPoints.Contains(tile.point)) {
					undefinedGems.Add(matchLogic.FindTile(tile.point));
				}
				if (columns.Contains(tile.point.x) && !explosionGems.Contains(tile.point))
				{
					explosionGems.Add(tile.point);
				}
			}
		}
		float delay = 0;
		for (int i = 0; i < explosionGems.Count; i++)
		{
			//Logger.Trace(explosionGems[i]);
			MatchItem gem = matchLogic.FindTile(explosionGems[i]);

			float topDelay = CreateTopFx(gem);
			delay = topDelay >= delay ? topDelay : delay;

			float leftDelay = CreateLeftFx(gem);
			delay = leftDelay >= delay ? leftDelay : delay;

			float bottomDelay = CreateBottomFx(gem);
			delay = bottomDelay >= delay ? bottomDelay : delay;

			float rightDelay = CreateRightFx(gem);
			delay = rightDelay >= delay ? rightDelay : delay;
		}
		matchLogic.NotifyMatchedGems(destroyList);

		DispatchActionSignals(matchedGems, undefinedGems, Shape.Tl);
		delay += 0.5f;
		delayFillEmpty = delayFillEmpty >= delay ? delayFillEmpty : delay;
		return destroyList;
	}

	private float CreateTopFx(MatchItem gem)
	{
		float delay = 0;
		float x = FromColumnToX(gem.point.x);
		float y = FromRowToY(0);
		Vector3 top = new Vector3(x, y + MatchLogic.cellHeight, 0);
		effectsManager.CreateTlFx(gem.transform.localPosition, top, Vector3.zero);
		for (int k = gem.point.y, m = 0; k >= 0; k--, m++) {
			float wait = m * gemWaitTime;
			if (delay < wait) delay = wait;
			matchLogic.routineRunner.StartCoroutine(DelayMatchFx(new TilePoint(gem.point.x, k), wait));
		}
		return delay;
	}

	private float CreateLeftFx(MatchItem gem) {
		float delay = 0;
		float x = FromColumnToX(0);
		float y = FromRowToY(gem.point.y);
		Vector3 left = new Vector3(x - MatchLogic.cellWidth, y, 0);
		effectsManager.CreateTlFx(gem.transform.localPosition, left, new Vector3(0, 0, 90));
		for (int k = gem.point.x, m = 0; k >= 0; k--, m++) {
			float wait = m * gemWaitTime;
			if (delay < wait) delay = wait;
			matchLogic.routineRunner.StartCoroutine(DelayMatchFx(new TilePoint(k, gem.point.y), wait));
		}
		return delay;
	}

	private float CreateBottomFx(MatchItem gem) {
		float delay = 0;
		float x = FromColumnToX(gem.point.x);
		float y = FromRowToY(Data.tileHeight - 1);
		Vector3 bottom = new Vector3(x, y - MatchLogic.cellHeight, 0);
		effectsManager.CreateTlFx(gem.transform.localPosition, bottom, new Vector3(0, 0, 180));
		for (int k = gem.point.y, m = 0; k < Data.tileHeight; k++, m++) {
			float wait = m * gemWaitTime;
			if (delay < wait) delay = wait;
			matchLogic.routineRunner.StartCoroutine(DelayMatchFx(new TilePoint(gem.point.x, k), wait));
		}
		return delay;
	}

	private float CreateRightFx(MatchItem gem) {
		float delay = 0;
		float x = FromColumnToX(Data.tileWidth - 1);
		float y = FromRowToY(gem.point.y);
		Vector3 right = new Vector3(x + MatchLogic.cellWidth, y, 0);
		effectsManager.CreateTlFx(gem.transform.localPosition, right, new Vector3(0, 0, -90));
		for (int k = gem.point.x, m = 0; k < Data.tileWidth; k++, m++) {
			float wait = m * gemWaitTime;
			if (delay < wait) delay = wait;
			matchLogic.routineRunner.StartCoroutine(DelayMatchFx(new TilePoint(k, gem.point.y), wait));
		}
		return delay;
	}

	private float FromRowToY(int row)
	{
		float halfCellHeight = MatchLogic.cellHeight / 2;
		float y = (Data.tileHeight - row) * MatchLogic.cellHeight - halfCellHeight;
		return y;
	}

	private float FromColumnToX(int column)
	{
		float halfCellWidth = MatchLogic.cellWidth / 2;
		float x = column * MatchLogic.cellWidth + halfCellWidth;
		return x;
	}

	private IEnumerator DelayMatchFx(TilePoint point, float delay)
	{
		yield return new WaitForSeconds(delay);
		List<TilePoint> p = new List<TilePoint>();
		p.Add(point);
		CreateMatchFx(p);
	}

	private void DispatchActionSignals(List<MatchItem> tlGems, List<MatchItem> undefinedGems, Shape shape)
	{
		if (matchLogic.dungeonState.playMode != PlayMode.Monster) return;
		matchLogic.actionSignal.Dispatch(new Action(tlGems[0].cell.cellType, shape, tlGems.Count));

		Dictionary<Data.TileTypes, int> undefinedMatches = new Dictionary<Data.TileTypes, int>();
		for (int i = 0; i < undefinedGems.Count; i++)
		{
			MatchItem item = undefinedGems[i];
//			Logger.Trace("matches[item.cell.cellType] ", matches[item.cell.cellType]);
			if (!undefinedMatches.ContainsKey(item.cell.cellType))
			{
				undefinedMatches[item.cell.cellType] = 0;
			}
			undefinedMatches[item.cell.cellType] = ++undefinedMatches[item.cell.cellType];
		}
		for (int i = 0; i < undefinedMatches.Count; i++)
		{
			KeyValuePair<Data.TileTypes, int> pair = undefinedMatches.ElementAt(i);
			Data.TileTypes matchType = pair.Key;
			int matchCount = pair.Value;
//			Logger.Trace("matchType ", matchType, " count ", matchCount);
			matchLogic.actionSignal.Dispatch(new Action(matchType, Shape.Undefined, matchCount));
		}
	}

	public List<MatchItem> CheckMatch3(List<MatchItem> matchedGems) {

		matchLogic.NotifyMatchedGems(matchedGems);
		if (matchLogic.dungeonState.playMode == PlayMode.Monster) {
			matchLogic.DispatchActionSignals(matchedGems);
//			Logger.Trace("matches gem count ", matchedGems.Count);
//			for (int i = 0; i < matchedGems.Count; i++)
//			{
//				Logger.Trace(matchedGems[i].point);
//			}
		}
		CreateMatchFx(matchedGems);

		return matchedGems;
//		CreateMatchFx(matchedGems);
	}

	private void CreateMatchFx(List<TilePoint> partitionedMatches)
	{
		List<MatchItem> matchedGems = new List<MatchItem>();
		for (int i = 0; i < partitionedMatches.Count; i++)
		{
			matchedGems.Add(matchLogic.FindTile(partitionedMatches[i]));
		}
		CreateMatchFx(matchedGems);
	}

	private void CreateMatchFx(List<MatchItem> matchedGems)
	{
		matchLogic.effectsManager.CreateGemFlyEffect(matchedGems);
		foreach (MatchItem item in matchedGems) {
			int type = (int)item.cell.cellType;

//			item.cell.cellType = Data.TileTypes.Empty;
			if(item.cell.cellType == Data.TileTypes.Chest) continue;
			TweenParms parms = new TweenParms();
			parms.Prop("localScale", new Vector3(1.2f, 1.2f, 1)).Ease(EaseType.EaseOutSine);
			HOTween.To(item.transform, 0.1f, parms);
			parms.NewProp("localScale", new Vector3(0, 0, 1)).Ease(EaseType.EaseInSine).Delay(0.1f);
			HOTween.To(item.transform, 0.2f, parms);

			Vector3 position = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y, 0);
			matchLogic.effectsManager.CreateMatchEffect(position);
		}
	}

	private IEnumerator SetEmpty(List<TilePoint> matchedGems, float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < matchedGems.Count; i++)
		{
			MatchItem gem = matchLogic.FindTile(matchedGems[i]);
			if(gem.cell.cellType == Data.TileTypes.Chest) continue;
			gem.cell.cellType = Data.TileTypes.Empty;
		}
		OnSetEmptyFinish();
	}

	private void OnSetEmptyFinish()
	{
		matchLogic.routineRunner.StartCoroutine(matchLogic.FillEmpty(0));
	}

	private List<MatchItem> FromPointsToMatchItems(List<TilePoint> points)
	{
		List<MatchItem> matchItems = new List<MatchItem>();
		for (int i = 0; i < points.Count; i++)
		{
			matchItems.Add(matchLogic.FindTile(points[i]));
		}
		return matchItems;
	}

	private List<TilePoint> FromMatchItemsToPoints(List<MatchItem> matchItems) {
		List<TilePoint> points = new List<TilePoint>();
		for (int i = 0; i < matchItems.Count; i++) {
			points.Add(matchItems[i].point);
		}
		return points;
	}

//	private void DispatchActionSignals(matchedGems)
//	{
//		
//	}
}

public class UnitedMatch
{
	public List<int> rows;
	public List<int> columns;

	public UnitedMatch()
	{
		rows = new List<int>();
		columns = new List<int>();
	}

	public bool PartialyContains(UnitedMatch otherMatch)
	{
		for (int i = 0; i < otherMatch.rows.Count; i++)
		{
			if (rows.Contains(otherMatch.rows[i])) return true;
		}

		for (int i = 0; i < otherMatch.columns.Count; i++)
		{
			if (columns.Contains(otherMatch.columns[i])) return true;
		}

		return false;
	}
}