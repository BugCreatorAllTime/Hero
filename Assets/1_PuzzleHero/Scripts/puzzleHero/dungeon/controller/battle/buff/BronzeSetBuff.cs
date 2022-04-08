using UnityEngine;

public class BronzeSetBuff : TurnBasedBuff
{
	private GameObject fxObject;
	private int blockCount;

	protected override void OnAdd()
	{
		base.OnAdd();
		owner.getStat().CurrentBlock += blockCount;
		owner.OnStatsChangedEvent += OnStatsChanged;
		fxObject = effectsManager.CreateBronzeSetSkillEffect();
	}

	public BronzeSetBuff(int turns, EffectsManager effectsManager, int blockCount)
		: base(turns, effectsManager)
	{
		this.blockCount = blockCount;
	}

	protected override void OnTurnElapsed()
	{
	}

	private void OnStatsChanged(HeroStat stat) {
		//Logger.Trace("stat", stat.CurrentBlock);
		if (stat.CurrentBlock == 0)
		{
			owner.OnStatsChangedEvent -= OnStatsChanged;
			Remove();
			//Logger.Trace("no more block");
			if (fxObject != null) {
				SkeletonAnimation anim = fxObject.GetComponent<SkeletonAnimation>();
				anim.state.SetAnimation(0, FxAnimationName.end, false);
				anim.state.End += null;
				anim.state.End += delegate(Spine.AnimationState state, int index)
				{
					effectsManager.spinePool.ReturnInstance(fxObject);
					fxObject.SetActive(false);
				};
			}
		}
	}

	private void Remove()
	{
		int index = -1;
		for (int i = 0; i < owner.buffs.Count; i++)
		{
			if (owner.buffs[i].GetType() == this.GetType())
			{
				index = i;
			}
		}
		if (index != -1)
		{
			owner.buffs.RemoveAt(index);
		}
	}
}