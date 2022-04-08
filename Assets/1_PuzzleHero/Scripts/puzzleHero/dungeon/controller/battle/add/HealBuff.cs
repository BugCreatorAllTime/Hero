
using System;

public class HealBuff : AbstractBuff
{
	public const float DELAY = 0.5f;
	
	public HealBuff (float value)
	{
		this.value = value;
	}

	protected override void OnFixedUpdate (float dt)
	{
	}

	protected override void OnAdd ()
	{
		this.duration = DELAY;
	}

	protected override void OnLeveling ()
	{
	}

	protected override void OnRemove ()
	{
		owner.Heal ((int)value);
	}

	public override void OnOverride (AbstractBuff buff)
	{
		value += buff.getValue();
	}
}


