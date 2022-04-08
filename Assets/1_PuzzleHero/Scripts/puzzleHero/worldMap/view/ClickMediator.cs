using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class ClickMediator : Mediator {

	[Inject]
	public ClickDungeonView click{get; set;}
	[Inject]
	public GoToDungeonSignal goToDungeonSignal{get; set;}
	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public CrossContextData crossContextData { get; set; }

	public override void OnRegister ()
	{
		base.OnRegister ();
		click.clickSignal.AddListener(OnClickDungeon);
	}

	void OnClickDungeon(int level){
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		goToDungeonSignal.Dispatch(level,gameObject.name, true);
		UIScrollView scrollView = GameObject.Find("ScrollWorldMap").GetComponent<UIScrollView>();
		crossContextData.lastMapPosition = scrollView.transform.localPosition;
	}
}
