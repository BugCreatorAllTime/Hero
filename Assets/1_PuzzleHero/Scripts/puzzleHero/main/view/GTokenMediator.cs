using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class GTokenMediator : Mediator {

	[Inject]
	public MainScreenHandler mainScreenHandler { get; set; }
	
	[Inject]
	public GTokenView gTokenView { get; set; }

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
		
		gTokenView.onClick.AddListener(OnClick);
		gTokenView.cacheSignal.AddListener(CacheObject);
	}

}
