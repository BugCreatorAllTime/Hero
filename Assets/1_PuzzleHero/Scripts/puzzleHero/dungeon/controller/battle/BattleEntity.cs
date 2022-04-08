using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Nfury.Base;
using UnityEngine;

public class BattleEntity : Entity
{
	private const float CHAR_RATETIMEATTACK = 0.5f;
	private const float MOB_RATETIMEATTACK = 0.525f;

	protected int team;
	private int objStatus = ObjectStatus.NORMAL;
	protected ObjectState objState = ObjectState.Idle;
	protected HeroStat heroStat;
	public List<BaseSkill> skills = new List<BaseSkill>();
	public List<AbstractBuff> buffs = new List<AbstractBuff>();
	public float deadDuration = 0.5f;
	public float attackDuration = -1;
	public float halfOfAttackDur = -1;
	public float preAttackDuration = -1;
	public float beHitDuration = -1;
	private int countTurn;
	private SkeletonAnimation skeletonAnimation;
	protected float originTimeScale;
	private GameObject obj;
	private IntegerWrapper curTurn;
	private float rateAttack = 1;

	public delegate void OnStatsChanged(HeroStat stat);
	public delegate void OnStatsElementChanged(int value, TextState textState, int team);
	public delegate void OnStateChanged(BattleEntity battleEntity);

	public event OnStatsChanged OnStatsChangedEvent;
	public event OnStatsElementChanged OnStatsElementChangedEvent;
	public event OnStateChanged OnStateChangedEvent;
	public event OnStateChanged OnTurnChangedEvent;

	public BattleEntity(GameObject obj)
	{
		gObject = obj;
		skeletonAnimation = gObject.GetComponent<SkeletonAnimation>();
		originTimeScale = skeletonAnimation.timeScale;
		objState = ObjectState.Idle;
	}

	public object tag { get; set; }

	public GameObject gObject
	{
		get
		{
			return obj;
		}
		set
		{
			obj = value;
		}
	}
	public int Team
	{
		get
		{
			return team;
		}
		set
		{
			team = value;
		}
	}
	public int CurTurn
	{
		get
		{
			return this.curTurn.GetValue();
		}
		set
		{
			this.curTurn = value;
			NotifyTurnChanged();
		}
	}
	public void addStat(HeroStat stat)
	{
		this.heroStat = stat;
		this.heroStat.Init();
		if (Team == BattleGameLogic.TEAM2)
		{
			this.CurTurn = stat.baseStat.turn;
			rateAttack = MOB_RATETIMEATTACK;
		}else{
			rateAttack = CHAR_RATETIMEATTACK;
		}
	}
	public HeroStat getStat()
	{
		return this.heroStat;
	}
	public BattleGameLogic BattleLogic
	{
		get
		{
			return gLogic as BattleGameLogic;
		}
	}

	public ObjectState ObjState
	{
		get
		{
			return objState;
		}
		set
		{
			if (objState != value && objState != ObjectState.Dead)
				objState = value;
			switch (value)
			{
				case ObjectState.Idle:
					Idle();
					break;
				case ObjectState.Run:
					Run();
					break;
				case ObjectState.Attack:
					Attack();
					break;
				case ObjectState.Cast:
					Cast();
					break;
				case ObjectState.PreAttack:
					PreAttack();
					break;
				case ObjectState.BeHit:
					BeHit();
					break;
				case ObjectState.Dead:
					Dead();
					break;
			}
			NotifyStateChanged();
		}
	}

	public void addStatus(int status)
	{
		if (!ObjectStatus.hasStatus(objStatus, status))
		{
			objStatus += status;
		}
	}

	public void removeStatus(int status)
	{
		if (ObjectStatus.hasStatus(objStatus, status))
		{
			objStatus -= status;
		}
	}

	public void AddBuff(AbstractBuff buff)
	{
		if (ObjState != ObjectState.Dead && !buffs.Contains(buff))
		{
			int foundIdx = -1;
			for (int i = 0; i < buffs.Count; ++i)
			{
				if (buffs[i].GetType() == buff.GetType())
				{
					foundIdx = i;
					break;
				}
			}
			if (foundIdx > -1)
			{
				buffs[foundIdx].OnOverride(buff);
			}
			else
			{
				buffs.Add(buff);
				buff.BeAdded(this);
			}
		}
	}

	public void AddSkill(BaseSkill skill)
	{
		if (ObjState != ObjectState.Dead && !skills.Contains(skill))
		{
			skills.Add(skill);
			skill.BeAdded(this);
			if (Team != BattleGameLogic.TEAM2 && this.objState != ObjectState.PreAttack)
				this.ObjState = ObjectState.PreAttack;
		}
	}
	public void RemoveSkill(BaseSkill skill)
	{
		skills.Remove(skill);
		skill.Destroy();
	}

	public void Heal(int value)
	{
		heroStat.AddHp(value);
		NotifyStatsChanged();
		NotifyStatsElementChanged (value, TextState.HpAdd, team);
	}

	public void AddArmor(int value)
	{
		heroStat.AddArmor(value);
		NotifyStatsChanged();
		NotifyStatsElementChanged (value, TextState.Def, team);
	}

	public void AddMana(int num)
	{
		heroStat.AddMana(num);
		NotifyStatsChanged();
		NotifyStatsElementChanged ((int)(num), TextState.Mana, team);
	}

	public void DoEmptyEnergy()
	{
		heroStat.SubMana(heroStat.baseStat.manaPool);
		NotifyStatsChanged();
	}

	protected override void OnFixedUpdate(float dt)
	{
		int len = buffs.Count;
		for (int i = 0; i < len; ++i)
		{
			AbstractBuff buff = buffs[i];
			buff.FixedUpdate(dt);
			if (buff.duration != AbstractBuff.INFINITY)
			{
				buff.duration -= dt;
				if (buff.duration <= 0)
				{
					buffs.RemoveAt(i);
					buff.BeRemoved();
					--len;
					--i;
				}
			}
		}

		len = skills.Count;
		for (int i = 0; i < len; ++i)
		{
			BaseSkill skill = skills[i];
			skill.FixedUpdate(dt);
		}
		if (this.ObjState == ObjectState.BeHit)
		{
			beHitDuration -= dt;
			if (beHitDuration <= 0)
				this.ObjState = ObjectState.Idle;
		}
		UpdateDeadCount(dt);
	}


	protected override void OnUpdate(float dt)
	{
		skeletonAnimation.timeScale = BattleLogic.GameSpeed;
	}

	protected override void Destroy()
	{
		for (int i = 0; i < buffs.Count; ++i)
		{
			buffs[i].BeRemoved();
		}
		buffs.Clear();
		for (int i = 0; i < skills.Count; ++i)
		{
			skills[i].Destroy();
		}
		skills.Clear();
		GameObject.Destroy(this.gObject);
	}

	public void DealDmgTo(BattleEntity target, int value)
	{
		if (target == null) return;
		target.BeDealtDmg(this, value, true);
	}

	public void DealUnabsorbableDmgTo(BattleEntity target, int value)
	{
		if (target == null) return;
		target.BeDealtDmg(this, value, false);
	}

	public void BeDealtDmg(BattleEntity dealer, int value, bool absorb)
	{
		if (ObjState != ObjectState.Dead)
		{
			if (heroStat.CurrentBlock > 0)
			{
				heroStat.CurrentBlock--;
			}
			else
			{
				if (absorb)
				{
					float rate = 0.8f;
					int absorbDmg = (int)(value * rate);
					absorbDmg = absorbDmg > heroStat.CurArmor ? heroStat.CurArmor : absorbDmg;
					heroStat.SubArmor(absorbDmg);
					value -= absorbDmg;
				}
				heroStat.SubHp(value);
				dealer.NotifyStatsElementChanged (value, TextState.HpSub, dealer.Team);
				if (heroStat.CurHp == 0) {
					ObjState = ObjectState.Dead;
//					skills.Clear();
				}
				else {
					ObjState = ObjectState.BeHit;
				}
			}
			NotifyStatsChanged();
		}
	}


	protected void UpdateDeadCount(float dt)
	{
		if (objState == ObjectState.Dead)
		{
			deadDuration -= dt;
			if (deadDuration < 0)
			{
				BattleLogic.RemoveObject(this);
			}
		}
	}

	protected void Idle()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);
		skeletonAnimation.skeletonDataAsset.GetAnimationStateData().SetMix(AnimationName.Idle, AnimationName.Run, 0.5f/gLogic.GameSpeed);
	}

	protected void Run()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.Run, true);
		skeletonAnimation.skeletonDataAsset.GetAnimationStateData().SetMix(AnimationName.Run, AnimationName.Idle, 0.5f/gLogic.GameSpeed);

	}

	protected void PreAttack()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.PreAttack, false);
		preAttackDuration = skeletonAnimation.state.GetCurrent(0).Animation.Duration / gLogic.GameSpeed;
		skeletonAnimation.state.AddAnimation(0, AnimationName.PreAttack2, true, 0);
	}

	protected void Attack()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.Attack, false);
		halfOfAttackDur = skeletonAnimation.state.GetCurrent(0).Animation.Duration * rateAttack / gLogic.GameSpeed;
		attackDuration = skeletonAnimation.state.GetCurrent(0).Animation.Duration / gLogic.GameSpeed;
		skeletonAnimation.skeletonDataAsset.GetAnimationStateData().SetMix(AnimationName.Attack, AnimationName.Idle, 0.5f/gLogic.GameSpeed);
	}

	protected void BeHit()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.BeHit, false);
		beHitDuration = skeletonAnimation.state.GetCurrent(0).Animation.Duration / gLogic.GameSpeed;
	}

	protected void Dead()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.BeHit, false);
		deadDuration += skeletonAnimation.state.GetCurrent(0).Animation.Duration;
		skeletonAnimation.state.AddAnimation(0, AnimationName.Dead, false, 0);
		deadDuration += skeletonAnimation.state.GetCurrent(0).Animation.Duration;
	}

	protected void Cast ()
	{
		skeletonAnimation.state.SetAnimation(0, AnimationName.Cast, false);
		attackDuration = skeletonAnimation.state.GetCurrent(0).Animation.Duration / gLogic.GameSpeed;
		skeletonAnimation.skeletonDataAsset.GetAnimationStateData().SetMix(AnimationName.Cast, AnimationName.Idle, 0.5f/gLogic.GameSpeed);
	}

	private void NotifyStatsChanged()
	{
		if (OnStatsChangedEvent != null) {
			OnStatsChangedEvent(heroStat);
		}
	}

	private void NotifyStatsElementChanged(int value, TextState textState, int team)
	{
		if (OnStatsChangedEvent != null) {
			OnStatsElementChangedEvent(value, textState, team);
		}
	}

	private void NotifyStateChanged()
	{
		if (OnStateChangedEvent != null)
		{
			OnStateChangedEvent(this);
		}
	}

	private void NotifyTurnChanged()
	{
		if (OnTurnChangedEvent != null)
		{
			OnTurnChangedEvent(this);
		}
	}
}
