using strange.extensions.signal.impl;
using UnityEngine;

public class IngameBaseView : BaseView
{
	public Signal<string> onClick;
	public Signal<GameObject> cacheSignal;

	[PostConstruct]
	public virtual void Init() {
		onClick = new Signal<string>();
		cacheSignal = new Signal<GameObject>();
	}

	protected override void OnStart()
	{
		Setup ();
	}

	public virtual void Setup(){}

	private void OnButtonClick(GameObject go) {
		//		Logger.Trace(go.name);
		//		Logger.Trace(parms.GetHashCode().ToString());
		onClick.Dispatch(go.name);
	}

	protected void SetupButton(UIButton button) {
		EventDelegate eventDelegate = new EventDelegate(this, "OnButtonClick");
		EventDelegate.Parameter parms = new EventDelegate.Parameter(button.gameObject, "go");
		eventDelegate.parameters[0] = parms;
		button.onClick.Add(eventDelegate);
		CacheObjectForEventHandling(button.gameObject);
	}

	protected void CacheObjectForEventHandling(GameObject objectToCache) {
		cacheSignal.Dispatch(objectToCache);
	}
}