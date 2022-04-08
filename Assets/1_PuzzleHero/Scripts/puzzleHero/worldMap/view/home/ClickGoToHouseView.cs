using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;
using strange.extensions.mediation.impl;
using strange.examples.strangerocks;

public class ClickGoToHouseView : View {

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public SoundManager soundMng { get; set;}

	private float defDPI = 300f;
	internal Signal<string> clickSignal = new Signal<string>();
	public UIScrollView scrollView;

	protected override void Awake ()
	{
		defDPI = Screen.dpi == 0? defDPI: Screen.dpi*1.5f;
//		UIEventListener.Get(GetComponent<UIButton>().gameObject).onDrag += DragButton;
		UIEventListener.Get(GetComponent<UIButton>().gameObject).onPress += PressButton;
		if(scrollView == null)
		{
			scrollView = gameObject.transform.parent.GetComponent<UIScrollView> ();
		}
	}

	public void Click(){
		soundMng.PlaySound (SoundName.BUTTON_CLICK);
		routineRunner.StartCoroutine(ProgressClick());
	}

	IEnumerator ProgressClick()
	{
		yield return new WaitForSeconds(0.15f);
		clickSignal.Dispatch(transform.name);
	}

	void DragButton (GameObject go, Vector2 delta)
	{
		if(scrollView != null && config.UserData.currentStepTownTutorial != TutorialFirstBattleLogic.CLICK_SHOP)
		{
			scrollView.Scroll (-delta.y/defDPI);
		}
	}

	void PressButton(GameObject go, bool isPressed)
	{
		if(isPressed)
		{
			gameObject.transform.localScale = new Vector3(1.25f,1.25f,1.25f);
		} else {
			gameObject.transform.localScale = Vector3.one;
		}
	}
}
