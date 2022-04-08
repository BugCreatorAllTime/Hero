using System;

public class EnergyBuff : AbstractBuff
{
	public const float DELAY = 0.5f;

	public EnergyBuff()
	{
	}

	public EnergyBuff(float value)
	{
		this.value = value;
	}

	protected override void OnFixedUpdate(float dt)
	{
	}

	protected override void OnAdd()
	{
		this.duration = DELAY;
	}

	protected override void OnLeveling()
	{
	}

	protected override void OnRemove()
	{
		owner.AddMana((int)value);
	}

	public override void OnOverride (AbstractBuff buff)
	{
		value += buff.getValue();
	}
	
}