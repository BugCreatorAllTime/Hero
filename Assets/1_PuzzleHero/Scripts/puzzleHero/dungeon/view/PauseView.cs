using strange.extensions.signal.impl;
using UnityEngine;

public class PauseView : BaseView
{
	public UIButton Resume { get; set; }

	[NGUITag]
	public UIButton Surrender { get; set; }

	[Inject]
	public ConfigManager config {get;set;}

	public Signal<string> onClick;
	public Signal<GameObject> cacheSignal;
	public UISlider musicSlider;
	public UISlider soundSlider;

	public Signal<UISlider> onChange = new Signal<UISlider>();

	public void Init()
	{
//		Logger.Trace("PView init");
		onClick = new Signal<string>();
		cacheSignal = new Signal<GameObject>();
	}

	public void Setup()
	{
		GameObject pauseBoard = GameObject.Find(GuiObjectName.pauseBoard);
		CacheObjectForEventHandling(pauseBoard);
		GameObject pauseSurrender = GameObject.Find(GuiObjectName.pauseSurrender);
		CacheObjectForEventHandling(pauseSurrender);

		Resume = pauseBoard.transform.Find(GuiObjectName.resume).GetComponent<UIButton>();
		SetupButton(Resume);

		UIButton surrender = pauseBoard.transform.Find(GuiObjectName.surrender).GetComponent<UIButton>();
		SetupButton(surrender);

		UIButton confirmResume = GameObject.Find(GuiObjectName.confirmResume).GetComponent<UIButton>();
		SetupButton(confirmResume);

		UIButton confirmSurrender = GameObject.Find((GuiObjectName.confirmSurrender)).GetComponent<UIButton>();
		SetupButton(confirmSurrender);

		musicSlider = GameObject.Find(GuiObjectName.musicSlider).GetComponent<UISlider>();
		musicSlider.value = (float)config.UserData.musicValue;
		SetupSlider(musicSlider);
		CacheObjectForEventHandling(musicSlider.gameObject);
		
		soundSlider = GameObject.Find(GuiObjectName.soundSlider).GetComponent<UISlider>();
		soundSlider.value = (float)config.UserData.soundValue;
		SetupSlider(soundSlider);
		CacheObjectForEventHandling(soundSlider.gameObject);

		gameObject.SetActive(false);
	}

	private void OnClick(GameObject go /*, Object parms*/)
	{
//		Logger.Trace(go.name);
//		Logger.Trace(parms.GetHashCode().ToString());
		onClick.Dispatch(go.name);
	}

	private void SetupButton(UIButton button)
	{
		EventDelegate eventDelegate = new EventDelegate(this, "OnClick");
		EventDelegate.Parameter parms = new EventDelegate.Parameter(button.gameObject, "go");
		eventDelegate.parameters[0] = parms;
		button.onClick.Add(eventDelegate);
		CacheObjectForEventHandling(button.gameObject);
	}

	private void CacheObjectForEventHandling(GameObject objectToCache)
	{
		cacheSignal.Dispatch(objectToCache);
	}

	private void OnChange(UISlider slider)
	{
		onChange.Dispatch(slider);
	}

	private void SetupSlider(UISlider slider)
	{
		EventDelegate eventDelegate = new EventDelegate(this, "OnChange");
		EventDelegate.Parameter parms = new EventDelegate.Parameter(slider, "slider");
		eventDelegate.parameters[0] = parms;
		slider.onChange.Add(eventDelegate);
	}
}