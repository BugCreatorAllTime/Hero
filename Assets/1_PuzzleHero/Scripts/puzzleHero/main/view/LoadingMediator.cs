using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class LoadingMediator : Mediator {

	[Inject]
	public MainScreenHandler mainScreenHandler { get; set; }
	
	[Inject]
	public LoadingView loadingView { get; set; }
	
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
		
		loadingView.onClick.AddListener(OnClick);
		loadingView.cacheSignal.AddListener(CacheObject);
	}
}
