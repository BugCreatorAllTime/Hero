using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using UnityEngine;
using strange.extensions.pool.api;
using System.Collections.Generic;
using strange.examples.strangerocks;
using System.Collections;

public class TextFxManager {

	[Inject(DungeonContext.TEXT)]
	public GameObject board { get; set; }

	[Inject(Prefabs.text)]
	public IPool<GameObject> textPool { get; set; }

	[Inject(Prefabs.talk)]
	public IPool<GameObject> talkPool { get; set; }

	[Inject(Prefabs.textString)]
	public IPool<GameObject> textStringPool { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public ConfigManager config { get; set;}

	private Color32 cHpTop = new Color32 (255,4,9,255);
	private Color32 addHpTop = new Color32 (4,255,57,255);
	private Color32 mHpTop = new Color32 (255,0,227,255);
	private Color32 cDefTop = new Color32 (53,26,255,255);
	private Color32 cManaTop = new Color32(135, 0, 255, 255);
	private Color32 cGoldTop = new Color32 (255,237,0,255);

	private Color32 cHpBottom = new Color32 (255,174,0,255);
	private Color32 addHpBottom = new Color32 (255,248,0,255);
	private Color32 mHpBottom = new Color32 (21,0,30,255);
	private Color32 cDefBottom = new Color32 (0,213,255,255);
	private Color32 cManaBottom = new Color32(255, 43, 170, 255);
	private Color32 cGoldBottom = new Color32 (255,149,53,255);

	private int goldCollect;
	private bool showGold =  false;
	private List<int> goldList = new List<int>();
	private int countWarning = 0;

	[PostConstruct]
	public void PostConstruct()
	{

	}

	public void ShowText(int value, TextState textState, int team)
	{
		string textShow = null;
		GameObject textObject = CreatePoolGameObject (textPool);
		UILabel fxText = textObject.GetComponent<UILabel> ();
		fxText.transform.parent = board.transform;
		textObject.transform.GetChild (0).gameObject.SetActive (false);
		fxText.depth = 23;
		Vector3 target = Vector3.zero;
		bool monPos = false;
		switch(textState)
		{
			case TextState.HpAdd:
				textShow = "+"+value;
				if(team == BattleGameLogic.TEAM2)
				{
					fxText.gradientTop = mHpTop;
					fxText.gradientBottom = mHpBottom;
					monPos = true;
				}
				else {
					fxText.gradientTop = addHpTop;
					fxText.gradientBottom = addHpBottom;
					fxText.transform.localPosition = new Vector3(-100,180,0);
					target = new Vector3(-100,350,0);
				}
				break;
			case TextState.HpSub:
				textShow = "-"+value;
				if(team == BattleGameLogic.TEAM1)
				{
					fxText.gradientTop = mHpTop;
					fxText.gradientBottom = mHpBottom;
					monPos = true;
				}
				else 
				{
					fxText.gradientTop = cHpTop;
					fxText.gradientBottom = cHpBottom;
					fxText.transform.localPosition = new Vector3(-100,180,0);
					target = new Vector3(-100,350,0);
				}
				break;
			case TextState.Def:
				textShow = "+"+value;
				fxText.gradientTop = cDefTop;
				fxText.gradientBottom = cDefBottom;
				fxText.transform.localPosition = new Vector3(-250,325,0);
				target = new Vector3(-250,420,0);
				break;
			case TextState.Mana:
				textShow = "+"+value;
				fxText.gradientTop = cManaTop;
				fxText.gradientBottom = cManaBottom;
				fxText.transform.localPosition = new Vector3(-80,315,0);
				target = new Vector3(-80,400,0);
				break;
			default:
				break;
		}
		fxText.text = textShow;
		MoveText (fxText, monPos, target, 1.25f);
		
	}

	public void NoMoreMatches(TweenDelegate.TweenCallback callback)
	{
		GameObject text = CreatePoolGameObject(textStringPool);
		text.SetActive(true);
		UILabel label = text.GetComponent<UILabel>();
		label.text = config.text.NoMoreMatches;
		label.transform.parent = board.transform;
		label.transform.localScale = Vector3.one;
		label.depth = 23;
		label.width = 550;
		int fontSize = label.fontSize;
		label.fontSize = 60;
		label.color = Color.white;
		label.transform.localPosition = new Vector3(700, 250, 0);
		Vector3 p1 = new Vector3(0, 250, 0);
		Vector3 p2 = new Vector3(-700, 250, 0);
//		Logger.Trace("begin time", Time.time);
		TweenParms parms = new TweenParms();
		parms.NewProp("localPosition", p1).Ease(EaseType.EaseOutBounce);
		HOTween.To(label.transform, 0.6f, parms);
		parms.NewProp("localPosition", p2).Delay(1.6f).Ease(EaseType.EaseOutBack).OnComplete(delegate()
		{
//			Logger.Trace("end time", Time.time);
			if (callback != null)
			{
				callback();
			}
			label.fontSize = fontSize;
			textStringPool.ReturnInstance(text);
			text.SetActive(false);
		});
		HOTween.To(label.transform, 0.4f, parms);
	}

	void MoveText(UILabel fxText, bool monPos, Vector3 target, float mScale)
	{
		TweenAlpha tweenAlpha;
		tweenAlpha = UITweener.Begin<TweenAlpha>(fxText.gameObject, 1.5f);
		tweenAlpha.from = 1;
		tweenAlpha.to = 0;
		
		TweenParms parms = new TweenParms();
		fxText.transform.localScale = new Vector3(mScale,mScale,mScale);
		parms.Prop ("localScale", new Vector3 (mScale*0.6f, mScale*0.6f, mScale*0.6f)).Ease (EaseType.EaseOutSine);
		HOTween.To(fxText.transform, 1f, parms);
		
		if (!monPos)
		{
			TweenParms parms2 = new TweenParms();
			parms2.Prop ("localPosition", target).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnInstance), fxText.gameObject);
			HOTween.To(fxText.transform, 1.5f, parms2);
		}
		else
		{
			TweenParms parms3 = new TweenParms();
			fxText.transform.localPosition = new Vector3(50,180,0);
			parms3.Prop ("localPosition", new Vector3 (50f, 350, 0f)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallbackWParms(ReturnInstance), fxText.gameObject);
			HOTween.To(fxText.transform, 1.5f, parms3);
		}
	}

	public void AddGoldList()
	{
		goldList.Add (goldCollect);
		routineRunner.StartCoroutine (StartShowGold ());
		StartShowGold ();
	}

	IEnumerator StartShowGold()
	{
		if(!showGold)
		{
			showGold = true;
			while(goldList.Count > 0)
			{
				ShowTextGold(goldList[0]);
				yield return new WaitForSeconds(0.3f);
				goldList.RemoveAt(0);
				if(goldList.Count == 0) showGold = false;
			}
		}
	}

	public void ShowTextGold(int value)
	{
		string textShow = "+"+value;
		GameObject textObject = CreatePoolGameObject (textPool);
		textObject.transform.GetChild (0).gameObject.SetActive (true);
		UILabel fxText = textObject.GetComponent<UILabel> ();
		fxText.transform.parent = board.transform;
		fxText.depth = 23;
		fxText.gradientTop = cGoldTop;
		fxText.gradientBottom = cGoldBottom;
		fxText.text = textShow;
		fxText.transform.localPosition = new Vector3(-70,180,0);
		Vector3 target = new Vector3(-70,320,0);
		MoveText (fxText, false, target, 0.8f);
	}

	public void ShowTextCombo(int combo)
	{
		bool showText = false;
		TextComboData data = null;
		if(config.comboCfg.combo.ContainsKey(combo.ToString()))
		{
			showText = true;
			data = config.comboCfg.combo[combo.ToString()];
		}
		if(combo > config.comboCfg.combo.Count)
		{
			showText = true;
			data = config.comboCfg.combo[config.comboCfg.combo.Count.ToString()];
		}
		if(showText)
		{
			string textShow = data.Text;
			GameObject textObject = CreatePoolGameObject (textStringPool);
			UILabel fxText = textObject.GetComponent<UILabel> ();
			fxText.depth = 23;
			fxText.alpha = 1;
			Color32 colorTop = new Color32((byte)data.ColorTop[0],(byte)data.ColorTop[1],(byte)data.ColorTop[2],(byte)data.ColorTop[3]);
			Color32 colorBottom = new Color32((byte)data.ColorBottom[0],(byte)data.ColorBottom[1],(byte)data.ColorBottom[2],(byte)data.ColorBottom[3]);
			fxText.gradientTop = colorTop;
			fxText.gradientBottom = colorBottom;
			fxText.text = textShow;
			fxText.transform.parent = board.transform;
			fxText.transform.localScale = Vector3.one * (float)data.ScaleMax;
			fxText.transform.localPosition = new Vector3(0,-100,0);
			routineRunner.StartCoroutine(ProgressCombo (fxText, (float)data.ScaleMax, (float)data.ScaleMin));
		}
	}

	IEnumerator ProgressCombo(UILabel fxText, float maxSize, float minSize)
	{
		TweenParms parms = new TweenParms();
		fxText.transform.localScale = new Vector3(maxSize, maxSize, maxSize);
		parms.Prop ("localScale", new Vector3 (minSize, minSize, minSize)).Ease (EaseType.EaseOutSine);
		HOTween.To(fxText.transform, 0.9f, parms);

		yield return new WaitForSeconds(0.9f);
		fxText.gameObject.SetActive (false);
		textStringPool.ReturnInstance(fxText.gameObject);

	}

	public void SetGold(int collectGold, int sum)
	{
		this.goldCollect = collectGold/sum;
	}

	public void Talk(string text, bool me, float duration)
	{
		text = text.Replace(";",",");
		GameObject textObject = CreatePoolGameObject (talkPool);
		UISprite talkImg = textObject.GetComponentInChildren<UISprite> ();
		if(me)
		{
			talkImg.flip = UIBasicSprite.Flip.Horizontally;
			talkImg.transform.localPosition = new Vector3(-50,350,0);
		} else {
			talkImg.flip = UIBasicSprite.Flip.Nothing;
			talkImg.transform.localPosition = new Vector3(50,350,0);
		}
		UILabel fxText = textObject.GetComponentInChildren<UILabel> ();
		textObject.transform.parent = board.transform;
		textObject.transform.localScale = Vector3.one;
		fxText.text = text;
		routineRunner.StartCoroutine (HideTalk (textObject, talkPool, duration));
	}

	IEnumerator HideTalk(GameObject go, IPool<GameObject> pool, float duration)
	{
		yield return new WaitForSeconds(duration - 0.5f);
		go.SetActive (false);
		pool.ReturnInstance(go);
	}

	public void Waring()
	{
		countWarning = 0;
		string textShow = "BOSS!";
		GameObject textObject = CreatePoolGameObject (textStringPool);
		UILabel fxText = textObject.GetComponent<UILabel> ();
		fxText.transform.parent = board.transform;
		fxText.depth = 23;
		fxText.gradientTop = Color.red;
		fxText.gradientBottom = Color.red;
		fxText.text = textShow;
		fxText.transform.localPosition = new Vector3(0,300,0);
		fxText.transform.localScale = Vector3.one;
		routineRunner.StartCoroutine (ProgressWarning (textObject));
	}

	IEnumerator ProgressWarning(GameObject go)
	{
		float start = 0.2f, end = 1f;
		if(countWarning%2==0)
		{
			start = 0.2f;
			end = 1f;
		} else {
			start = 1f;
			end = 0.2f;
		}
		TweenAlpha tweenAlpha;
		tweenAlpha = UITweener.Begin<TweenAlpha>(go, 0.8f);
		tweenAlpha.from = start;
		tweenAlpha.to = end;
		countWarning++;
		yield return new WaitForSeconds(0.8f);
		if(countWarning < 4)
			routineRunner.StartCoroutine (ProgressWarning (go));
		else {
			go.SetActive (false);
			textStringPool.ReturnInstance(go);
		}
	}

	GameObject CreatePoolGameObject(IPool<GameObject> gamePool)
	{
		GameObject contentObject = gamePool.GetInstance();
		contentObject.SetActive(true);
		return contentObject;
	}

	void ReturnInstance(TweenEvent tweenEvent)
	{
		GameObject gObject = (GameObject)tweenEvent.parms [0];
		gObject.SetActive (false);
		textPool.ReturnInstance(gObject);
	}

}
