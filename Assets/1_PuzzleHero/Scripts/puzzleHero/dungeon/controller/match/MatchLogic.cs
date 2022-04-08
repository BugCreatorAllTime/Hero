using System.Linq;
using Nfury.Base;
using System.Collections;
using System.Collections.Generic;
using strange.examples.strangerocks;
using strange.extensions.pool.api;
using UnityEngine;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using System;
using System.Security.Cryptography;
using DG.Tweening;

public class MatchLogic : AbstractGameLogic
{
	public const float targetScreenWidth = 640;
	public const float targetScreenHeight = 960;
	public const float targetCellWidth = 90;
	public const float targetCellHeight = 90;
	public static Vector3 cellScale = Vector3.one;
	public static float cellScaleX = 1;
	public static float cellScaleY = 1;
	public static float cellScaleZ = 1;
	public static float cellWidth = 90;
	public const int cellHeight = 90;
	public static float offsetX = 0;
	public const float TIME_PLAY_SOUND_GEM_DOWN = 0.3f;

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject(Prefabs.gem)]
	public IPool<GameObject> gemPool { get; set; }

	[Inject]
	public GameInput gameInput { get; set; }

	[Inject]
	public AddActionSignal actionSignal { get; set; }

	[Inject]
	public Team1StartAttackSignal startAttackSignal { get; set; }

	[Inject]
	public EffectsManager effectsManager { get; set; }

	[Inject]
	public AssetMgr assetMgr { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public ChestModeLogic chestModeLogic { get; set; }

	[Inject]
	public CalculatorComboSignal comboSinal{ get; set;}

	[Inject]
	public TutorialFirstBattleLogic tutLogic {get;set;}

	[Inject]
	public ConfigManager config {get;set;}

	[Inject]
	public TutorialStateSignal tutStateSignal {get;set;}

	[Inject]
	public DungeonService dungeonService { get; set; }

	[Inject]
	public SoundManager soundMng { get; set;}

	[Inject]
	public TextFxManager textFxManager { get; set; }

	[Inject]
	public TutorialFirstBattleLogic tutorialFirstBattleLogic { get; set; }

	[Inject]
	public PreloadedAssets preloadedAssets { get; set; }
	
	public BoardBgInfo boardInfo;
	public delegate void OnMatchingFinish();
	public event OnMatchingFinish OnMatchingFinishEvent;

	public delegate void OnMatched(List<MatchItem> matchedItems);
	public event OnMatched OnMatchedEvent;

	public delegate void OnBoardLocked(bool locked);
	public event OnBoardLocked OnBoardLockedEvent;

	public string[] sprites = new string[] { "Attack", "Defend", "Skill", "Heal", "Gold" , "key"};
	public List<MatchItem> tiles;
	public Cell[,] cells = new Cell[Data.tileWidth, Data.tileHeight];
	public BoardBackgroundLogic boardBackgroundLogic;

	private Camera boardCamera;
	private GameObject boardBackgrounds;
	public GameObject grid;
	private GameObject gem;

	private UISprite choice;
	private MatchItem curTile = null;

	private bool isDoing = false;
	private bool isBoardLocked = false;
	private bool isMatchFromUserInput;
	private Shuffle shuffle;
	private Matches matches;
	private ComboBonus comboBonus;
	private Dictionary<TilePoint, Data.TileTypes> predefine;
	private Hint hint;
	private float timeGemDown;

	public Cell[,] Cells { get { return cells; } }

	protected override void OnInit()
	{
		base.OnInit();
		ScaleBoard();
		InitObjects();
		Start();
	}

	private void InitObjects()
	{
		boardCamera = GameObject.FindWithTag(ObjectTag.boardCamera).GetComponent<Camera>();
		boardBackgrounds = GameObject.FindWithTag(ObjectTag.boardBackgrounds);
		grid = GameObject.FindWithTag(ObjectTag.grid);
		choice = GameObject.FindWithTag(ObjectTag.selector).GetComponent<UISprite>();
//		gemPool.size = Data.tileWidth * Data.tileHeight;
		gemPool.inflationType = PoolInflationType.INCREMENT;
//		gem = gemPool.GetInstance();
		gameInput.Drag += new GameInput.Dragged(OnDrag);
		gameInput.FingerDown += new GameInput.FingerDowned(OnFingerDown);

		boardBackgroundLogic = new BoardBackgroundLogic(this, assetMgr, boardBackgrounds, preloadedAssets);
		boardBackgroundLogic.InitObjects(gemPool);
		boardBackgroundLogic.UpdateAssets(boardInfo);

		shuffle = new Shuffle(this);
		matches = new Matches(this, effectsManager);
		comboBonus = new ComboBonus(this);
		if(predefine == null) predefine = new Dictionary<TilePoint, Data.TileTypes>();
		hint = new Hint(this, choice);
//		Predefine();
	}

	public void Predefine(Dictionary<TilePoint, Data.TileTypes> predefineGems)
	{
		this.predefine = predefineGems;
	}

	private void Predefine()
	{
//		predefine.Add(new TilePoint(4, 1), Data.TileTypes.Attack);
////		predefine.Add(new TilePoint(4, 2), Data.TileTypes.Gold);
////		predefine.Add(new TilePoint(4, 3), Data.TileTypes.Gold);
////		predefine.Add(new TilePoint(4, 4), Data.TileTypes.Gold);
//		predefine.Add(new TilePoint(3, 1), Data.TileTypes.Attack);
//		predefine.Add(new TilePoint(2, 1), Data.TileTypes.Attack);
//		predefine.Add(new TilePoint(5, 1), Data.TileTypes.Attack);
//		predefine.Add(new TilePoint(6, 1), Data.TileTypes.Attack);
//		predefine.Add(new TilePoint(4, 1), Data.TileTypes.Attack);
//		predefine.Add(new TilePoint(6, 0), Data.TileTypes.Heal);
		predefine.Add(new TilePoint(0, 0), Data.TileTypes.Gold);
		predefine.Add(new TilePoint(1, 0), Data.TileTypes.Gold);
		predefine.Add(new TilePoint(2, 0), Data.TileTypes.Gold);
		predefine.Add(new TilePoint(2, 2), Data.TileTypes.Attack);
		predefine.Add(new TilePoint(2, 3), Data.TileTypes.Attack);
		predefine.Add(new TilePoint(2, 4), Data.TileTypes.Attack);
		predefine.Add(new TilePoint(3, 2), Data.TileTypes.Attack);
		predefine.Add(new TilePoint(4, 2), Data.TileTypes.Attack);
	}

	public int ComboCount()
	{
		return comboBonus.GetComboCount();
	}

	public void CountCombo()
	{
		comboSinal.Dispatch (true);
		comboBonus.CountCombo();
//		soundMng.PlaySound (SoundName.COMBO);
	}

	public void ResetCombo()
	{
		comboSinal.Dispatch (false);
		comboBonus.ResetCombo();
	}

	public bool IsUserFirstMatch()
	{
		return comboBonus.IsUserFirstMatch() && isMatchFromUserInput;
	}

	public void StoreFirstMatchDamage(int damage)
	{
		comboBonus.FirstMatchDamage = damage;
	}

	public int GetBonusDamage()
	{
		return comboBonus.BonusDamage;
	}

	public void Hint()
	{
		if (tutorialFirstBattleLogic.CheckTutorialProgress()) return;
		hint.DoHint();
	}

	public void Unhint()
	{
		hint.Unhint();
	}

	private void ScaleBoard()
	{
		GameObject board = GameObject.FindWithTag(ObjectTag.board);
		cellScaleX = ((float)Screen.width / (float)Screen.height) / (targetScreenWidth / targetScreenHeight);
		cellWidth = targetCellWidth * cellScaleX;
		offsetX = (targetScreenWidth - (targetScreenWidth * cellScaleX)) / 2;
		board.transform.localPosition += new Vector3(offsetX, 0, 0);
		cellScale = new Vector3(1 * cellScaleX, 1 * cellScaleY, 1 * cellScaleZ);
		effectsManager.OffsetFxLayer(new Vector3(offsetX, 0, 0));
	}

	public void DisableBoardInput()
	{
		if(isBoardLocked) return;
//		Logger.Trace("disableBoardInput");
		isBoardLocked = true;
		NotifyBoardLockedStatus();
	}
	public void EnableBoardInput()
	{
		if(!isBoardLocked) return;
		isBoardLocked = false;
		if(tutLogic != null)
		{
			if(tutLogic.checkInput)
			{
				if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START &&
				   config.UserData.GetTutCollectGold() == 1)
				{
					tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_COLLECT_GOLD, tutLogic.GetSkill());
				}
				tutStateSignal.Dispatch(TutorialFirstBattleLogic.TUT_SHOW, tutLogic.GetSkill());
			}
		}
		NotifyBoardLockedStatus();
	}

	private void NotifyBoardLockedStatus()
	{
		if (OnBoardLockedEvent != null)
		{
			OnBoardLockedEvent(isBoardLocked);
		}
	}

	public bool IsBoardInputDisabled()
	{
//		return disableInputToken.HasObtained();
		return isBoardLocked;
	}

	public bool IsBoardStatic()
	{
		return !isDoing;
	}

	public void SetBoardStatic(bool isStatic)
	{
		//Logger.Trace("setBoardStatic", isStatic);
		isDoing = !isStatic;
	}

	public void SetMatchIsFromUserInput()
	{
		isMatchFromUserInput = true;
	}

	private void OnDrag(DragGesture gesture)
	{
//		Logger.Trace("matchLogic onDrag");
		Vector2 worldPosition = boardCamera.ScreenToWorldPoint(gesture.Position);
		Vector2 boardPosition = grid.transform.InverseTransformPoint(worldPosition);
		MatchItem dragTile = FindTileWithPosition(boardPosition);
		if (dragTile == curTile)
		{
			return;
		}
		OnClickAction(dragTile);
	}

	private void OnFingerDown(FingerDownEvent fingerDownEvent)
	{
		Vector2 worldPosition = boardCamera.ScreenToWorldPoint(fingerDownEvent.Position);
		Vector2 boardPosition = grid.transform.InverseTransformPoint(worldPosition);
//		Logger.Trace("matchLogic fingerDown");
//		Logger.Trace("screenPos ", fingerDownEvent.Position.x, fingerDownEvent.Position.y);
//		Logger.Trace("worldPos ", worldPosition.x * 480, worldPosition.y * 480);
//		Logger.Trace("boardPos ", boardPosition.x, boardPosition.y);
//		Logger.Trace("tile ", FindTileWithPosition(boardPosition));
		OnClickAction(FindTileWithPosition(boardPosition));
	}

	protected override void OnStart()
	{
		base.OnStart();
		this.Start();
	}

	protected override void OnFixedUpdate(float dt)
	{
//		Logger.Trace("MatchLogic fixedupdate");
	}

	protected override void OnUpdate(float dt)
	{
//		Logger.Trace("MatchLogic update");
	}

	public override void OnAddObject(Entity obj)
	{
	}

	public override void OnRemoveObject(Entity obj)
	{
	}

	// Init Tile Grid
	public void InitTileGrid() {
		for (int x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				cells[x, y] = new Cell();
				cells[x, y].SetRandomTile(Enum.GetNames(typeof(Data.TileTypes)).Length - 2);
			}
		}
		if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START &&
		   config.UserData.GetTutCollectGold() == 1)
		{
			cells[3, 0].cellType = Data.TileTypes.Gold;
			cells[3, 2].cellType = Data.TileTypes.Attack;
			cells[3, 1].cellType = Data.TileTypes.Gold;
			cells[4, 2].cellType = Data.TileTypes.Gold;
		}
	}

	public MatchItem ChangeCellType(int column, int row, Data.TileTypes newType, string sprite)
	{
		cells[column, row].cellType = newType;
		for (int i = 0; i < tiles.Count; i++)
		{
			MatchItem tile = tiles[i];
			if (tile.point.x == column && tile.point.y == row)
			{
				string spriteName = null;
				if (sprite == null)
				{
					int type = (int) cells[column, row].cellType;
					spriteName = sprites[(type - 1)];
				}
				else
				{
					spriteName = sprite;
				}
				tile.gameObject.GetComponent<UISprite>().spriteName = spriteName;
				return tile;
			}
		}
		return null;
	}

	int i = 0;
	// Display Tile Grid
	public void DisplayTileGrid() {
		tiles = new List<MatchItem>();
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				i++;
				int type = (int)cells[x, y].cellType;
				Data.TileTypes predefineType;
				if (predefine.ContainsKey(new TilePoint(x, y)))
				{
					type = (int) predefine[new TilePoint(x, y)];
					cells[x, y].cellType = predefine[new TilePoint(x, y)];
				}
				string spriteName = sprites[(type - 1)];
				GameObject instance = gemPool.GetInstance();
				instance.transform.parent = grid.transform;
				instance.GetComponent<UISprite>().spriteName = spriteName;
				instance.transform.localScale = cellScale;
				instance.transform.localPosition = new Vector3(x * cellWidth + cellWidth / 2, (Data.tileHeight - 1 - y) * cellHeight + cellHeight / 2, 0f);
				MatchItem tile = instance.GetComponent<MatchItem>();
//				tile.target = gameObject;
				tile.cell = cells[x, y];
				tile.point = new TilePoint(x, y);
				tiles.Add(tile);
			}
		}
	}

	// Create Tile Grid
	public void CreateTileGrid() {
		for (int x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				cells[x, y] = new Cell();
			}
		}
	}

	// Find Match-3 Tile
	public Dictionary<TilePoint, Data.TileTypes> FindMatch(Cell[,] cells) {
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) continue;
				int matchCount = 0;
				int y2 = Mathf.Min(Data.tileHeight - 1, y + 2);
				int y1;
				for (y1 = y + 1; y1 <= y2; y1++) {
					if (cells[x, y1].IsEmpty || thiscell.cellType != cells[x, y1].cellType) break;
					matchCount++;
				}
				if (matchCount >= 2) {
					y1 = Mathf.Min(Data.tileHeight - 1, y1 - 1);
					for (var y3 = y; y3 <= y1; y3++) {
						if (!stack.ContainsKey(new TilePoint(x, y3)))
							stack.Add(new TilePoint(x, y3), cells[x, y3].cellType);
					}
				}
			}
		}
		for (var y = 0; y < Data.tileHeight; y++) {
			for (var x = 0; x < Data.tileWidth; x++) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) continue;
				int matchCount = 0;
				int x2 = Mathf.Min(Data.tileWidth - 1, x + 3);
				int x1;
				for (x1 = x + 1; x1 <= x2; x1++) {
					if (cells[x1, y].IsEmpty || thiscell.cellType != cells[x1, y].cellType) break;
					matchCount++;
				}
				if (matchCount >= 2) {
					x1 = Mathf.Min(Data.tileWidth - 1, x1 - 1);
					for (var x3 = x; x3 <= x1; x3++) {
						if (!stack.ContainsKey(new TilePoint(x3, y)))
							stack.Add(new TilePoint(x3, y), cells[x3, y].cellType);
					}
				}
			}
		}
		return stack;
	}

	// Find Match-3 Tile Hint
	public Dictionary<TilePoint, Data.TileTypes> FindHint()
	{
		return FindHint(cells);
	}

	public Dictionary<TilePoint, Data.TileTypes> FindHint(Cell[,] cellss)
	{
		Dictionary<TilePoint, Data.TileTypes> stack = new Dictionary<TilePoint, Data.TileTypes>();
		Cell[,] clone = new Cell[Data.tileWidth, Data.tileHeight];
		for (var x = 0; x < Data.tileWidth - 1; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				System.Array.Copy(cellss, clone, Data.tileWidth * Data.tileHeight);
				MatchItem gem1 = FindTile(new TilePoint(x, y));
				MatchItem gem2 = FindTile(new TilePoint(x + 1, y));
				if (!gem1.Swappable() || !gem2.Swappable()) continue;
				var thiscell = clone[x, y];
				clone[x, y] = clone[x + 1, y];
				clone[x + 1, y] = thiscell;
				Dictionary<TilePoint, Data.TileTypes> st = new Dictionary<TilePoint, Data.TileTypes>();
				st = FindMatch(clone);
				if (st.Count > 0) {
					TilePoint tp = new TilePoint(x, y);
					if (!stack.ContainsKey(tp) && (st.ElementAt(0).Value != clone[x, y].cellType))
						stack.Add(tp, clone[x, y].cellType);
					tp = new TilePoint(x + 1, y);
					if (!stack.ContainsKey(tp) && (st.ElementAt(0).Value != clone[x + 1, y].cellType))
						stack.Add(tp, clone[x + 1, y].cellType);
				}
			}
		}
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight - 1; y++) {
				System.Array.Copy(cellss, clone, Data.tileWidth * Data.tileHeight);
				MatchItem gem1 = FindTile(new TilePoint(x, y));
				MatchItem gem2 = FindTile(new TilePoint(x, y + 1));
				if (!gem1.Swappable() || !gem2.Swappable()) continue;
				var thiscell = clone[x, y];
				clone[x, y] = clone[x, y + 1];
				clone[x, y + 1] = thiscell;
				Dictionary<TilePoint, Data.TileTypes> st = new Dictionary<TilePoint, Data.TileTypes>();
				st = FindMatch(clone);
				if (st.Count > 0) {
					TilePoint tp = new TilePoint(x, y);
					if (!stack.ContainsKey(tp) && (st.ElementAt(0).Value != clone[x, y].cellType))
						stack.Add(tp, clone[x, y].cellType);
					tp = new TilePoint(x, y + 1);
					if (!stack.ContainsKey(tp) && (st.ElementAt(0).Value != clone[x, y + 1].cellType))
						stack.Add(tp, clone[x, y + 1].cellType);
				}
			}
		}
		return stack;
	}

	// Do Empty Tile Move Down
	private void DoEmptyDown() {
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				var thiscell = cells[x, y];
				if (!thiscell.IsEmpty) continue;
				int y1;
				for (y1 = y; y1 > 0; y1--) {
					DoSwapTile(FindTile(new TilePoint(x, y1)), FindTile(new TilePoint(x, y1 - 1)));
				}
			}
		}
		for (var x = 0; x < Data.tileWidth; x++) {
			int y;
			for (y = Data.tileHeight - 1; y >= 0; y--) {
				var thiscell = cells[x, y];
				if (thiscell.IsEmpty) break;
			}
			if (y < 0) continue;
			var y1 = y;
			for (y = 0; y <= y1; y++) {
				MatchItem tile = FindTile(new TilePoint(x, y));
				tile.transform.localPosition = new Vector3(x * cellWidth + cellWidth / 2,
					(Data.tileHeight - 1 - (y - (y1 + 1))) * cellHeight + cellHeight / 2, 0f);
				tile.cell.SetRandomTile(Enum.GetNames(typeof(Data.TileTypes)).Length - 2);
				string spriteName = sprites[(int)tile.cell.cellType - 1];
				UISprite sprite = tile.GetComponent<UISprite>();
				sprite.spriteName = spriteName;
				sprite.enabled = true;
				sprite.transform.localScale = cellScale;
			}
		}
		List<TilePoint> fallingGems = new List<TilePoint>();
		foreach (MatchItem tile in tiles) {
			Vector3 pos = new Vector3(tile.point.x * cellWidth + cellWidth / 2, (Data.tileHeight - 1 - tile.point.y) * cellHeight + cellHeight / 2);
			float dist = Vector3.Distance(tile.transform.localPosition, pos)/* * 0.01f*/;
			if (dist > 0)
			{
				fallingGems.Add(tile.point);
			}
		}
		TilePoint point = new TilePoint();
		float delay = 0;
		float waitTime = 0.025f;
		for (int i = 0; i < Data.tileWidth;i++)
		{
			int m = 0;
			for (int k = Data.tileHeight - 1; k >= 0; k--)
			{
				point.x = i;
				point.y = k;
				MatchItem tile = FindTile(point);
				if (fallingGems.Contains(point))
				{
					Vector3 pos = new Vector3(tile.point.x * cellWidth + cellWidth / 2, (Data.tileHeight - 1 - tile.point.y) * cellHeight + cellHeight / 2);
					TweenParams tParams = new TweenParams().SetTarget(tile.transform).SetEase(Ease.InQuad).SetDelay(m * waitTime).OnComplete(new TweenCallback(PlaySoundGemDown));
					float fallTime = 0;
					float fallLength = Vector3.Distance(pos, tile.transform.localPosition);
					if (fallLength == 90)
					{
						fallTime = 0.3f;
					}else if (fallLength == 180)
					{
						fallTime = 0.4f;
					}
					else
					{
						fallTime = 0.45f;
					}
					tile.transform.DOLocalMove(pos, fallTime).SetAs(tParams);
					if (m * waitTime + fallTime > delay) delay = m * waitTime + fallTime;

					m++;
				}
			}
		}
		delay += 0.075f;
		routineRunner.StartCoroutine(CheckMatch3TileOnly(delay));
	}

	// Find Match-3 Tile
	public MatchItem FindTile(TilePoint point) {
		foreach (MatchItem tile in tiles) {
			if (tile.point.Equals(point)) return tile;
		}
		return null;
	}

	// Find Match-3 Tile with Position
	MatchItem FindTileWithPosition(Vector2 pos) {
		MatchItem selectedTile = null;
		if(tiles != null)
		{
			foreach (MatchItem tile in tiles)
			{
				Vector2 tilesCenter = tile.transform.localPosition;
				float distance = Vector2.Distance(tilesCenter, pos);
				if (distance <= cellWidth / 2)
				{
					selectedTile = tile;
				}
			}
		}
		return selectedTile;
	}

	// Swap Motion Animation
	void DoSwapMotion(Transform a, Transform b)
	{
		tutLogic.EmptyBlackList ();
		if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START &&
		   config.UserData.GetTutCollectGold() == 1)
		{
			config.UserData.SetTutCollectGold(0);
		}
		Vector3 originalScaleA = a.localScale;
		Vector3 originalScaleB = b.localScale;
		Vector3 scaleA = new Vector3(originalScaleA.x * 1.3f, originalScaleA.y * 1.3f, originalScaleA.z * 1);
		Vector3 scaleB = new Vector3(originalScaleB.x * 0.75f, originalScaleB.y * 0.75f, originalScaleB.z * 1);
		Vector3 posA = a.localPosition;
		Vector3 posB = b.localPosition;
		a.GetComponent<UISprite>().depth = 2;
		TweenParms parms = new TweenParms().Prop("localPosition", posB)
			.Ease(EaseType.EaseOutSine)
			.OnComplete(a.gameObject, "OnSwapMotionComplete", null, SendMessageOptions.DontRequireReceiver);
		HOTween.To(a, 0.3f, parms);
		parms.NewProp("localScale", scaleA).Ease(EaseType.EaseOutSine);
		HOTween.To(a, 0.15f, parms);
		parms.NewProp("localScale", originalScaleA).Ease(EaseType.EaseOutSine).Delay(0.15f);
		HOTween.To(a, 0.15f, parms);
		parms = new TweenParms().Prop("localPosition", posA)
			.Ease(EaseType.EaseOutSine)
			.OnComplete(a.gameObject, "OnSwapMotionComplete", null, SendMessageOptions.DontRequireReceiver);
		HOTween.To(b, 0.3f, parms);
		parms.NewProp("localScale", scaleB).Ease(EaseType.EaseOutSine);
		HOTween.To(b, 0.15f, parms);
		parms.NewProp("localScale", originalScaleB).Ease(EaseType.EaseOutSine).Delay(0.15f);
		HOTween.To(b, 0.15f, parms);
	}

	// Swap Two Tile
	public void DoSwapTile(MatchItem a, MatchItem b) {
		TilePoint p1 = a.point;
		TilePoint p2 = b.point;

		Cell cell = cells[p1.x, p1.y];
		cells[p1.x, p1.y] = cells[p2.x, p2.y];
		cells[p2.x, p2.y] = cell;

		a.point = p2;
		b.point = p1;
	}

	// Attack NPC Monster
	IEnumerator AttackMonster(float delayTime) {
//		pcControl.Attack();
		yield return new WaitForSeconds(delayTime);
//		npcControl.Damage();
	}

	// Fill Empty Tile
	public IEnumerator FillEmpty(float delayTime) {
		yield return new WaitForSeconds(delayTime);
		DoEmptyDown();
	}

	public void CheckMatch(Dictionary<TilePoint, Data.TileTypes> stack, MatchItem a, MatchItem b)
	{
		isDoing = true;
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(FindTile(item.Key));
		}

		List<List<TilePoint>> partitionedMatches = null;
		bool matchComplex = matches.IsMatchComplex(destroyList, out partitionedMatches);
//		Logger.Trace("match complex ", matchComplex);
		if (matchComplex)
		{
			
			matches.PerformComplexMatches(partitionedMatches, a, b);
		}
		else
		{
			CheckMatch3(stack);
		}
		soundMng.PlaySound (SoundName.MATCH_GEM);
	}

	// Check Match-3 Tile
	public void CheckMatch3(Dictionary<TilePoint, Data.TileTypes> stack)
	{
		isDoing = true;
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(FindTile(item.Key));
		}

		NotifyMatchedGems(destroyList);

		if (dungeonState.playMode == PlayMode.Monster)
		{
			DispatchActionSignals(destroyList);
		}

		effectsManager.CreateGemFlyEffect(destroyList);
		foreach (MatchItem item in destroyList) {
//			audioMatchSource[(int)(item.cell.cellType) - 1].Play();

			int type = (int)item.cell.cellType;

			item.cell.cellType = Data.TileTypes.Empty;
			TweenParams tParams = new TweenParams();
			tParams.SetEase(Ease.OutSine);
			item.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.1f).SetAs(tParams);
			tParams = new TweenParams();
			tParams.SetEase(Ease.InSine).SetDelay(0.1f);
			item.transform.DOScale(new Vector3(0, 0, 1), 0.2f).SetAs(tParams);

			Vector3 position = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y, 0);
			effectsManager.CreateMatchEffect(position);
		}
		//		StartCoroutine(AttackMonster(0.7f));
		routineRunner.StartCoroutine(FillEmpty(0.5f));
	}

	public void DispatchActionSignals(List<MatchItem> destroyList)
	{
		List<List<MatchItem>> columns = Matches.FindMatchedColumns(destroyList);
		List<List<MatchItem>> rows = Matches.FindMatchedRows(destroyList);

		for (int i = 0; i < columns.Count; i++)
		{
			List<MatchItem> item = columns[i];
			Data.TileTypes matchType = item[0].cell.cellType;
			int matchCount = item.Count;
			Matches.Shape shape = matchCount == 3
				? Matches.Shape.Three
				: (matchCount == 4 ? Matches.Shape.Four : Matches.Shape.Five);
			actionSignal.Dispatch(new Action(matchType, shape, matchCount));
		}

		for (int i = 0; i < rows.Count; i++)
		{
			List<MatchItem> item = rows[i];
			Data.TileTypes matchType = item[0].cell.cellType;
			int matchCount = item.Count;
			Matches.Shape shape = matchCount == 3
				? Matches.Shape.Three
				: (matchCount == 4 ? Matches.Shape.Four : Matches.Shape.Five);
			actionSignal.Dispatch(new Action(matchType, shape, matchCount));
		}
	}

	public void SetMatch(Dictionary<TilePoint, Data.TileTypes> stack)
	{
		isDoing = true;
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(FindTile(item.Key));
		}

		NotifyMatchedGems(destroyList);

		foreach (MatchItem item in destroyList) {
			int type = (int)item.cell.cellType;
			item.cell.cellType = Data.TileTypes.Empty;
			item.GetComponent<UISprite>().enabled = false;
		}
		routineRunner.StartCoroutine(FillEmpty(0.5f));
	}

	public void Counter(Dictionary<TilePoint, Data.TileTypes> stack)
	{
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(FindTile(item.Key));
		}

		NotifyCounteredGems(destroyList);
	}

	public void Counter(List<MatchItem> stack)
	{
		NotifyCounteredGems(stack);
	}

	public void TransformGems(Dictionary<TilePoint, Data.TileTypes> stack, float duration, Data.TileTypes type)
	{
		routineRunner.StartCoroutine(DoTransformGems(stack, duration, type));
	}

	private IEnumerator DoTransformGems(Dictionary<TilePoint, Data.TileTypes> stack, float duration, Data.TileTypes type)
	{
		List<MatchItem> destroyList = new List<MatchItem>();
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in stack) {
			destroyList.Add(FindTile(item.Key));
		}

		foreach (MatchItem item in destroyList) {
			item.GetComponent<UISprite>().enabled = false;
		}
		yield return new WaitForSeconds(duration);
		foreach (MatchItem item in destroyList) {
			item.cell.cellType = type;
			item.GetComponent<UISprite>().enabled = true;
			item.GetComponent<UISprite>().spriteName = type.ToString();
		}
		routineRunner.StartCoroutine(CheckMatch3TileOnly(0));
	}

	// Find Hint Debug
	void DebugFindHint() {
		Dictionary<TilePoint, Data.TileTypes> st = FindHint();
		string str = "";
		foreach (KeyValuePair<TilePoint, Data.TileTypes> item in st) {
			str += item.Key + ", ";
		}
		Debug.Log("FindHint: " + str);
	}

	public void Shuffle()
	{
		shuffle.DoShuffle();
	}

	// Ready Game Trun
	public void ReadyGameTurn() {
		isDoing = false;
		EnableBoardInput();
		isMatchFromUserInput = false;
		DebugFindHint();
	}

	private IEnumerator DelayEndMatch(float delay)
	{
		yield return new WaitForSeconds(delay);
		EndMatch();
	}

	private void EndMatch()
	{
		isDoing = false;
		//			Logger.Trace("matchLogic CheckMatch3TileOnly");
		if (OnMatchingFinishEvent != null) {
			OnMatchingFinishEvent();
		}
		if (dungeonState.playMode == PlayMode.Monster) {
			startAttackSignal.Dispatch(isMatchFromUserInput);
		}
		else {
			chestModeLogic.ElapseOneTurn();
		}
		//			Logger.Trace("startAttackSignal");
		if (isMatchFromUserInput) {
			isMatchFromUserInput = false;
			//			ReadyGameTurn();
		}
		ResetCombo();
		Hint();
	}

	// Check Only Match-3 Tile
	public IEnumerator CheckMatch3TileOnly(float delayTime) {
		yield return new WaitForSeconds(delayTime);
		Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
		if (stack.Count > 0) {
			if (isMatchFromUserInput) {
				CountCombo();
			}
			CheckMatch(stack, null, null);
		}
		else
		{
			if (FindHint(cells).Count > 0)
			{
				EndMatch();
			}
			else
			{
				//Logger.Trace("check m-3 only");
				NotifyCounteredGems(tiles);
				textFxManager.NoMoreMatches(delegate()
				{
					shuffle.DoShuffle();
					float delay = global::Shuffle.totalShufflingTime + 0.2f;
					routineRunner.StartCoroutine(DelayEndMatch(delay));
				});
			}
		}
	}

	// Check Match-3 Tile
	IEnumerator CheckMatch3Tile(float delayTime, MatchItem a, MatchItem b) {
		yield return new WaitForSeconds(delayTime);
		Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
		if (stack.Count > 0) {
			tutLogic.checkInput = true;
			tutLogic.EmptyBlackList();
			CheckMatch(stack, a, b);
		}
		else {
			Hint();
			DoSwapTile(a, b);
			DoSwapMotion(a.transform, b.transform);
			routineRunner.StartCoroutine(DelayedReadyGameTurn());
		}
	}

	public IEnumerator DelayedReadyGameTurn()
	{
		yield return new WaitForSeconds(0.3f);
		ReadyGameTurn();
	}

	// Click Event
	void OnClickAction(MatchItem tile) {
//		Debug.Log("OnClickAction");
		if (IsBoardInputDisabled()) return;
		if (guiEventHandler.IsGuiBeingDisplayed()) return;
		if (tile == null) return;

		if(InputTut(tile))
		{
			if (curTile == null) {
				SetCurTile(tile);
			}
			else {
				if (!Swappable(curTile, tile)) {
					curTile = null;
					choice.enabled = false;
					return;
				}
				Unhint();
				isDoing = true;
				isMatchFromUserInput = true;
				DisableBoardInput();
				DoSwapTile(curTile, tile);
				DoSwapMotion(curTile.transform, tile.transform);
				routineRunner.StartCoroutine(CheckMatch3Tile(0.4f, curTile, tile));
				curTile = null;
				choice.enabled = false;
			}
		}
	}

	private bool Swappable(MatchItem curTile, MatchItem tile)
	{
		return !(Mathf.Abs(curTile.point.x - tile.point.x) + Mathf.Abs(curTile.point.y - tile.point.y) != 1 || !curTile.Swappable() || !tile.Swappable());
	}

	void OnDragAction(MatchItem tile) {
//		Debug.Log("OnDragAction");
		if (IsBoardInputDisabled()) return;

		if (tile == null) return;
		if (curTile == null) {
			curTile = tile;
			choice.transform.localPosition = curTile.transform.localPosition;
			choice.enabled = true;
		}
		else if (tile == curTile) {
			Debug.Log("tile == curTile");
		}
		else {
			if (Mathf.Abs(curTile.point.x - tile.point.x) + Mathf.Abs(curTile.point.y - tile.point.y) != 1) {
				curTile = null;
				choice.enabled = false;
				return;
			}
//			isDoing = true;
			DisableBoardInput();
			DoSwapTile(curTile, tile);
			DoSwapMotion(curTile.transform, tile.transform);
			routineRunner.StartCoroutine(CheckMatch3Tile(0.35f, curTile, tile));
			curTile = null;
			choice.enabled = false;
		}
	}

	public void NotifyMatchedGems(List<MatchItem> destroyList)
	{
		if (OnMatchedEvent != null)
		{
			OnMatchedEvent(destroyList);
		}
	}

	private void NotifyCounteredGems(List<MatchItem> destroyList)
	{
		if (OnMatchedEvent != null)
		{
			OnMatchedEvent(destroyList);
		}
	}

	void GotoMenu() {
		Application.LoadLevel("Menu");
	}

	// Start Game
	private void Start() {
//		isDoing = false;
//		EnableBoardInput();
//		SetupAudioSource();
		choice.enabled = false;
//		CreateTileGrid();
		if(!tutLogic.CheckInitMatch(config.UserData))
		{
			while (true) {
				InitTileGrid();
				Dictionary<TilePoint, Data.TileTypes> stack = FindMatch(cells);
				if (stack.Count < 1) break;
			}
			DisplayTileGrid();
		}		
//		ReadyGameTurn();
	}

	public void SetCurTile(MatchItem tile)
	{
		curTile = tile;
		choice.transform.localPosition = curTile.transform.localPosition;
		choice.enabled = true;
	}

	private bool InputTut(MatchItem tile)
	{
		bool check = true;
		if(tutLogic.CheckPause(config.UserData))
		{
			GridTutorialData gridData;
			if(config.UserData.currentStepTownTutorial > TutorialFirstBattleLogic.START &&
			   config.UserData.GetTutCollectGold() == 1)
			{
				gridData = config.gridTutorialCfg.GetGridTutorialByIdTutorial(TutorialFirstBattleLogic.ID_TUT_COLLECT_GOLD);
			}
			else 
			{
				gridData = config.gridTutorialCfg.GetGridTutorialByIdTutorial(config.UserData.currentStepTownTutorial);
			}
			if(gridData != null)
			{
				check = false;
				for(int i = 0; i < gridData.SelectPosition.x.Count;i++)
				{
					if(tile.point.x == gridData.SelectPosition.x[i] && tile.point.y == gridData.SelectPosition.y[i])
					{
						check = true;
						break;
					}
				}
			}
		}
		return check;
	}

	public void PlaySound(int id)
	{
		soundMng.PlaySound (id);
	}

	private void PlaySoundGemDown()
	{
		if(timeGemDown <= 0)
		{
			timeGemDown = TIME_PLAY_SOUND_GEM_DOWN;
			soundMng.PlaySound (SoundName.GEM_DOWN);
			routineRunner.StartCoroutine(CountTimeGemDown());
		}
	}

	private IEnumerator CountTimeGemDown()
	{
		while(timeGemDown > 0)
		{
			timeGemDown -= Time.deltaTime;
			if(timeGemDown <= 0) timeGemDown = 0;
			yield return new WaitForEndOfFrame();
		}
	}
}