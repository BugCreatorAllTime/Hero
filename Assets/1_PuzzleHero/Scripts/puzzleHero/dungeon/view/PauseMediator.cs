using strange.extensions.mediation.impl;
using UnityEngine;

public class PauseMediator : Mediator
{
	[Inject]
	public PauseView pauseView { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	public override void OnRegister()
	{
		pauseView.Init();
		pauseView.onClick.AddListener(OnClick);
		pauseView.cacheSignal.AddListener(CacheObject);
		pauseView.onChange.AddListener(OnChange);
		pauseView.Setup();
	}

	private void OnClick(string buttonName)
	{
//		Logger.Trace("click");
		guiEventHandler.HandleClick(buttonName);
	}

	private void OnChange(UISlider slider)
	{
		guiEventHandler.HandleValueChange(slider);
	}

	private void CacheObject(GameObject go)
	{
		guiEventHandler.CacheObject(go);
	}
}