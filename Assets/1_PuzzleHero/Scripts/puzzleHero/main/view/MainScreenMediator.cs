using strange.extensions.mediation.impl;
using UnityEngine;

public class MainScreenMediator : Mediator
{
	[Inject]
	public MainScreenHandler mainScreenHandler { get; set; }

	[Inject]
	public MainScreenView mainScreenView { get; set; }

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

		mainScreenView.onClick.AddListener(OnClick);
		mainScreenView.cacheSignal.AddListener(CacheObject);
	}
}