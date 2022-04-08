using UnityEngine;
using System.Collections;

[System.Serializable]
public abstract class AbstractBuff
{
	protected int level = 0;

	public static readonly float INFINITY = float.MaxValue;
	public float duration;
	protected int count;
	protected float value;
	protected BattleEntity owner;
	
	public void BeAdded (BattleEntity owner)
	{
		this.owner = owner;
		OnAdd ();
	}

	public void BeRemoved ()
	{
		OnRemove ();
		owner = null;
	}

	public void Leveling (int value)
	{
		level = value;
		OnLeveling ();
	}
	public float getValue ()
	{
		return value;
	}
	public void FixedUpdate (float dt)
	{
		OnFixedUpdate (dt);
	}

	public virtual void OnOverride(AbstractBuff buff)
	{
		this.duration = buff.duration;
	}

	protected abstract void OnFixedUpdate (float dt);
	protected abstract void OnAdd ();
	protected abstract void OnRemove ();
	protected abstract void OnLeveling ();

}
