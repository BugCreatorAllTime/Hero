using System;

public class ArmorBuff : AbstractBuff
{
	public const float DELAY = 0.5f;

	public ArmorBuff ()
	{
	}
	
	public ArmorBuff (float value)
	{
		this.value = value;
		this.duration = DELAY;
	}
	
	protected override void OnFixedUpdate (float dt)
	{
	}
	protected override void OnLeveling ()
	{
	}
	protected override void OnRemove ()
	{
		owner.AddArmor ((int)value);
	}
	protected override void OnAdd ()
	{
		this.duration = DELAY;
	}

	public override void OnOverride (AbstractBuff buff)
	{
		value += buff.getValue();
	}
}
