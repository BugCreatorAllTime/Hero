using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using Holoville.HOTween.Plugins.Core;
using Nfury.Base;
using strange.examples.strangerocks;
using strange.extensions.pool.api;
using UnityEngine;
using AnimationState = Spine.AnimationState;
using PathType = Holoville.HOTween.PathType;
using Tweener = Holoville.HOTween.Tweener;

public class EffectsManager {
	public static Vector3 monsterCastPosition = new Vector3(480, 610, 0);
	public static Vector3 characterCastPosition = new Vector3(135, 600, 0);
	public static Vector3 boardTopCenter = new Vector3(315, 545, 0);
	public static Vector3 team1BeHitPosition = new Vector3(130, 650, 0);
	public static Vector3 team2BeHitPosition = new Vector3(480, 700, 0);
	public static Vector3 team2AutoRecoverSkillPosition = new Vector3(480, 580, 0);
	public static Vector3 team2AdventurerSetSkillPosition = new Vector3(150, 560, 0);
	public static Vector3 team2HunterSetSkillPosition = new Vector3(50, 560, 0);
	public static Vector3 team1KnightSetSkillPosition = new Vector3(150, 560, 0);
	public static Vector3 team1SamuraiSetSkillPosition = new Vector3(150, 560, 0);
	public static Vector3 team1BronzeSetSkillPosition = new Vector3(122, 580, 0);
	public static Vector3 team1AssassinSetSkillPosition = new Vector3(220, 590, 0);
	public static Vector3 team1AssassinSetSkillPosition2 = new Vector3(97, 590, 0);
	public static Vector3 vikingSetSkillExplosionPosition = new Vector3(465, 660, 0);
	public static Vector3 vikingSetSkillParticleStartPosition = new Vector3(-100, 670, 0);
	public static Vector3 vikingSetSkillParticleStopPosition = vikingSetSkillExplosionPosition;
	public static Vector3 vikingSetSkillFireSlashPosition = new Vector3(100, 570, 0);
	public static Vector3 vikingSetSkillChargePosition = new Vector3(140, 575, 0);
	public static Vector3 chargeFxPosition = new Vector3(260, 790, 0);
	public static Vector3 team1PaladinSetSkillPosition = new Vector3(150, 560, 0);
	public static Vector3 team1PaladinSetSkillPosition2 = new Vector3(120, 850, 0);

	[Inject(Prefabs.fx)]
	public IPool<GameObject> spinePool { get; set; }

	[Inject(Prefabs.gem)]
	public IPool<GameObject> uiSpritePool { get; set; }

	[Inject(Prefabs.gemFlyFx)]
	public IPool<GameObject> particlePool { get; set; }

	[Inject(Prefabs.adventurerSetSkill)]
	public IPool<GameObject> adventurerSetSkillPool { get; set; }

	[Inject(Prefabs.poisonFlyFx)]
	public IPool<GameObject> poisonFlyPool { get; set; }

	[Inject(Prefabs.fireFlyFx)]
	public IPool<GameObject> fireFlyPool { get; set; }

	[Inject(Prefabs.autoRecoverFlyFx)]
	public IPool<GameObject> autoRecoverFlyPool { get; set; }

	[Inject(Prefabs.autoRecoverFlyBackFx)]
	public IPool<GameObject> autoRecoverFlyBackPool { get; set; }

	[Inject(Prefabs.iceLockFlyFx)]
	public IPool<GameObject> iceLockFlyPool { get; set; }

	[Inject(Prefabs.rockLockFlyFx)]
	public IPool<GameObject> rockLockFlyPool { get; set; }

	[Inject(Prefabs.counterFlyFx)]
	public IPool<GameObject> counterFlyPool { get; set; }

	[Inject(Prefabs.matchTlFx)]
	public IPool<GameObject> matchTlPool { get; set; }

	[Inject(Prefabs.vikingSetSkill)]
	public IPool<GameObject> vikingSetSkillPool { get; set; }

	[Inject(Prefabs.sprite)]
	public IPool<GameObject> spritePool { get; set; }

	[Inject(Prefabs.lineUpward)]
	public IPool<GameObject> linesUpwardsPool { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleGameLogic { get; set; }

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public TextFxManager textManager { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	//Charge Fx
	private const float minRatio = 0.5f;
	private const float stepRatio = 0.3f;
	private const float maxRatio = 1.7f;

	private GameObject fxLayer;
	private GameObject fxEndGame;
	private GameObject chargeFx;
	private Dictionary<string, SkeletonDataAsset> cachedSkeletonDataAssets;
	private Dictionary<AnimationState, GameObject> returnInstances;
	private Dictionary<Data.TileTypes, Vector3> destinationPositions;
	private Dictionary<Data.TileTypes, Sprite> colorEffect;

	private Dictionary<Data.TileTypes, Material> starEffect;
	private Dictionary<Type, IPool<GameObject>> flyCastPools;
	private Sprite hpEffect;
	private Sprite mpEffect;
	private Sprite defEffect;
	private Sprite goldEffect;
	private Sprite attackEffect;

	private Material hpStarEffect;
	private Material mpStarEffect;
	private Material defStarEffect;
	private Material goldStarEffect;
	private Material attackStarEffect;

	[PostConstruct]
	public void PostConstruct() {
		this.fxLayer = GameObject.FindWithTag(ObjectTag.fxLayer);
		this.fxEndGame = GameObject.FindWithTag(ObjectTag.fxEndGame);
		guiEventHandler.OnGuiBeingDisplayedEvent += OnGuiBeingDisplayed;
		spinePool.inflationType = PoolInflationType.INCREMENT;
		particlePool.inflationType = PoolInflationType.INCREMENT;

		cachedSkeletonDataAssets = new Dictionary<string, SkeletonDataAsset>();
		returnInstances = new Dictionary<AnimationState, GameObject>();
		destinationPositions = new Dictionary<Data.TileTypes, Vector3>();
		destinationPositions.Add(Data.TileTypes.Attack, new Vector3(200, 655, 0));
		destinationPositions.Add(Data.TileTypes.Defend, new Vector3(60, 930, 0));
		destinationPositions.Add(Data.TileTypes.Gold, new Vector3(200, 655, 0));
		destinationPositions.Add(Data.TileTypes.Heal, new Vector3(150, 900, 0));
		destinationPositions.Add(Data.TileTypes.Skill, new Vector3(175, 850, 0));
		hpEffect = Sprite.Instantiate(assetMgr.GetAssetSync<Sprite>("Textures/Board/Red")) as Sprite;
		mpEffect = Sprite.Instantiate(assetMgr.GetAssetSync<Sprite>("Textures/Board/Violet")) as Sprite;
		defEffect = Sprite.Instantiate(assetMgr.GetAssetSync<Sprite>("Textures/Board/Blue")) as Sprite;
		goldEffect = Sprite.Instantiate(assetMgr.GetAssetSync<Sprite>("Textures/Board/Yellow")) as Sprite;
		attackEffect = Sprite.Instantiate(assetMgr.GetAssetSync<Sprite>("Textures/Board/White")) as Sprite;

		hpStarEffect = Material.Instantiate(assetMgr.GetAssetSync<Material>("Prefabs/Fx/Materials/Red")) as Material;
		mpStarEffect = Material.Instantiate(assetMgr.GetAssetSync<Material>("Prefabs/Fx/Materials/Violet")) as Material;
		defStarEffect = Material.Instantiate(assetMgr.GetAssetSync<Material>("Prefabs/Fx/Materials/Blue")) as Material;
		goldStarEffect = Material.Instantiate(assetMgr.GetAssetSync<Material>("Prefabs/Fx/Materials/Yellow")) as Material;
		attackStarEffect = Material.Instantiate(assetMgr.GetAssetSync<Material>("Animation/Fx/Gems/Match/star")) as Material;
		colorEffect = new Dictionary<Data.TileTypes, Sprite>();
		colorEffect.Add(Data.TileTypes.Attack, attackEffect);
		colorEffect.Add(Data.TileTypes.Defend, defEffect);
		colorEffect.Add(Data.TileTypes.Gold, goldEffect);
		colorEffect.Add(Data.TileTypes.Heal, hpEffect);
		colorEffect.Add(Data.TileTypes.Skill, mpEffect);

		starEffect = new Dictionary<Data.TileTypes, Material>();
		starEffect.Add(Data.TileTypes.Attack, attackStarEffect);
		starEffect.Add(Data.TileTypes.Defend, defStarEffect);
		starEffect.Add(Data.TileTypes.Gold, goldStarEffect);
		starEffect.Add(Data.TileTypes.Heal, hpStarEffect);
		starEffect.Add(Data.TileTypes.Skill, mpStarEffect);

		flyCastPools = new Dictionary<Type, IPool<GameObject>>();
		flyCastPools.Add(typeof(PoisonSkill), poisonFlyPool);
		flyCastPools.Add(typeof(FireSkill), fireFlyPool);
		flyCastPools.Add(typeof(IceLockGemSkill), iceLockFlyPool);
		flyCastPools.Add(typeof(RockLockGemSkill), rockLockFlyPool);
		flyCastPools.Add(typeof(AutoRecoverSkill), autoRecoverFlyPool);
		flyCastPools.Add(typeof(VikingSetSkill), counterFlyPool);
		flyCastPools.Add(typeof(PaladinSetSkill), counterFlyPool);
	}

	public void OffsetFxLayer(Vector3 offset) {
		fxLayer.transform.localPosition += offset;
	}

	public void PlayChargeFx() {
		chargeFx = CreateSpineEffect(Effects.charge, FxAnimationName.begin, null, false, null, false);
		chargeFx.SetActive(true);
		chargeFx.transform.localScale = Vector3.one * minRatio;
		SkeletonAnimation anim = chargeFx.GetComponent<SkeletonAnimation>();
		anim.state.AddAnimation(0, FxAnimationName.idle, true, 0);
		chargeFx.transform.localPosition = UndoOffset(chargeFxPosition);
	}

	public void GrowChargeFx() {
		if (chargeFx == null) return;
		SkeletonAnimation anim = chargeFx.GetComponent<SkeletonAnimation>();
		float beginDuration = anim.state.GetCurrent(0).Animation.Duration;
		TweenParms parms = new TweenParms();
		Vector3 maxScale = Vector3.one * maxRatio;
		Vector3 currentScale = chargeFx.transform.localScale;
		Vector3 toScale = currentScale + Vector3.one * stepRatio;
		toScale = toScale.magnitude > maxScale.magnitude ? maxScale : toScale;
		parms.NewProp("localScale", toScale)/*.Delay(beginDuration)*/;
		HOTween.To(chargeFx.transform, 0.5f, parms);
		anim.state.AddAnimation(0, FxAnimationName.idle, true, 0);
	}

	public void EndChargeFx() {
		if (chargeFx != null) {
			//Logger.Trace("end charge");
			SkeletonAnimation anim = chargeFx.GetComponent<SkeletonAnimation>();
			anim.state.SetAnimation(0, FxAnimationName.end, false);
			anim.state.End += null;
			anim.state.End += new AnimationState.StartEndDelegate(delegate(AnimationState state, int trackIndex)
			{
				chargeFx.transform.localScale = Vector3.one;
				chargeFx.SetActive(false);
				//Logger.Trace("return instance ", chargeFx.GetHashCode());
				spinePool.ReturnInstance(chargeFx);
				returnInstances.Remove(anim.state);
				chargeFx = null;
			});

		}
	}

	public delegate void OnSpineAnimationEndCallback();

	public GameObject CreateSpineEffect(string pathToAsset, string animationName, OnSpineAnimationEndCallback callback) {
		return CreateSpineEffect(pathToAsset, animationName, false, callback);
	}

	public GameObject CreateSpineEffect(string pathToAsset, string animationName, bool loop, OnSpineAnimationEndCallback callback) {
		return CreateSpineEffect(pathToAsset, animationName, null, loop, callback, true);
	}

	public GameObject CreateSpineEffect(string pathToAsset, string animationName, string skin, bool loop, OnSpineAnimationEndCallback callback, bool autoReturnInstance) {
		GameObject fxObject = spinePool.GetInstance();
		fxObject.transform.parent = fxLayer.transform;
		fxObject.transform.localPosition = Vector3.zero;
		fxObject.transform.localScale = Vector3.one;
		fxObject.SetActive(true);

		SkeletonDataAsset skeletonDataAsset = null;
		cachedSkeletonDataAssets.TryGetValue(pathToAsset, out skeletonDataAsset);
		if (skeletonDataAsset == null) {
			skeletonDataAsset = assetMgr.GetAssetSync<SkeletonDataAsset>(pathToAsset);
			cachedSkeletonDataAssets[pathToAsset] = skeletonDataAsset;
		}

		SkeletonAnimation anim = fxObject.GetComponent<SkeletonAnimation>();
		anim.skeletonDataAsset = skeletonDataAsset;
		anim.AnimationName = "";
		anim.Reset();
		anim.state.SetAnimation(0, animationName, loop);
		anim.timeScale = 1;

		anim.state.End += null;
		anim.state.End += new AnimationState.StartEndDelegate(delegate(AnimationState state, int trackIndex)
		{
			if (callback != null) {
				callback();
			}
			if (autoReturnInstance) {
				OnSpineAnimationEnd(state, trackIndex);
			}
		});

		if (skin != null) {
			anim.skeleton.SetSkin(skin);
			anim.skeleton.SetSlotsToSetupPose();
		}

		if (autoReturnInstance) {
			returnInstances.Add(anim.state, fxObject);
		}
		return fxObject;
	}

	public GameObject CreateLoopSpineEffct(string pathToAsset, string[] animationName, bool[] loop, OnSpineAnimationEndCallback callback) {
		GameObject fxObject = CreateSpineEffect(pathToAsset, animationName[0], null, loop[0], callback, false);
		SkeletonAnimation anim = fxObject.GetComponent<SkeletonAnimation>();
		for (int i = 1; i < animationName.Length; i++) {
			anim.state.AddAnimation(0, animationName[i], loop[i], 0);
		}
		fxObject.transform.parent = fxEndGame.transform;
		return fxObject;
	}

	public GameObject CreateMatchEffect(Vector3 position) {
		GameObject fxObject = this.CreateSpineEffect(Effects.match, FxAnimationName.active, null);
		fxObject.transform.localPosition = position;
		return fxObject;
	}

	public void ReturnSpineAnimationToPool(AnimationState state) {
		OnSpineAnimationEnd(state, 0);
	}

	public void ReturnSpineAnimationToPool(GameObject fxObject) {
		spinePool.ReturnInstance(fxObject);
		fxObject.SetActive(false);
	}

	private void OnSpineAnimationEnd(AnimationState state, int trackIndex) {
		GameObject fxObject = null;
		returnInstances.TryGetValue(state, out fxObject);
		if (fxObject != null) {
			//Logger.Trace("return instance ", fxObject.GetHashCode());
			spinePool.ReturnInstance(fxObject);
			fxObject.SetActive(false);
			foreach (KeyValuePair<AnimationState, GameObject> pair in returnInstances) {
				if (pair.Value == fxObject) {
					returnInstances.Remove(pair.Key);
					break;
				}
			}
			//Logger.Trace("returnInstances size ", returnInstances.Count);
		}
	}

	public GameObject CreateUiSpriteEffect(string spriteName) {
		GameObject fxObject = uiSpritePool.GetInstance();
		fxObject.transform.parent = fxLayer.transform;
		fxObject.transform.localPosition = Vector3.zero;
		fxObject.SetActive(true);

		UISprite sprite = fxObject.GetComponent<UISprite>();
		sprite.spriteName = spriteName;

		return fxObject;
	}

	private GameObject CreateParticleGameObject() {
		GameObject particleObject = particlePool.GetInstance();
		particleObject.transform.parent = fxLayer.transform;
		particleObject.SetActive(true);
		particleObject.transform.localScale = Vector3.one;
		ParticleSystem particleSystem = particleObject.transform.GetChild(0).GetComponent<ParticleSystem>();
		particleSystem.startSize = 90;
		particleSystem.Play();

		return particleObject;
	}

	/*public GameObject CreateGemFlyEffect(string spriteName, Vector3 sourcePosition, Vector3 destinationPosition) {
		GameObject fxObject = CreateParticleGameObject();
		fxObject.transform.localPosition = sourcePosition;

		TweenParms parms = new TweenParms();
		parms.Prop("localPosition", new PlugVector3Y(destinationPosition.y, EaseType.EaseOutSine, false));
		HOTween.To(fxObject.transform, 0.7f, parms);
		parms.NewProp("localPosition", new PlugVector3X(destinationPosition.x, EaseType.EaseOutBack, false));
		HOTween.To(fxObject.transform, 0.7f, parms);
		parms.NewProp("startSize", 0).Ease(EaseType.EaseInCubic);
		HOTween.To(fxObject.transform.particleSystem, 0.7f, parms);
		parms.NewProp("startSize", 0).Delay(0.9f).OnComplete(new TweenDelegate.TweenCallbackWParms(ReturnInstance), fxObject);
		HOTween.To(fxObject.transform.particleSystem, 0, parms);

		return fxObject;
	}*/

	public void CreateGemFlyEffect(List<MatchItem> destroyList) {
		Dictionary<Data.TileTypes, Data.TileTypes> types = new Dictionary<Data.TileTypes, Data.TileTypes>();
		for (int i = 0; i < destroyList.Count; i++) {
			MatchItem item = destroyList[i];
			types[item.cell.cellType] = item.cell.cellType;
			//Logger.Trace(item.cell.cellType);
		}

		//Logger.Trace("type count ", types.Count);
		List<TilePoint> createdCells = new List<TilePoint>();
		for (int i = 0; i < destroyList.Count; i++) {
			MatchItem tile = destroyList[i];
			if (createdCells.Contains(tile.point)) continue;
			createdCells.Add(tile.point);
			Data.TileTypes type = tile.cell.cellType;
			Vector3 destinationPosition = Vector3.one;
			Sprite mSprite = null;
			Material star = null;
			destinationPositions.TryGetValue(type, out destinationPosition);
			colorEffect.TryGetValue(type, out mSprite);
			starEffect.TryGetValue(type, out star);
			if (mSprite == null || star == null) {
				//Logger.Trace("gemfly null sprite for type ", type);
				continue;
			}
			GameObject fxObject = CreateParticleGameObject();
			fxObject.transform.localPosition = destroyList[i].transform.localPosition;
			fxObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
			fxObject.GetComponent<SpriteRenderer>().sprite = mSprite;

			fxObject.transform.Find("Tail").GetComponent<Renderer>().material = star;

			TweenParams tParams = new TweenParams();
			tParams.SetDelay(i * 0.075f).SetEase(Ease.Linear);
			fxObject.transform.DOLocalMoveY(destinationPosition.y, 0.7f).SetAs(tParams);

			tParams = new TweenParams();
			tParams.SetDelay(i * 0.075f).SetEase(Ease.InCubic);
			fxObject.transform.DOLocalMoveX(destinationPosition.x - MatchLogic.offsetX, 0.7f).SetAs(tParams);

			tParams = new TweenParams();
			tParams.SetEase(Ease.InCirc).SetDelay(i * 0.075f);
			fxObject.transform.DOScale(Vector3.one * 0.3f, 0.7f).SetAs(tParams);

			tParams.Clear();
			tParams.SetEase(Ease.InCirc).SetDelay(i * 0.075f).OnComplete(() => CompleteMove(type));
			ParticleSystem ps = fxObject.transform.GetChild(0).GetComponent<ParticleSystem>();
			DOTween.To(() => ps.startSize, x => ps.startSize = x, 0, 0.9f).SetAs(tParams);

			tParams.Clear();
			tParams.SetDelay(0.7f + i * 0.075f).OnComplete(() => ReturnInstance(fxObject));
			DOTween.To(() => ps.startSize, x => ps.startSize = x, 0, 0).SetAs(tParams);
		}

		//Logger.Trace("finish create gem fly");
	}

	public float CreateSpreadFx() {
		Vector3 o = team2BeHitPosition;
		Vector3 a = o + new Vector3(-80, 50);
		Vector3 b = o + new Vector3(-120, -90);
		Vector3 c = o + new Vector3(-140, -30);
		Vector3 d = o + new Vector3(0, -130);
		Vector3 e = o + new Vector3(70, 20);
		Vector3 f = o + new Vector3(130, -70);
		Vector3 g = o + new Vector3(110, -130);
		Vector3[] pos = new[] { a, b, c, d, e, f, g };
		float time = 0;
		for (int i = 0; i < pos.Length; i++) {
			GameObject fxObject = CreateParticleGameObject();
			fxObject.transform.localPosition = o;
			fxObject.transform.localScale = new Vector3(1, 1, 1);
			fxObject.GetComponent<SpriteRenderer>().sprite = colorEffect[Data.TileTypes.Skill];

			fxObject.transform.Find("Tail").GetComponent<Renderer>().material = starEffect[Data.TileTypes.Skill];

			TweenParms parms = new TweenParms();
			parms.Prop("localPosition", new PlugVector3Path(new[] { o, pos[i], new Vector3(186, 875, 0) })).SpeedBased().Ease(EaseType.EaseInCirc);
			Tweener t = HOTween.To(fxObject.transform, 600, parms);
			t.GetPathLength();
			if (time < t.duration) time = t.duration;
			parms.NewProp("startSize", 0).Ease(EaseType.EaseInCirc);
			HOTween.To(fxObject.transform.GetChild(0).GetComponent<ParticleSystem>(), t.duration, parms);
			parms.NewProp("localScale", Vector3.one).Delay(t.duration).OnComplete(new TweenDelegate.TweenCallbackWParms(ReturnInstance), fxObject);
			HOTween.To(fxObject.transform, 0, parms);
		}
		return time;
	}

	public float CreateCastFlyEffect(Type type, Vector3[] path) {
		IPool<GameObject> pool = null;
		flyCastPools.TryGetValue(type, out pool);
		if (pool == null) {
			//Logger.Error("flyCastPool of type ", type, " is null");
			return -1;
		}
		GameObject fxObject = pool.GetInstance();
		fxObject.SetActive(true);
		Transform sprite = fxObject.transform.GetChild(0);
		fxObject.transform.parent = fxLayer.transform;
		ParticleSystem[] particleSystems = sprite.transform.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particleSystems.Length; i++) {
			//Logger.Trace("particleSystem", i, "play");
			particleSystems[i].Play();
		}
		fxObject.transform.localPosition = path[0];
		fxObject.transform.rotation = new Quaternion();

		TweenParms parms = new TweenParms();
		parms.NewProp("localPosition", new PlugVector3Path(path, PathType.Curved).OrientToPath(Axis.Z)).Loops(1).Ease(EaseType.Linear).SpeedBased().OnComplete(new TweenDelegate.TweenCallbackWParms(
			FlyCastReturnInstance), fxObject, type);
		Tweener t = HOTween.To(fxObject.transform, 700, parms);
		t.GetPathLength();
		return t.duration;
	}

	public float CreateAutoRecoverBackFx(Vector3[] path) {
		GameObject fxObject = autoRecoverFlyBackPool.GetInstance();
		fxObject.SetActive(true);
		Transform sprite = fxObject.transform.GetChild(0);
		fxObject.transform.parent = fxLayer.transform;
		ParticleSystem[] particleSystems = sprite.transform.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particleSystems.Length; i++) {
			//Logger.Trace("particleSystem", i, "play");
			particleSystems[i].Play();
		}
		fxObject.transform.localPosition = path[0];
		fxObject.transform.rotation = new Quaternion();

		TweenParms parms = new TweenParms();
		parms.NewProp("localPosition", new PlugVector3Path(path, PathType.Curved).OrientToPath(Axis.Z)).Loops(1).Ease(EaseType.Linear).SpeedBased().OnComplete(
			delegate()
			{
				particleSystems = sprite.transform.GetComponentsInChildren<ParticleSystem>();
				for (int i = 0; i < particleSystems.Length; i++) {
					particleSystems[i].Clear();
				}
				fxObject.SetActive(false);
				autoRecoverFlyBackPool.ReturnInstance(fxObject);
			});
		Tweener t = HOTween.To(fxObject.transform, 700, parms);
		t.GetPathLength();
		return t.duration;
	}

	public void CreateBeHitEffect(int beHitTeam) {
		Vector3 pos = GetBeHitTeamPosition(beHitTeam);
		pos = UndoOffset(pos);
		string effect = beHitTeam == BattleGameLogic.TEAM1 ? Effects.beHitCharacter : Effects.beHitMonster;
		GameObject fxObject = CreateSpineEffect(effect, FxAnimationName.active, null, false, null, true);
		fxObject.transform.localPosition = pos;
	}

	public void CreateMatch4Fx(MatchItem centerGem) {
		GameObject fxObject = CreateSpineEffect(Effects.match4Explosion, FxAnimationName.active, null, false, null, true);
		fxObject.transform.localPosition = centerGem.transform.localPosition;
	}

	private Vector3 UndoOffset(Vector3 positionToBeUndo) {
		return (positionToBeUndo - new Vector3(MatchLogic.offsetX, 0, 0));
	}

	private Vector3 GetBeHitTeamPosition(int beHitTeam) {
		if (beHitTeam == BattleGameLogic.TEAM1) {
			return team1BeHitPosition;
		}
		else if (beHitTeam == BattleGameLogic.TEAM2) {
			return team2BeHitPosition;
		}
		return Vector3.zero;
	}

	private Vector3 FindSourcePosition(List<MatchItem> destroyList, Data.TileTypes type) {
		int top = destroyList[0].point.y;
		int left = destroyList[0].point.x;
		int bot = destroyList[0].point.y;
		int right = destroyList[0].point.x;
		for (int i = 0; i < destroyList.Count; i++) {
			MatchItem item = destroyList[i];
			if (item.cell.cellType != type) {
				continue;
			}
			if (item.point.y > top) {
				top = item.point.y;
			}
			if (item.point.x < left) {
				left = item.point.x;
			}
			if (item.point.y < bot) {
				bot = item.point.y;
			}
			if (item.point.x > right) {
				right = item.point.x;
			}
		}
		//Logger.Trace(top, bot, left, right);

		int longestRowLength = 0;
		int longestColumnLength = 0;
		int longestRow = -1;
		int longestColumn = -1;

		for (int i = bot; i <= top; i++) {
			int rowLength = 0;
			for (int j = 0; j < destroyList.Count; j++) {
				MatchItem item = destroyList[j];
				if (item.point.y != i || item.cell.cellType != type) {
					continue;
				}
				rowLength++;
			}
			if (rowLength > longestRowLength) {
				longestRowLength = rowLength;
				longestRow = i;
			}
		}

		for (int i = left; i <= right; i++) {
			int columnLength = 0;
			for (int j = 0; j < destroyList.Count; j++) {
				MatchItem item = destroyList[j];
				if (item.point.x != i || item.cell.cellType != type) {
					continue;
				}
				columnLength++;
			}
			if (columnLength > longestColumnLength) {
				longestColumnLength = columnLength;
				longestColumn = i;
			}
		}
		for (int i = 0; i < destroyList.Count; i++) {
			if (destroyList[i].point.x == longestColumn && destroyList[i].point.y == longestRow) {
				return destroyList[i].transform.localPosition;
			}
		}
		//Logger.Trace(longestColumn, "x", longestRow);
		return Vector3.zero;
	}

	private void FlyCastReturnInstance(TweenEvent tweenEvent) {
		GameObject fxObject = (GameObject)tweenEvent.parms[0];
		ParticleSystem[] particleSystems = fxObject.transform.GetChild(0).GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < particleSystems.Length; i++) {
			//Logger.Trace("particleSystem", i, "clear");
			particleSystems[i].Clear();
		}
		fxObject.SetActive(false);
		flyCastPools[(Type)tweenEvent.parms[1]].ReturnInstance(fxObject);
	}

	private void ReturnInstance(GameObject fxObject)
	{
		fxObject.SetActive(false);
		fxObject.transform.GetChild(0).GetComponent<ParticleSystem>().Clear();
		particlePool.ReturnInstance(fxObject);
	}

	private void ReturnInstance(TweenEvent tweenEvent)
	{
		ReturnInstance(tweenEvent.parms[0] as GameObject);
	}

	public delegate void OnReturnInstance(GameObject fxGameObject);

	private IEnumerator WaitAndReturnInstance(IPool<GameObject> pool, GameObject instance, float delay, OnReturnInstance callback) {
		yield return new WaitForSeconds(delay);
		callback(instance);
		pool.ReturnInstance(instance);
	}

	public GameObject CreateSwapFx() {
		GameObject fxGameObject = CreateSpineEffect(Effects.swap, FxAnimationName.active, null, false, null, false);
		SkeletonAnimation animation = fxGameObject.GetComponent<SkeletonAnimation>();
		animation.state.AddAnimation(0, FxAnimationName.idle, true, 0);
		return fxGameObject;
	}

	public GameObject CreateAutoRecoverSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.adventurer, FxAnimationName.idle, "monster", false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team2AutoRecoverSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateAdventurerSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.adventurer, FxAnimationName.idle, "character", false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team2AdventurerSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateBronzeSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.bronze, FxAnimationName.active, null, false, null, false);
		SkeletonAnimation animation = fxGameObject.GetComponent<SkeletonAnimation>();
		animation.state.AddAnimation(0, FxAnimationName.idle, true, 0);
		fxGameObject.transform.localPosition = UndoOffset(team1BronzeSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateHunterSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.hunter, FxAnimationName.begin, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team2HunterSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateHunterSetSkillBeHitFx() {
		Vector3 pos = GetBeHitTeamPosition(BattleGameLogic.TEAM2);
		pos = UndoOffset(pos);
		string effect = Effects.beHitCharacter;
		GameObject fxObject = CreateSpineEffect(effect, FxAnimationName.active, null, false, null, true);
		fxObject.transform.localPosition = pos;
		return fxObject;
	}

	public GameObject CreateElvenSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.elven, FxAnimationName.begin, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team2AdventurerSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateKnightSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.knight, FxAnimationName.begin, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team1KnightSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateSamuraiSetSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.samurai, FxAnimationName.active, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team1SamuraiSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateAssassinSetSkillEffect(int order) {
		string slash = (order == 0) ? Effects.slashHorizontally : Effects.slashVertically;
		Vector3 pos = (order == 0) ? team1AssassinSetSkillPosition2 : team1AssassinSetSkillPosition;
		GameObject fxGameObject = CreateSpineEffect(slash, FxAnimationName.active, "default skill", false, null, true);
		fxGameObject.GetComponent<SkeletonAnimation>().timeScale = battleGameLogic.GameSpeed;
		fxGameObject.transform.localPosition = UndoOffset(pos);
		return fxGameObject;
	}

	public GameObject CreateVikingExplosionEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.viking, FxAnimationName.active, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(vikingSetSkillExplosionPosition);
		return fxGameObject;
	}

	public GameObject CreateVikingFireSlash() {
		GameObject fxGameObject = CreateSpineEffect(Effects.viking, FxAnimationName.active, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(vikingSetSkillFireSlashPosition);
		return fxGameObject;
	}

	public GameObject CreateVikingChargeFx() {
		GameObject fxGameObject = CreateSpineEffect(Effects.vikingCharge, FxAnimationName.active, null, false, null, false);
		SkeletonAnimation anim = fxGameObject.GetComponent<SkeletonAnimation>();
		anim.state.AddAnimation(0, FxAnimationName.idle, true, 0);
		fxGameObject.transform.localPosition = UndoOffset(vikingSetSkillChargePosition);
		return fxGameObject;
	}

	public float CreateVikingParticleChargeFx(Vector3 from, Vector3 to) {
		GameObject fxObject = CreateParticleGameObject();
		fxObject.transform.localPosition = from;
		fxObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
		fxObject.GetComponent<SpriteRenderer>().sprite = colorEffect[Data.TileTypes.Attack];

		fxObject.transform.Find("Tail").GetComponent<Renderer>().material = attackStarEffect;

		float flyDuration = 0;
		TweenParms parms = new TweenParms();
		parms.Prop("localPosition", to).SpeedBased();
		HOTween.To(fxObject.transform, 450, parms);
		flyDuration = (from - to).magnitude / 450;
		//Logger.Trace("magnitude", (from - to).magnitude, "duration", flyDuration);
		parms.NewProp("rotation", fxObject.transform.rotation).OnComplete(delegate()
		{
			fxObject.SetActive(false);
			fxObject.transform.GetChild(0).GetComponent<ParticleSystem>().Clear();
			particlePool.ReturnInstance(fxObject);
		}).Delay(flyDuration);
		HOTween.To(fxObject.transform, 0, parms);
		return flyDuration;
	}

	public GameObject CreateVikingHead(float duration) {
		GameObject fxGameObject = vikingSetSkillPool.GetInstance();
		fxGameObject.SetActive(true);
		fxGameObject.transform.parent = fxLayer.transform;
		fxGameObject.transform.localPosition = UndoOffset(vikingSetSkillParticleStartPosition);

		TweenParms parms = new TweenParms();
		parms.NewProp("localPosition", vikingSetSkillParticleStopPosition).Ease(EaseType.Linear).OnComplete(delegate()
		{
			fxGameObject.GetComponent<ParticleSystem>().Clear();
			fxGameObject.SetActive(false);
			vikingSetSkillPool.ReturnInstance(fxGameObject);
		});
		HOTween.To(fxGameObject.transform, duration, parms);

		return fxGameObject;
	}

	public GameObject CreatePaladinSkillEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.paladin, FxAnimationName.active, null, false, null, true);
		fxGameObject.transform.localPosition = UndoOffset(team1PaladinSetSkillPosition);
		return fxGameObject;
	}

	public GameObject CreateRedSlashEffect() {
		GameObject fxGameObject = CreateSpineEffect(Effects.slashVertically, FxAnimationName.active, "default skill", false, null, true);
		fxGameObject.GetComponent<SkeletonAnimation>().timeScale = battleGameLogic.GameSpeed;
		fxGameObject.transform.localPosition = UndoOffset(characterCastPosition);
		return fxGameObject;
	}

	public GameObject CreateTlFx(Vector3 from, Vector3 to, Vector3 rotation) {
		GameObject fxObject = matchTlPool.GetInstance();
		fxObject.SetActive(true);
		fxObject.transform.parent = fxLayer.transform;
		fxObject.transform.eulerAngles = rotation;
		fxObject.transform.localPosition = from;

		TweenParms parm = new TweenParms();
		parm.NewProp("localPosition", to).SpeedBased().OnComplete(delegate()
		{
			ParticleSystem[] particleSystems = fxObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < particleSystems.Length; i++) {
				//Logger.Trace("clear particle system", i);
				particleSystems[i].Clear();
			}
			matchTlPool.ReturnInstance(fxObject);
			fxObject.SetActive(false);
		});
		HOTween.To(fxObject.transform, 850f, parm);

		return fxObject;
	}

	public float CreatePreSkillFx(string texture, List<Color> colors) {
		float skillPicAnimationDuration = .5f;
		float skillPicShowupDuration = .75f;
		float fadeoutDuration = .25f;
		float fadeoutDelay = skillPicAnimationDuration + skillPicShowupDuration;
		float scaleDuration = .5f;
		float duration = skillPicAnimationDuration + skillPicShowupDuration + fadeoutDuration;
		float fromAlpha = 0;
		float toAlpha = 0.95f;
		GameObject dim = spritePool.GetInstance();
		dim.SetActive(true);
		GameObject pic = spritePool.GetInstance();
		pic.SetActive(true);
		GameObject lines = linesUpwardsPool.GetInstance();
		lines.SetActive(true);
		lines.transform.parent = fxLayer.transform;
		lines.transform.localPosition = new Vector3(lines.transform.localPosition.x, lines.transform.localPosition.y, -50);
		lines.transform.localPosition = UndoOffset(lines.transform.localPosition);
		ParticleAnimator pAnim = lines.GetComponent<ParticleAnimator>();
		pAnim.colorAnimation = colors.ToArray();

		dim.transform.localScale = new Vector3(100, 120, 0);
		dim.transform.parent = fxLayer.transform;
		dim.transform.localPosition = UndoOffset(new Vector3(315, 475, -10));
		SpriteRenderer dimRenderer = dim.GetComponent<SpriteRenderer>();
		dimRenderer.sprite = assetMgr.GetAssetSync<Sprite>("Textures/Home/bga");
		dimRenderer.sortingOrder = 0;
		dimRenderer.color = new Color(dimRenderer.color.r, dimRenderer.color.g, dimRenderer.color.b, fromAlpha);
		FloatTweenHolder holder = new FloatTweenHolder(fromAlpha, dimRenderer, delegate(float value)
		{
			dimRenderer.color = new Color(dimRenderer.color.r, dimRenderer.color.g, dimRenderer.color.b, value);
		});
		TweenParms parms = new TweenParms();
		parms.NewProp("value", new PlugFloat(toAlpha, EaseType.Linear)).OnUpdate(holder.OnUpdate);
		HOTween.To(holder, skillPicAnimationDuration, parms);

		pic.transform.localScale = Vector3.one;
		pic.transform.localPosition = Vector3.zero;
		pic.transform.parent = fxLayer.transform;
		SpriteRenderer picRenderer = pic.GetComponent<SpriteRenderer>();
		picRenderer.sprite = assetMgr.GetAssetSync<Sprite>("Textures/PreSkill/" + texture);
		picRenderer.sortingOrder = 1;
		picRenderer.color = new Color(picRenderer.color.r, picRenderer.color.g, picRenderer.color.b, 1);
		int spriteHeight = picRenderer.sprite.texture.height;
		pic.transform.localPosition = UndoOffset(Vector3.zero + new Vector3(315, 960 / 2 - spriteHeight));
		Vector3 p = UndoOffset(new Vector3(315, 475, 0));
		parms = new TweenParms();
		parms.NewProp("localPosition", p).Ease(EaseType.Linear);
		HOTween.To(pic.transform, skillPicAnimationDuration, parms);

		holder = new FloatTweenHolder(1, dimRenderer, delegate(float value)
		{
			//Logger.Trace("value", value);
			dimRenderer.color = new Color(dimRenderer.color.r, dimRenderer.color.g, dimRenderer.color.b, value);
		});
		parms = new TweenParms();
		parms.NewProp("value", new PlugFloat(0, EaseType.Linear)).OnUpdate(holder.OnUpdate).Delay(fadeoutDelay).OnComplete(delegate()
		{
			lines.SetActive(false);
			dim.SetActive(false);
			pic.SetActive(false);
			linesUpwardsPool.ReturnInstance(lines);
			spritePool.ReturnInstance(dim);
			spritePool.ReturnInstance(pic);
		});
		HOTween.To(holder, fadeoutDuration, parms);

		FloatTweenHolder holder2 = new FloatTweenHolder(1, picRenderer, delegate(float value)
		{
			picRenderer.color = new Color(picRenderer.color.r, picRenderer.color.g, picRenderer.color.b, value);
			Color[] colors2 = pAnim.colorAnimation;
			for (int i = 0; i < colors2.Length; i++) {
				Color c = colors2[i];
				colors2[i] = new Color(c.r, c.g, c.b, value);
				//Logger.Trace(particlesColors[i]);
			}
			pAnim.colorAnimation = colors2;
		});
		parms = new TweenParms();
		parms.NewProp("value", new PlugFloat(0, EaseType.Linear)).OnUpdate(holder2.OnUpdate).Delay(fadeoutDelay);
		HOTween.To(holder2, fadeoutDuration, parms);

		parms = new TweenParms();
		parms.NewProp("localScale", new Vector3(5, 5, 0)).Ease(EaseType.Linear).Delay(fadeoutDelay);
		HOTween.To(pic.transform, scaleDuration, parms);

		return duration;
	}

	void CompleteMove(Data.TileTypes type)
	{
		if (type == Data.TileTypes.Gold)
			textManager.AddGoldList();
	}

	private void OnGuiBeingDisplayed(bool displayed) {
		fxLayer.SetActive(!displayed);
	}
}

public class FloatTweenHolder {
	public delegate void OnValueUpdate(float value);
	public object targetObj;
	public float value;
	private OnValueUpdate callback;
	public FloatTweenHolder(float initialValue, object target, OnValueUpdate callback) {
		this.targetObj = target;
		this.callback = callback;
		this.value = initialValue;
	}

	public void OnUpdate() {
		callback(value);
	}
}