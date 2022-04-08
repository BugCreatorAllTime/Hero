using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Holoville.HOTween;
using UnityEngine;

public class VikingSetSkill : ItemSetSkill
{
	private List<TilePoint> affectedGems;
	private List<MatchItem> counteredGems;
	private float damagePercent;
	private GameObject fxGameObject;
	private GameObject chargeFxObject;
	private const float flyDuration = 0.4f;

	protected override void OnDestroy()
	{
	}

	protected override void OnAdd()
	{
		string[] parms = extrasInfo.Split('/');
		damagePercent = float.Parse(parms[0]);
	}

	protected override void ProcessLogicSkill() {
		float delay = CreateParticleFromAtkGem();
		matchLogic.routineRunner.StartCoroutine(WaitThenAttack(delay));
	}

	public override void PlayChargeFx() {
		chargeFxObject = Charge();
	}

	private IEnumerator WaitThenAttack(float delay)
	{
		yield return new WaitForSeconds(delay);
		effectsManager.ReturnSpineAnimationToPool(chargeFxObject);
		owner.ObjState = ObjectState.Attack;
		duration = this.owner.attackDuration;
		owner.DoEmptyEnergy();
		SoundManager.intance.PlaySound(SoundName.VIKING_SET_SKILL);
	}

	private GameObject Charge()
	{
		return effectsManager.CreateVikingChargeFx();
	}

	private float CreateParticleFromAtkGem()
	{
		float delay = 0;
		List<TilePoint> points = FindMatchingGems();
		for (int i = 0; i < points.Count; i++)
		{
			TilePoint p = points[i];
			MatchItem item = matchLogic.FindTile(p);
			Vector3 from = item.transform.localPosition;
			Vector3 to = EffectsManager.characterCastPosition;
			delay = effectsManager.CreateVikingParticleChargeFx(from, to);
		}
		return delay;
	}

	protected override void OnCastFinished()
	{
		base.OnCastFinished();
		FireSlash();
	}

	private void FireSlash()
	{
		fxGameObject = effectsManager.CreateVikingFireSlash();
		SkeletonAnimation animation = fxGameObject.GetComponent<SkeletonAnimation>();
		animation.state.End += OnFireSlashFinished;
	}

	private void OnFireSlashFinished(Spine.AnimationState state, int trackIndex)
	{
		DealDmg();
		matchLogic.SetMatchIsFromUserInput();
		end();
	}

	private void DealDmg() {
		int damage = Mathf.CeilToInt(FindMatchingGems().Count * (owner.getStat().GetDmg() * damagePercent));
		owner.DealDmgTo(battleGameLogic.FindEnemy(owner.Team), damage);
	}

	private List<TilePoint> FindMatchingGems()
	{
		Cell[,] cells = matchLogic.Cells;
		List<TilePoint> points = new List<TilePoint>();
		for (int x = 0; x < Data.tileWidth; x++)
		{
			for (int y = 0; y < Data.tileHeight; y++)
			{
				TilePoint point = new TilePoint(x, y);
				if (matchLogic.FindTile(point).cell.cellType != Data.TileTypes.Attack)
				{
					continue;
				}
				points.Add(point);
			}
		}
		return points;
	}

	protected override string GetPreSkillTexture() {
		string[] parms = extrasInfo.Split('/');
		return parms[1];
	}

	protected override string GetPreSkillColorsString() {
		return extrasInfo.Split('/')[2];
	}
}