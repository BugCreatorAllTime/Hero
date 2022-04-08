using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemSetSkill : BaseSkill
{
	public string extrasInfo;

	public override void Active(BattleEntity target)
	{
		Phase = ActionPhase.CASTING;
		this.target = target;
		float duration = PlayPreSkillFx();
		matchLogic.routineRunner.StartCoroutine(DelayedProcessLogicSkill(duration));
	}

	public override void Active()
	{
		Phase = ActionPhase.CASTING;
		float duration = PlayPreSkillFx();
		matchLogic.routineRunner.StartCoroutine(DelayedProcessLogicSkill(duration));
	}

	private IEnumerator DelayedProcessLogicSkill(float delay)
	{
		yield return new WaitForSeconds(delay);
		ProcessLogicSkill();
	}

	protected override void ProcessLogicSkill()
	{
		this.owner.ObjState = ObjectState.Cast;
		this.duration = this.owner.attackDuration;
		//Logger.Trace("cast duration ", duration);
		owner.DoEmptyEnergy();
	}

	protected virtual float PlayPreSkillFx()
	{
		List<Color> colors = new List<Color>();
		string[] colorsFromExtras = GetPreSkillColorsString().Split('-');
		for (int i = 0; i < colorsFromExtras.Length; i++) {
			string[] rgbString = colorsFromExtras[i].Split('.');
			float r = (float)int.Parse(rgbString[0]) / 255;
			float g = (float)int.Parse(rgbString[1]) / 255;
			float b = (float)int.Parse(rgbString[2]) / 255;
			float a = 0.25f;
			Color c = new Color(r, g, b, a);
			colors.Add(c);
		}
		return effectsManager.CreatePreSkillFx(GetPreSkillTexture(), colors);
	}

	protected virtual string GetPreSkillTexture()
	{
		return null;
	}

	protected virtual string GetPreSkillColorsString()
	{
		return null;
	}

	protected override void OnFixedUpdate(float dt) {
		if (this.duration > 0) {
			this.duration -= dt;
			if (duration <= 0) {
				finishCast();
				OnCastFinished();
			}
		}
	}

	protected virtual void OnCastFinished() {
		//Logger.Trace("onCastFinished");
	}

	public virtual void PlayChargeFx(){}
}