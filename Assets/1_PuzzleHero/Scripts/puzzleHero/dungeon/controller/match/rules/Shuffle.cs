using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Holoville.HOTween;
using UnityEngine;

public class Shuffle
{
	public const float totalShufflingTime = 0.8f;

	public delegate void OnShufflingFinish();

	public event OnShufflingFinish OnShufflingFinishEvent;

	private MatchLogic matchLogic;

	public Shuffle(MatchLogic matchLogic)
	{
		this.matchLogic = matchLogic;
	}

	public void DoShuffle()
	{
		matchLogic.DisableBoardInput();
		matchLogic.SetBoardStatic(false);
		ShuffleOut();
		matchLogic.routineRunner.StartCoroutine(DoShuffle(0.2f));
		matchLogic.routineRunner.StartCoroutine(ShuffleIn(0.35f));
	}

	private void ShuffleOut()
	{
		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			TweenParms parms = new TweenParms().Prop("localScale", Vector3.zero).Ease(EaseType.EaseOutSine);
			HOTween.To(matchLogic.tiles[i].transform, 0.2f, parms);
		}
	}

	private IEnumerator DoShuffle(float delay)
	{
		yield return new WaitForSeconds(delay);
		while (true)
		{
			RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
			int n = matchLogic.tiles.Count;
			while (n > 1)
			{
				byte[] box = new byte[1];
				do provider.GetBytes(box); while (!(box[0] < n * (Byte.MaxValue / n)));
				int k = (box[0] % n);
				n--;
				MatchItem value = matchLogic.tiles[k];
				matchLogic.tiles[k] = matchLogic.tiles[n];
				matchLogic.tiles[n] = value;
			}

			for (int x = 0; x < Data.tileWidth; x++)
			{
				for (int y = 0; y < Data.tileHeight; y++)
				{
					MatchItem item = matchLogic.tiles[x * Data.tileHeight + y];
					matchLogic.cells[x, y] = item.cell;
					item.point = new TilePoint(x, y);
					item.transform.localPosition = new Vector3(x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2,
						(Data.tileHeight - 1 - y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2, 0f);
				}
			}
			Dictionary<TilePoint, Data.TileTypes> stack = matchLogic.FindMatch(matchLogic.cells);
			if (stack.Count < 1) break;
		}
	}

	private IEnumerator ShuffleIn(float delay)
	{
		yield return new WaitForSeconds(delay);
		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			TweenParms parms = new TweenParms().Prop("localScale", MatchLogic.cellScale).Ease(EaseType.EaseOutSine);
			HOTween.To(matchLogic.tiles[i].transform, 0.3f, parms);
		}
		matchLogic.routineRunner.StartCoroutine(FinishShuffling(0.3f));
	}

	private IEnumerator FinishShuffling(float delay)
	{
		yield return new WaitForSeconds(delay);
		matchLogic.EnableBoardInput();
		matchLogic.SetBoardStatic(true);
		if (OnShufflingFinishEvent != null)
		{
			OnShufflingFinishEvent();
		}
	}
}