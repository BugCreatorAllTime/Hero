using UnityEngine;
using AnimationState = Spine.AnimationState;

public class OneTimeSkill : ItemSetSkill
{
	protected float castFxDuration = -1;

	protected override void OnDestroy()
	{
	}

	protected override void OnAdd()
	{
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		PlayCastFx();
	}

	protected override void OnFixedUpdate(float dt)
	{
		base.OnFixedUpdate(dt);

		if (castFxDuration > 0)
		{
			castFxDuration -= dt;
			if (castFxDuration <= 0)
			{
				OnCastFxEnd();
				end();
			}
		}
	}

	protected virtual void PlayCastFx()
	{
		//Logger.Trace(this.GetType(), "play cast fx");
		GameObject fxObject;
		OnPlayCastFx(out fxObject, out castFxDuration);
		if (fxObject == null)
		{
			end();
		}
	}

	protected virtual void OnPlayCastFx(out GameObject go, out float duration)
	{
		go = null;
		duration = -1;
	}

	protected virtual void OnCastFxEnd()
	{
	}
}