using strange.extensions.mediation.impl;
using UnityEngine;

public class IngameBaseMediator : Mediator
{
	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	protected void OnClick(string buttonName) {
		//		Logger.Trace("click");
		guiEventHandler.HandleClick(buttonName);
	}

	protected void CacheObject(GameObject go) {
		guiEventHandler.CacheObject(go);
	}
}