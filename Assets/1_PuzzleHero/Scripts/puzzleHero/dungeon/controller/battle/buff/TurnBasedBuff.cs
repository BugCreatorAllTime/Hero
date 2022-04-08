public class TurnBasedBuff : AbstractBuff
{
	protected int turnBasedDuration;
	protected EffectsManager effectsManager;

	public int DurationInTurn
	{
		get { return turnBasedDuration; }
	}

	public TurnBasedBuff(int turnBasedDuration, EffectsManager effectsManager)
	{
		this.duration = INFINITY;
		this.turnBasedDuration = turnBasedDuration;
		this.effectsManager = effectsManager;
	}

	protected override void OnFixedUpdate(float dt)
	{
	}

	protected override void OnAdd()
	{
	}

	protected override void OnRemove()
	{
	}

	protected override void OnLeveling()
	{
	}

	public void ElapseTurn()
	{
		//Logger.Trace(this.GetType(), "elapse turn");
		this.turnBasedDuration--;
		OnTurnElapsed();
	}

	protected virtual void OnTurnElapsed()
	{
	}
}