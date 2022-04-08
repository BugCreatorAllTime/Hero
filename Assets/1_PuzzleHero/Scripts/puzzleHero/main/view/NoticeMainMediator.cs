using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class NoticeMainMediator : Mediator {

	[Inject]
	public MainScreenHandler mainScreenHandler { get; set; }
	
	[Inject]
	public NoticeMainView noticeView { get; set; }
	
	protected void OnClick(string buttonName) {
		//		Logger.Trace("click");
		mainScreenHandler.HandleClick(buttonName);
	}
	
	protected void CacheObject(GameObject go) {
		mainScreenHandler.CacheObject(go);
	}
	
	public override void OnRegister()
	{
		base.OnRegister();
		
		noticeView.onClick.AddListener(OnClick);
		noticeView.cacheSignal.AddListener(CacheObject);
	}

}
