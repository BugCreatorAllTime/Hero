using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using UnityEngine;

public class Hint
{
	private int waitTime;

	private MatchLogic matchLogic;
	private float time = 0;
	private bool startHint = false;
	private MatchItem currentGem;
	private Tweener t1, t2, t3, t4;
	private UISprite choice;

	public Hint(MatchLogic matchLogic, UISprite choice)
	{
		this.matchLogic = matchLogic;
		this.choice = choice;
		waitTime = matchLogic.config.general.HintTime;
	}

	public void DoHint()
	{
		Start();
	}

	public void Unhint()
	{
		Stop();
	}

	private void Animate()
	{
		if (currentGem == null) currentGem = FindHint();
		if (currentGem == null) return;
		//JumpAnimate();
		FadeAnimate();
	}

	private void Start()
	{
		startHint = true;
		matchLogic.routineRunner.StartCoroutine(Loop());
	}

	private void Stop()
	{
		startHint = false;
		time = 0;
		if (currentGem != null)
		{
			//StopJumpAnimate();
			StopFadeAnimate();
			currentGem = null;
		}
	}

	private MatchItem FindHint()
	{
		Dictionary<TilePoint, Data.TileTypes> hints = matchLogic.FindHint(matchLogic.cells);
		if (hints == null) return null;
		TilePoint p = PickRandomly(hints);
		MatchItem gem = matchLogic.FindTile(p);
		return gem;
	}

	private TilePoint PickRandomly(Dictionary<TilePoint, Data.TileTypes> hints)
	{
		int index = UnityEngine.Random.Range(0, hints.Count);
		return hints.ElementAt(index).Key;
	}

	private IEnumerator Loop()
	{
		while (startHint)
		{
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
			if (time >= waitTime)
			{
				time -= waitTime;
				Animate();
			}
		}
	}

	private void JumpAnimate()
	{
		currentGem.GetComponent<UISprite>().depth = 2;
		Vector3 gemPos = currentGem.transform.localPosition;
		Vector3 p = new Vector3(0, 50, 0);
		TweenParms parms = new TweenParms();
		parms.NewProp("localPosition", gemPos + p).Ease(EaseType.EaseOutQuart);
		t1 = HOTween.To(currentGem.transform, 0.1f, parms);
		parms.NewProp("localPosition", gemPos).Ease(EaseType.EaseOutBounce).Delay(0.1f);
		t2 = HOTween.To(currentGem.transform, 0.3f, parms);
		parms.NewProp("localPosition", gemPos + p).Ease(EaseType.EaseOutQuart).Delay(0.6f);
		t3 = HOTween.To(currentGem.transform, 0.1f, parms);
		parms.NewProp("localPosition", gemPos).Ease(EaseType.EaseOutBounce).Delay(0.7f);
		t4 = HOTween.To(currentGem.transform, 0.3f, parms);
	}

	private void StopJumpAnimate()
	{
		if (t1 != null) t1.Kill();
		if (t2 != null) t2.Kill();
		if (t3 != null) t3.Kill();
		if (t4 != null) t4.Kill();
		TilePoint p = currentGem.point;
		Vector3 v = new Vector3(p.x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2,
			(Data.tileHeight - 1 - p.y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2, 0f);
		currentGem.transform.localPosition = v;
		currentGem.GetComponent<UISprite>().depth = 1;
	}

	private void FadeAnimate()
	{
		choice.transform.localPosition = currentGem.transform.localPosition;
		choice.enabled = true;
		choice.GetComponent<TweenAlpha>().to = 0.5f;
	}

	private void StopFadeAnimate()
	{
		choice.enabled = false;
		choice.GetComponent<TweenAlpha>().to = 1f;
	}
}