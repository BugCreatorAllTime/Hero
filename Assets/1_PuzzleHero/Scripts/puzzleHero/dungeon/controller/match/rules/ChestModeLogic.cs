using System.Collections;
using System.Collections.Generic;
using Nfury.Base;
using strange.examples.strangerocks;
using UnityEngine;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using AnimationState = Spine.AnimationState;

public class ChestModeLogic
{
	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleGameLogic { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject(DungeonContext.DUNGEON)]
	public GameObject dunObj { get; set; }

	[Inject]
	public RemainingMovesChangedSignal remainingMovesChangedSignal { get; set; }

	[Inject(GuiObjectName.victoryPopup)]
	public GameObject victoryPopup { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public TutorialStateSignal tutStateSignal {get;set;}

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	private MatchItem chest;
	private int chestId;
	private string sprite;
	private BattleEntity chestEntity;
	private BattleAction waitForHudAndChestSetupCompleteAction;
	private int chestColumn = -1;
	private int chestRow = -1;

	public bool ElapseTurn(int turn)
	{
		dungeonState.remainingMoves--;
		//Logger.Trace("remaining moves", dungeonState.remainingMoves);
		if (dungeonState.remainingMoves < 0)
		{
			return false;
		}
		SetRemainingMoves();
		if (chest.point.y == Data.tileHeight - 1)
		{
			SkeletonAnimation anim = chestEntity.gObject.GetComponent<SkeletonAnimation>();
			anim.state.SetAnimation(0, AnimationName.Dead, false);
			anim.state.End += new AnimationState.StartEndDelegate(OnChestDeadAnimationEnd);
			((MatchLogic)matchLogic).DisableBoardInput();
			return true;
		}
		if (dungeonState.remainingMoves == 0)
		{
			guiEventHandler.ShowBuyMovePopup();
			return true;
		}
		return true;
	}

	private void OnChestDeadAnimationEnd(AnimationState state, int trackIndex)
	{
		((MatchLogic) matchLogic).routineRunner.StartCoroutine(DelayedWin(1));
	}

	private IEnumerator DelayedWin(float delay)
	{
		yield return new WaitForSeconds(delay);
		dungeonState.DefeatMonster(chestId, 1);
		((BattleGameLogic)battleGameLogic).Win();
	}

	public bool ElapseOneTurn()
	{
		return ElapseTurn(1);
	}

	public void Begin(MonsterCfgImpl cfgData) {
		CreateChest(cfgData);
		if (!configManager.UserData.restoreDungeonState) {
			ChangePlayModeToChestMode(cfgData);
		}
	}

	private bool FindColumnOfRestoredChest()
	{
		MatchLogic mLogic = ((MatchLogic) matchLogic);
		for (int i = 0; i < mLogic.tiles.Count; i++)
		{
			MatchItem item = mLogic.tiles[i];
			if (item.cell.cellType == Data.TileTypes.Chest)
			{
				chestColumn = item.point.x;
				chestRow = item.point.y;
				return true;
			}
		}
		return false;
	}

	private void CreateChest(MonsterCfgImpl cfgData) {
		BattleGameLogic battleGameLogic = this.battleGameLogic as BattleGameLogic;
		GameObject monster = GameObject.Instantiate(assetMgr.GetAssetSync<GameObject>("Prefabs/Battle/SimpleObj")) as GameObject;
		string[] arrStr = cfgData.Ani.Split('/');
		//Logger.Trace(arrStr);
		SkeletonDataAsset skele = assetMgr.GetAssetSync<SkeletonDataAsset>("Animation/" + arrStr[0] + "/" + arrStr[0] + ".ske");
		SkeletonAnimation skeletonAnimation = monster.GetComponent<SkeletonAnimation>();
		skeletonAnimation.skeletonDataAsset = skele;
		skeletonAnimation.skeletonDataAsset.Reset();
		skeletonAnimation.Reset();
		if (arrStr.Length > 1) {
			skeletonAnimation.skeleton.SetSkin(arrStr[1]);
			skeletonAnimation.skeleton.SetSlotsToSetupPose();
			sprite = arrStr[2];
		}
		skeletonAnimation.state.SetAnimation(0, AnimationName.Idle, true);

		monster.transform.parent = dunObj.transform;
		monster.name = cfgData.Name;
		monster.transform.position = new Vector2(800, 105);

		chestEntity = new BattleEntity(monster);
		chestEntity.Team = BattleGameLogic.TEAM2;
		chestEntity.CurTurn = cfgData.Stat.turn;

		battleGameLogic.AddObject(chestEntity);

		battleGameLogic.OnEnemySpottedEvent += new BattleGameLogic.OnEnemySpotted(OnChestFound);
		routineRunner.StartCoroutine(battleGameLogic.RunToFindEnemy());

		chestId = cfgData.MonsterId;
	}

	private void OnChestFound(BattleEntity chest) {
//		Logger.Trace("Chest found!!!!!!!!!!!!");
		waitForHudAndChestSetupCompleteAction = battlePhaseManager.AddNeutralAction();
		routineRunner.StartCoroutine(WaitAndCreateChestOnBoard(0.75f));
		MoveHudHorizontally ();
	}

	private void MoveHudHorizontally()
	{
		TweenParms parms = new TweenParms();
		parms.Prop ("localPosition", new Vector3(-350, 0, 0)).Ease (EaseType.EaseOutSine);
		HOTween.To(GameObject.Find("HudCharacter").transform, 1.5f, parms);

		TweenParms parms2 = new TweenParms();
		parms2.Prop ("localPosition", new Vector3(350, 0, 0)).Ease (EaseType.EaseOutSine);
		HOTween.To(GameObject.Find("HudMonster").transform, 1.5f, parms2);
	}

	private IEnumerator WaitAndCreateChestOnBoard(float delay)
	{
		yield return new WaitForSeconds(delay);
		SetRemainingMoves();
		yield return new WaitForSeconds(delay);
		bool found = FindColumnOfRestoredChest();
		if (!found)
		{
			chestColumn = UnityEngine.Random.Range(0, Data.tileWidth);
		}
		MatchLogic mLogic = ((MatchLogic) matchLogic);
		int chestRow = (this.chestRow > 0) ? this.chestRow : 0;
		MatchItem item = mLogic.FindTile(new TilePoint(chestColumn, chestRow));
		string skin = item.cell.cellType.ToString();
		if (found)
		{
			mLogic.routineRunner.StartCoroutine(OnGemTransformInFinish(0.1f, item.transform.localPosition));
		}
		else
		{
			GameObject go = effectsManager.CreateSpineEffect(Effects.transform, FxAnimationName.active, skin, false, null, true);
			go.transform.localScale = MatchLogic.cellScale;
			go.transform.localPosition = item.transform.localPosition;
			item.GetComponent<UISprite>().spriteName = null;
			SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
			float duration = anim.state.GetCurrent(0).Animation.Duration;
			mLogic.routineRunner.StartCoroutine(OnGemTransformInFinish(duration + 0.1f, item.transform.localPosition));
		}
//		Logger.Trace("column ", column, " sprite ", sprite);
	}

	private IEnumerator OnGemTransformInFinish(float delay, Vector3 pos)
	{
		yield return new WaitForSeconds(delay);
		GameObject go = effectsManager.CreateSpineEffect(Effects.transform, FxAnimationName.end, sprite, false, null, true);
		go.transform.localScale = MatchLogic.cellScale;
		go.transform.localPosition = pos;
		SkeletonAnimation anim = go.GetComponent<SkeletonAnimation>();
		anim.state.End += new AnimationState.StartEndDelegate(delegate(AnimationState state, int trackIndex)
		{
			int chestRow = (this.chestRow > 0) ? this.chestRow : 0;
			chest = ((MatchLogic)matchLogic).ChangeCellType(chestColumn, chestRow, Data.TileTypes.Chest, sprite);
			chest.transform.name = "ChestGem";
			if (waitForHudAndChestSetupCompleteAction != null) {
				waitForHudAndChestSetupCompleteAction.state = BattleAction.ActionState.Finished;
				tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_FRIST_BRING, "");
			}
		});
	}

	public void SetRemainingMoves()
	{
		remainingMovesChangedSignal.Dispatch("MOVES LEFT: " + dungeonState.remainingMoves);
	}


	public void AddMove(int number)
	{
		dungeonState.remainingMoves = number;
	}

	private void ChangePlayModeToChestMode(MonsterCfgImpl cfgData) {
		dungeonState.playMode = PlayMode.Chest;
		dungeonState.remainingMoves = Mathf.Abs(cfgData.Stat.turn);
	}
}