using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
using strange.examples.strangerocks;
using LitJson;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;

public class ClickBackHomeTownMediator : Mediator {

	[Inject]
	public ClickBackHomeTownView click{get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public AssetMgr mgr{get; set;}

	[Inject]
	public ConfigManager config { get; set;}

	private UIScrollView scroll;
	private bool backHome;
	private int posCurentDungeon;
	private const int posCheck = -400;
	private const int maxPos = 5760;
	private const string backHomeIcon = "HomeIcon";
	private const string goCurDungeonIcon = "FightIcon";
	private UISprite btBackHome;

	[PostConstruct]
	public void PostConstruct()
	{
		scroll = GameObject.Find ("ScrollWorldMap").GetComponent<UIScrollView>(); 
		btBackHome = GameObject.Find ("IconButtonBackHome").GetComponent<UISprite>(); 
		CheckBackHome ();
		mgr.GetAsset<TextAsset>("Config/WorldMapCfg.json", LoadData);
		scroll.onDragFinished += new UIScrollView.OnDragFinished(OnDragFinished);
	}

	private void LoadData(TextAsset ta)
	{
		string jsonText = ta.text;
		JsonData data = JsonMapper.ToObject(jsonText);
		int numberOfDungeon = (int)data["dungeon"].Count;
		
		for (int i = 0; i < numberOfDungeon; i++)
		{
			int piece = (int)data["dungeon"][i]["piece"];
			int level = (int)data["dungeon"][i]["level"];
			int posY = (int)data["dungeon"][i]["y"];
			if(config.UserData.curMapId == level)
			{
				posCurentDungeon = posY;
				if(posCurentDungeon > maxPos) posCurentDungeon = maxPos;
				break;
			}
		}
	}

	private void OnDragFinished()
	{
		CheckBackHome ();
	}

	private void CheckBackHome()
	{
		if (scroll.transform.localPosition.y > posCheck)
		{
			backHome = false;
			btBackHome.spriteName = goCurDungeonIcon;
		}
		else
		{
			backHome = true;
			btBackHome.spriteName = backHomeIcon;
		}
		btBackHome.MakePixelPerfect ();
	}

	public override void OnRegister ()
	{
		base.OnRegister ();
		click.clickSignal.AddListener(OnClickBackHomeTown);
	}

	void OnClickBackHomeTown(){
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		if(backHome)
			BackHome();
		else GoCurDungeon ();
	}

	private void BackHome() 
	{
		float timeScroll = -scroll.transform.localPosition.y / maxPos;
		if(timeScroll < 0.15f) timeScroll = 0.15f; 
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (0, 0, 0f)).Ease (EaseType.EaseOutSine);
		HOTween.To(scroll.transform, timeScroll, parms);
		
		UIPanel panel = scroll.transform.GetComponent<UIPanel>();
		TweenParms parms2 = new TweenParms();
		parms2.Prop ("clipOffset", new Vector2 (0, 0)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(CheckBackHome));
		HOTween.To(panel, timeScroll, parms2);
	}

	private void GoCurDungeon() 
	{
		float timeScroll = posCurentDungeon / maxPos;
		if(timeScroll < 0.15f) timeScroll = 0.15f; 
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3 (scroll.transform.localPosition.x, -posCurentDungeon, 0f)).Ease (EaseType.EaseOutSine);
		HOTween.To(scroll.transform, timeScroll, parms);

		UIPanel panel = scroll.transform.GetComponent<UIPanel>();
		TweenParms parms2 = new TweenParms();
		parms2.Prop ("clipOffset", new Vector2 (0, posCurentDungeon)).Ease (EaseType.EaseOutSine).OnComplete (new TweenDelegate.TweenCallback(CheckBackHome));
		HOTween.To(panel, timeScroll, parms2);
	}
}
