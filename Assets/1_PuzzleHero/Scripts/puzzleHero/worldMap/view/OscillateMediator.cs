using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using strange.examples.strangerocks;

public class OscillateMediator : Mediator {

	[Inject]
	public OscillateView oscillateView { get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	private Vector3 pos;
	private UISprite image;

	public override void OnRegister ()
	{
		base.OnRegister ();
		image = this.gameObject.GetComponentInChildren<UISprite>();
		switch(oscillateView.state)
		{
			case OscillateView.VERTICALLY:
				VerticallyUp ();
				break;
			case OscillateView.HORIZONTAL:
				SetRandomHorizontal();
				break;
			case OscillateView.FLASH:
//				routineRunner.StartCoroutine (AlphaChange (0.75f, 1.25));
			ScaleUp();
				break;
			default:
				break;
		}

	}

	void VerticallyUp()
	{
		if(oscillateView.start)
		{
			pos = oscillateView.pos;
			oscillateView.start = false;
			gameObject.transform.localPosition = pos;
		}

		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (pos.x, pos.y+15, pos.z)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(VerticallyDown));
		HOTween.To(gameObject.transform, 0.5f, parms);
	}

	void VerticallyDown()
	{
		if(oscillateView.start)
		{
			pos = oscillateView.pos;
			oscillateView.start = false;
			gameObject.transform.localPosition = pos;
		}
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (pos.x, pos.y-15, pos.z)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(VerticallyUp));
		HOTween.To(gameObject.transform, 0.5f, parms);
	}

	void HorizontalLeft()
	{
		image.flip = UIBasicSprite.Flip.Nothing;
		int y = CalY (oscillateView.index, oscillateView.numerical);
		pos = new Vector3 (-550,y,0);
		int addTime = Random.Range (5,45);
		gameObject.transform.localPosition = pos;
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (550, y+500, pos.z)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(SetRandomHorizontal));
		HOTween.To(gameObject.transform, 5f+addTime, parms);
	}

	void HorizontalRight()
	{
		image.flip = UIBasicSprite.Flip.Horizontally;
		int y = CalY (oscillateView.index, oscillateView.numerical);
		pos = new Vector3 (550,y,0);
		int addTime = Random.Range (5,45);
		gameObject.transform.localPosition = pos;
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (-550, y+400, pos.z)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(SetRandomHorizontal));
		HOTween.To(gameObject.transform, 5f+ addTime, parms);
	}

	void ScaleUp()
	{
		gameObject.transform.localScale = Vector3.one * 0.85f;
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", Vector3.one*1f).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(ScaleDown));
		HOTween.To(gameObject.transform, 0.5f, parms);
	}

	void ScaleDown()
	{
		gameObject.transform.localScale = Vector3.one * 1f;
		TweenParms parms = new TweenParms();
		parms.Prop ("localScale", Vector3.one*0.85f).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(ScaleUp));
		HOTween.To(gameObject.transform, 0.5f, parms);
	}

	int CalY(int index, int numerical)
	{
		return Random.Range (960*oscillateView.index-320,960*oscillateView.index+200);
	}

	void SetRandomHorizontal()
	{
		int scale = Random.Range (0,10);
		gameObject.transform.localScale = Vector3.one * (1 + (float)scale / 10);
		if(oscillateView.start)
		{
			HorizontalLeft();
		} else {
			HorizontalRight();
		}
	}
}
