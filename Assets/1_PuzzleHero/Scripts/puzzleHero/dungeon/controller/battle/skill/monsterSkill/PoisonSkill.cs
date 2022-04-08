using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AnimationState = Spine.AnimationState;

public class PoisonSkill : MonsterSkill
{
	private float damagePercent;	//5%
	protected List<MatchItem> affectedGems = new List<MatchItem>();
	protected Dictionary<MatchItem, GameObject> fxObjects = new Dictionary<MatchItem, GameObject>();
	protected BattleAction autoActivationBattleAction;
	private MatchLogic.OnMatched onMatched;

	protected override void AutoActivate()
	{
		base.AutoActivate();
		//Logger.Trace("auto activate");
		//Logger.Trace("affectedGems.Count", affectedGems.Count);

		//if owner is dead, drop gem and release input
		if (owner.ObjState == ObjectState.Dead || affectedGems.Count <= 0) {
		//Logger.Trace("auto active while owner is dead");
			return;
		}

		//if skill has no activation time, release gem, 

		//obtain input
		matchLogic.DisableBoardInput();
		autoActivationBattleAction = matchLogic.battlePhaseManager.AddNeutralAction();

		//play active effect, when finish, deal damage and release input
		if (affectedGems.Count > 0)
		{
			float wait = 0;
			for (int i = 0; i < fxObjects.Count; i++)
			{
				SkeletonAnimation anim = fxObjects.ElementAt(i).Value.GetComponent<SkeletonAnimation>();
				anim.state.SetAnimation(0, FxAnimationName.active, false);
				//Logger.Trace("active duration ", anim.state.GetCurrent(0).Animation.Duration);
				anim.state.End += null;
				wait = anim.state.GetCurrent(0).Animation.Duration;
			}
			matchLogic.routineRunner.StartCoroutine(WaitForActiveFxFinish(wait + 0.2f));
		}
	}

	private IEnumerator WaitForActiveFxFinish(float wait)
	{
		yield return new WaitForSeconds(wait);
		OnActiveEffectFinish();
	}

	protected virtual void OnActiveEffectFinish()
	{
		//Logger.Trace("onActiveFinish");
		BattleEntity character = battleGameLogic.FindEntity(BattleGameLogic.TEAM1);
		int characterMaxHp = character.getStat().GetMaxHp();
		float damagePerGem = damagePercent * characterMaxHp;
		int totalDamage = (int)(damagePerGem * fxObjects.Count);
		owner.DealUnabsorbableDmgTo(character, totalDamage);
		for (int i = 0; i < fxObjects.Count; i++)
		{
			effectsManager.ReturnSpineAnimationToPool(fxObjects.ElementAt(i).Value);
		}
		RemoveCallbacks();
		Destroy();
	}

	protected override void AutoDeactivate()
	{
		base.AutoDeactivate();

		//drop gem
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		skillsManager.AddSkill(this);
		GetExtras();
		//pick gem
		PickGem();

		//play begin effect, when effect finish, release input
		//PlayBeginEffect();
		PlayCastToBoardEffect();

		SetupCallbacks();
	}

	private void SetupCallbacks()
	{
		//TODO Drop gem when gem is matched; if all gems is match, remove skill
		onMatched = new MatchLogic.OnMatched(OnMatched);
		matchLogic.OnMatchedEvent += onMatched;

		//drop gem when owner die
		owner.OnStateChangedEvent += new BattleEntity.OnStateChanged(OnOwnerStateChanged);

		battleGameLogic.FindEnemy(owner.Team).OnStateChangedEvent += OnEnemyStateChanged;
	}

	protected virtual void GetExtras()
	{
		if (!string.IsNullOrEmpty(skillInfo.extras))
		{
			damagePercent = float.Parse(skillInfo.extras.Split('/')[0]);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveSelf();
	}

	protected override void OnFixedUpdate(float dt)
	{
		base.OnFixedUpdate(dt);

		//update effects position according to gems position
		for (int i = 0; i < affectedGems.Count; i++)
		{
			MatchItem gem = affectedGems[i];
			if(!gem.IsBeingAffected()) continue;
			GameObject fxGameObject = null;
			fxObjects.TryGetValue(gem, out fxGameObject);
			if (fxGameObject != null)
			{
				fxGameObject.transform.localPosition = gem.transform.localPosition;
			}
		}
	}

	private void PickGem()
	{
		List<TilePoint> affectableGems = FindAffectableGems();
		int numberOfAffectedGems = skillInfo.numberOfAffectedGems;

		if(affectableGems.Count == 0)
		{}

		if (affectableGems.Count < numberOfAffectedGems)
		{
			numberOfAffectedGems = affectableGems.Count;
		}

		//Logger.Trace("gems count ", affectableGems.Count);
		List<TilePoint> randomizedGems = new List<TilePoint>();
		for (int i = 0; i < numberOfAffectedGems; i++)
		{
			TilePoint tilePoint;
			do
			{
				int index = UnityEngine.Random.Range(0, affectableGems.Count);
				//Logger.Trace("random index ", index);
				tilePoint = affectableGems[index];
			} while (randomizedGems.Contains(tilePoint));
			randomizedGems.Add(tilePoint);
			MatchItem gem = matchLogic.FindTile(tilePoint);
			affectedGems.Add(gem);
			OnGemPicked(gem);
		}
	}

	protected virtual List<TilePoint> FindAffectableGems()
	{
		Cell[,] cells = matchLogic.Cells;
		List<TilePoint> affectableGems = new List<TilePoint>();
		for (int x = 0; x < Data.tileWidth; x++) {
			for (int y = 0; y < Data.tileHeight; y++) {
				Cell thiscell = cells[x, y];
				BoardSkillInfo.AffectType type = skillInfo.Parse(skillInfo.affectedGemType);
				if (!IsThisGemTypePickable(thiscell.cellType, type)) {
					continue;
				}
				TilePoint point = new TilePoint(x, y);
				MatchItem tile = matchLogic.FindTile(point);
				if (tile.IsBeingAffected()) {
					continue;
				}
				affectableGems.Add(point);
			}
		}
		return affectableGems;
	}

	protected virtual bool IsThisGemTypePickable(Data.TileTypes gemType, BoardSkillInfo.AffectType skillType)
	{
		if (skillType == BoardSkillInfo.AffectType.Random)
		{
			return true;
		}
		else
		{
			return gemType == skillInfo.Convert(skillType);
		}
	}

	protected virtual void OnGemPicked(MatchItem gem)
	{
		gem.SetAffected(true);
	}

	protected virtual void OnGemDropped(MatchItem gem)
	{
		gem.SetAffected(false);
	}

	public override bool AreThereAnyAffectableGems()
	{
		return FindAffectableGems().Count > 0;
	}

	private void PlayCastToBoardEffect()
	{
		float longestDuration = 0;
		for (int i = 0; i < affectedGems.Count; i++)
		{
			Vector3 p0 = EffectsManager.monsterCastPosition;
			Vector3 p1 = EffectsManager.boardTopCenter;
			Vector3 p2 = affectedGems[i].transform.localPosition;
			Vector3[] path = new Vector3[]{p0, p1, p2};
			float duration = effectsManager.CreateCastFlyEffect(this.GetType(), path);

			matchLogic.routineRunner.StartCoroutine(PlayBeginEffect(duration, i));
			if (duration >= longestDuration) longestDuration = duration;
		}
		SoundManager.intance.PlaySound (SoundName.CAST_SKILL_TO_BOARD);
		matchLogic.routineRunner.StartCoroutine(WaitForBeginEffectFinish(longestDuration));
		//Logger.Trace("duration ", duration);
	}

	private IEnumerator WaitForBeginEffectFinish(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		OnBeginEffectFinish();
	}

	private IEnumerator PlayBeginEffect(float delay, int fxIndex)
	{
		yield return new WaitForSeconds(delay);
		MatchItem gem = affectedGems[fxIndex];
		EffectsManager.OnSpineAnimationEndCallback callback = null;
		GameObject fxGameObject = effectsManager.CreateSpineEffect(skillInfo.animation, FxAnimationName.begin, GetSkinNameFor(gem), false, null, false);
		Vector3 localScale = fxGameObject.transform.localScale;
		fxGameObject.transform.localScale = new Vector3(localScale.x * MatchLogic.cellScaleX, 1, 1);
		SkeletonAnimation anim = fxGameObject.GetComponent<SkeletonAnimation>();
		anim.state.AddAnimation(0, FxAnimationName.idle, true, 0);
		fxGameObject.transform.localPosition = gem.transform.localPosition;
		fxObjects.Add(gem, fxGameObject);
	}

	protected virtual string GetSkinNameFor(MatchItem gem)
	{
		return null;
	}

	protected virtual void OnBeginEffectFinish()
	{
		//Change phase to release input
		end();
	}

	private void OnMatched(List<MatchItem> matchedItems)
	{
		OnMatched(matchedItems, true);
	}

	private void OnMatched(List<MatchItem> matchedItems, bool autoRemove)
	{
		bool checkSound = false;
		float wait = 0;
		List<GameObject> fxObjectsToRemove = new List<GameObject>();
		for (int i = 0; i < matchedItems.Count; i++)
		{
			MatchItem matchedItem = matchedItems[i];
			for (int j = 0; j < affectedGems.Count; j++)
			{
				MatchItem affectedGem = affectedGems[j];
				if (matchedItem.point.Equals(affectedGem.point))
				{
					//Logger.Trace("affectedGem found");
					GameObject fxGameObject = null;
					fxObjects.TryGetValue(affectedGem, out fxGameObject);
					if (fxGameObject != null)
					{
						//Logger.Trace("fxObject found");
						//Logger.Trace("fxObject", fxGameObject.name);
						checkSound = true;
						fxObjectsToRemove.Add(fxGameObject);
						OnGemDropped(affectedGem);
						fxObjects.Remove(affectedGem);
						affectedGems.Remove(affectedGem);
						SkeletonAnimation anim = fxGameObject.GetComponent<SkeletonAnimation>();
						anim.state.SetAnimation(0, FxAnimationName.counter, false);
						wait = anim.state.GetCurrent(0).Animation.Duration;
					}
				}
			}
		}
		//if there is no more affectedGem, remove this
		if (fxObjects.Count == 0) {
			RemoveCallbacks();
			if (owner != null)
			{
				owner.RemoveSkill(this);
			}
			else
			{
				Destroy();
			}
		}
		if (wait > 0)
		{
			matchLogic.routineRunner.StartCoroutine(WaitForCounterFxFinish(wait, fxObjectsToRemove));
		}
		if(checkSound)
		{
			PlaySoundDeactivate();
		}
	}

	private IEnumerator WaitForCounterFxFinish(float wait, List<GameObject> fxObjectsToRemove)
	{
		yield return new WaitForSeconds(wait);

		for (int i = 0; i < fxObjectsToRemove.Count; i++)
		{
			effectsManager.ReturnSpineAnimationToPool(fxObjectsToRemove[i]);
		}
	}

	protected override void PlaySoundDeactivate()
	{
	}

	private void OnEnemyStateChanged(BattleEntity battleEntity)
	{
		if (battleEntity.ObjState == ObjectState.Dead)
		{
			RemoveCallbacks();
			RemoveAffectedGems();
			Destroy();
		}
	}

	private void OnOwnerStateChanged(BattleEntity owner)
	{
		if (owner.ObjState == ObjectState.Dead)
		{
			//Logger.Trace("affected gems count", affectedGems.Count);
			RemoveCallbacks();
			RemoveAffectedGems();
		}
	}

	private void RemoveAffectedGems()
	{
		List<MatchItem> matchedItems = new List<MatchItem>(affectedGems);
		OnMatched(matchedItems);
	}

	protected override void RemoveSelf()
	{
		//Logger.Trace("removeSelf");
		base.RemoveSelf();
		//TODO remove callback from matchLogic
		if (onMatched != null)
		{
			matchLogic.OnMatchedEvent -= onMatched;
		}
	}

	protected void RemoveCallbacks()
	{
		if(owner == null) return;
		//Logger.Trace("remove callback");
		BattleEntity enemy = battleGameLogic.FindEnemy(owner.Team);
		owner.OnStateChangedEvent -= OnOwnerStateChanged;
		enemy.OnStateChangedEvent -= OnEnemyStateChanged;
	}

	protected override void Reset()
	{
		base.Reset();
		if (autoActivationBattleAction != null)
		{
			autoActivationBattleAction.state = BattleAction.ActionState.Finished;
		}
		for (int i = 0; i < affectedGems.Count; i++)
		{
			OnGemDropped(affectedGems[i]);
		}
		affectedGems.Clear();
		fxObjects.Clear();
	}

	public override List<MatchItem> GetAffectedGems() {
		return affectedGems;
	}

	public override void SetAffectedGems(List<MatchItem> gems)
	{
		affectedGems = gems;
	}

	public override void PlayFxOnGem()
	{
		for (int i = 0; i < affectedGems.Count; i++) {
			matchLogic.routineRunner.StartCoroutine(PlayBeginEffect(0.1f, i));
		}
	}

	public override void SetOwner(BattleEntity entity)
	{
		base.SetOwner(entity);
		GetExtras();
		SetupCallbacks();
	}
}