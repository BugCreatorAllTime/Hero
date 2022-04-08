using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

public class HudView : BaseView
{
	public UISlider CharacterHpBar { get; set; }
	public UISlider DefendBar { get; set; }
	public UISlider MonsterHpBar { get; set; }
	public UISlider SkillBar { get; set; }
	public UILabel TurnsToAttackLabel { get; set; }
	public GameObject Skull { get; set; }
	public Signal<string> onClick;

	private UIButton pauseButton;
	private UILabel remainingMoves;

	public void Init()
	{
		onClick = new Signal<string>();
		
		CharacterHpBar = GameObject.Find(GuiObjectName.characterHpBar).GetComponent<UISlider>();
		DefendBar = GameObject.Find(GuiObjectName.defendBar).GetComponent<UISlider>();
		MonsterHpBar = GameObject.Find(GuiObjectName.monsterHpBar).GetComponent<UISlider>();
		SkillBar = GameObject.Find(GuiObjectName.skillBar).GetComponent<UISlider>();
		TurnsToAttackLabel = GameObject.Find(GuiObjectName.turnsToAttackLabel).GetComponent<UILabel>();
		pauseButton = GameObject.Find(GuiObjectName.pauseButtonName).GetComponent<UIButton>();
		Skull = GameObject.Find(GuiObjectName.skull);

		EventDelegate eventDelegate = new EventDelegate(this, "OnClick");
		eventDelegate.parameters[0] = new EventDelegate.Parameter(pauseButton.gameObject, "go");
		pauseButton.onClick.Add(eventDelegate);

		remainingMoves = GameObject.Find("RemainingMoves").GetComponent<UILabel>();
		remainingMoves.gameObject.SetActive(false);
	}

	public void SetRemainingMoves(string text)
	{
		remainingMoves.gameObject.SetActive(true);
		remainingMoves.text = text;
		TweenAlpha tweenAlpha;
		tweenAlpha = UITweener.Begin<TweenAlpha>(remainingMoves.gameObject, 0.5f);
		tweenAlpha.from = 0;
		tweenAlpha.to = 1;
	}

	public void OnCharacterHpChanged(int currentHp, int maxHp) {
		SliderTween("value", CharacterHpBar, (float) currentHp / (float) maxHp, EaseType.Linear, 0.2f);
	}

	public void OnMonsterHpChanged(int currentHp, int maxHp)
	{
		SliderTween("value", MonsterHpBar, (float)currentHp / (float)maxHp, EaseType.Linear, 0.2f);
	}

	public void OnDefendChanged(int currentDef, int maxDef)
	{
		SliderTween("value", DefendBar, (float) currentDef / (float) maxDef, EaseType.Linear, 0.2f);
	}

	public void OnCharacterSkillChanged(float currentEnergy)
	{
//		Logger.Trace("update energy ", currentEnergy);
		SliderTween("value", SkillBar, currentEnergy, EaseType.Linear, 0.2f);
	}

	public void OnMonsterTurnChanged(int turn)
	{
		TweenColor tColor = UITweener.Begin<TweenColor>(Skull, 0.5f);
		tColor.duration = 1;
		tColor.to = Color.red;
		tColor.style= UITweener.Style.Once;
		tColor.animationCurve = new AnimationCurve(new[] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0)});
		EventDelegate.Set(tColor.onFinished, delegate () { OnTweenColorComplete(turn, tColor.GetHashCode()); });
	}

	private void OnTweenColorComplete(int turn, int hashcode)
	{
		TurnsToAttackLabel.text = turn.ToString();
	}

	public void OnClick(GameObject go)
	{
		onClick.Dispatch(go.name);
	}

	private void SliderTween(string propertyName, object obj, object endValue, EaseType easeType, float duration)
	{
		TweenParms parms = new TweenParms();
		parms.Prop(propertyName, endValue).Ease(easeType);
		HOTween.To(obj, duration, parms);
	}
}