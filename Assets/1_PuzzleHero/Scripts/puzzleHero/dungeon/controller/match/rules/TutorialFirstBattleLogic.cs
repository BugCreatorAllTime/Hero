using UnityEngine;
using System.Collections;
using strange.examples.strangerocks;
using Nfury.Base;
using System.Collections.Generic;
using strange.extensions.pool.api;
using Holoville.HOTween;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;
using System.Linq;
using System;

public class TutorialFirstBattleLogic {

	[Inject(DungeonContext.HUD)]
	public GameObject hud { get; set; }

	[Inject(Prefabs.gem)]
	public IPool<GameObject> gemPool { get; set; }

	[Inject(Prefabs.blackImage)]
	public IPool<GameObject> blackPool { get; set; }

	[Inject(Prefabs.black)]
	public IPool<GameObject> blackBgPool { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set;}

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleLogic { get; set; }

	[Inject]
	public ConfigManager config {get;set;}

	[Inject]
	public TextFxManager textManager {get;set;}

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public CharacterSkillChangedSignal characterSkillChangedSignal { get; set; }

	[Inject]
	public SoundManager soundMng { get; set;}

	private Dictionary<int, Data.TileTypes> gemType;
	private Dictionary<Type, string> nameMonsterSkills;
	private Dictionary<string, HomeTownTutorialData> tutmonSkills;
	private List<GameObject> blackList;
	private List<GameObject> gemList = new List<GameObject>();
	private List<ImgDataShow> listParent = new List<ImgDataShow> ();
	private GameObject blackObject;
	public bool checkInput;
	public const float TIME_PLAY_SOUND_GEM_DOWN = 0.3f;
	public const int BEGIN = -3;
	public const int SHOW_TURN = 2;
	public const int TUT_MATCH_5 = 5;
	public const int TRANS = 6;
	public const int TUT_MANA = 7;
	public const int START = 9;
	public const int HOME_TUT = 10;
	public const int GO_WM = 22;
	public const int CLICK_SHOP = 23;
	public const int BUY_ITEM = 26;
	public const int END_SHOP = 27;
	public const int BLACKSMITH = 28;
	public const int RECEIVE_ITEM = 29;
	public const int END = 34;
	public const int FIRST_BRING = 35;
	public const int END_BRING = 38;
	public const int TUT_OPEN_CHEST = 39;
	public const int TUT_OPEN_END = 41;
	public const int ID_TUT_COLLECT_GOLD = 50;
	public const int BANNER_SHOW = 51;
	public const int TUT_SHOW = 1;
	public const int TUT_FRIST_BRING = 2;
	public const int TUT_BRING_END = 3;
	public const int TUT_FIRST_SKILL_MONSTER = 4;
	public const int TUT_COLLECT_GOLD = 5;

	public const string POISON = "Poison";
	public const string ICE = "Ice";
	public const string DESTROY = "Destroy";
	public const string TRANSFORM = "Transform";
	public const string SHUFFLE = "Shuffle";
	public const string HEAL = "Heal";
	public const string ROCK = "Rock";
	public const string FIRE = "Fire";
	public int TURN = 4;
	public int countTurn = 0;

	private string monsterSkill = "";
	public int current;
	public UserData uData;
	private float timeGemDown;
	
	public string GetSkill()
	{
		if (monsterSkill == null)
						return "";
		return monsterSkill;
	}

	[PostConstruct]
	public void PostConstruct()
	{
		TURN = config.general.NumberFreeTurn - 1;
		if(config.UserData.currentStepTownTutorial > START)
			checkInput = true;
		else checkInput = false;
		blackList = new List<GameObject> ();
		gemType = new Dictionary<int, Data.TileTypes>();
		gemType.Add(1, Data.TileTypes.Attack);
		gemType.Add(2, Data.TileTypes.Defend);
		gemType.Add(3, Data.TileTypes.Skill);
		gemType.Add(4, Data.TileTypes.Gold);
		gemType.Add(5, Data.TileTypes.Heal);

		nameMonsterSkills = new Dictionary<Type, string>();
		nameMonsterSkills.Add(typeof(PoisonSkill), POISON);
		nameMonsterSkills.Add(typeof(IceLockGemSkill), ICE);
		nameMonsterSkills.Add(typeof(DestroyGemSkill), DESTROY);
		nameMonsterSkills.Add(typeof(TransformGemSkill), TRANSFORM);
		nameMonsterSkills.Add(typeof(ShuffleMatchGemSkill), SHUFFLE);
		nameMonsterSkills.Add(typeof(AutoRecoverSkill), HEAL);
		nameMonsterSkills.Add(typeof(RockLockGemSkill), ROCK);
		nameMonsterSkills.Add(typeof(FireSkill), FIRE);


		tutmonSkills = new Dictionary<string, HomeTownTutorialData> ();
		for(int i = 0; i < nameMonsterSkills.Count; i++)
		{
			tutmonSkills.Add(nameMonsterSkills.ElementAt(i).Value, GetData(nameMonsterSkills.ElementAt(i).Value));
		}
	}

	public void SetNameMonsterSkill(Type type)
	{
		string nameSkill = "";
		nameMonsterSkills.TryGetValue(type, out nameSkill);
		if(nameSkill != "") monsterSkill = nameSkill;
	}

	public HomeTownTutorialData GetDataTut(string name)
	{
		HomeTownTutorialData data = null;
		tutmonSkills.TryGetValue (name, out data);
		return data;
	}

	private HomeTownTutorialData GetData(string name)
	{
		HomeTownTutorialData data = null;
		for(int i = 0; i < config.townTutorialCfg.homeTown.Count; i++)
		{
			if(name == config.townTutorialCfg.homeTown.ElementAt(i).Value.Target)
			{
				return config.townTutorialCfg.homeTown.ElementAt(i).Value;
			}
		}
		return data;
	}

	public bool CheckPause(UserData uData)
	{
		return ((uData.currentStepTownTutorial < START && (countTurn == 0 || countTurn > TURN))|| 
		         (uData.currentStepTownTutorial >= FIRST_BRING && uData.currentStepTownTutorial <= END_BRING) || 
		        (uData.GetTutCollectGold() == 1 && uData.currentStepTownTutorial > START));
	}

	public void FindToAlchemist()
	{
		if(config.UserData.currentStepTownTutorial == BEGIN)
		{
			hud.gameObject.SetActive (false);
			if(((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM2).gObject.transform.position.x < 1300)
			{
				config.UserData.NextStep();
				routineRunner.StartCoroutine(TalkPreSceneOne());
			}
		}
		if(config.UserData.currentStepTownTutorial == BEGIN+1)
		{
			if(((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM2).gObject.transform.position.x < 600)
			{
				((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1).ObjState = ObjectState.Idle;
				config.UserData.NextStep();
				routineRunner.StartCoroutine(TalkSceneOne());
			}
		}
	}

	public void StartSceneTwo()
	{
		((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1).ObjState = ObjectState.Idle;
		routineRunner.StartCoroutine(TalkSceneTwo());
	}

	public bool CheckRun(UserData uData)
	{
		return uData.currentStepTownTutorial != BEGIN+2 || uData.currentStepTownTutorial == BEGIN+1;
	}

	public bool CheckPreStartBattle(UserData uData)
	{
		return uData.currentStepTownTutorial == BEGIN+3 ;
	}

	public bool CheckInitMatch(UserData uData)
	{
		return uData.currentStepTownTutorial == BEGIN;
	}

	private IEnumerator TalkSceneTwo()
	{
		yield return new WaitForSeconds(0.5f);
		textManager.Talk (config.text.TextSceneTwoFirst, false,3.5f);
		yield return new WaitForSeconds(3.5f);
		textManager.Talk (config.text.TextSceneTwoSecond, true,2.5f);
		yield return new WaitForSeconds(2.5f);
		InitTileGrid (0);
		hud.gameObject.SetActive (true);
	}

	private IEnumerator TalkPreSceneOne()
	{
		yield return new WaitForSeconds(0.5f);
		textManager.Talk (config.text.TextPreSceneOne, false,2.5f);
		yield return new WaitForSeconds(2.5f);
	}

	private IEnumerator TalkSceneOne()
	{
		yield return new WaitForSeconds(0.5f);

		textManager.Talk (config.text.TextSceneOneFirst, true,2.5f);
		yield return new WaitForSeconds(2.5f);
		textManager.Talk (config.text.TextSceneOneSecond, false,3.5f);
		yield return new WaitForSeconds(3.5f);
		config.UserData.NextStep ();


		((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1).ObjState = ObjectState.Run;
//		textManager.Talk ("Hahaha", false);
		yield return new WaitForSeconds(2.5f);
	}

	public void CloneItemTutorial(UserData mData)
	{
		
		ItemBaseData item;
		for(int i = 0; i < config.matchTutCfg.itemtutorial.Count; i++)
		{
			int id = config.matchTutCfg.itemtutorial.ElementAt(i).Value.Id;
			ItemCfgImpl iConfig = config.ItemCfg.GetItemByItemId(id);
			item = new ItemBaseData(iConfig);
			item.LevelUpgrade = 10;
			mData.Inventory.AddItem(item);
			mData.EquipItem (item);
		}
	}

	public void InitTileGrid(int index) {
		Cell[,] cells = ((MatchLogic)matchLogic).cells;
		for (var x = 0; x < Data.tileHeight; x++) {
			MatchTutorialData matchData = config.matchTutCfg.getMatchTutorial(x+1+index);
			Data.TileTypes d = Data.TileTypes.Attack;
			gemType.TryGetValue(matchData.ColumnA, out d);
			cells[0,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnB, out d);
			cells[1,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnC, out d);
			cells[2,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnD, out d);
			cells[3,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnE, out d);
			cells[4,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnF, out d);
			cells[5,x] = InitCell(d);
			gemType.TryGetValue(matchData.ColumnG, out d);
			cells[6,x] = InitCell(d);
		}			
		bool fall = false;
		if(index == 0) fall = true;
		DisplayTileGrid (cells, fall);
	}

	private void DisplayTileGrid(Cell[,] cells, bool fall) {
		((MatchLogic)matchLogic).tiles = new List<MatchItem>();
		EmptyGemList ();
		gemList = new List<GameObject> ();
		int i = 0;
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				i++;
				int type = (int)cells[x, y].cellType;
				string spriteName = ((MatchLogic)matchLogic).sprites[(type - 1)];
				GameObject instance = gemPool.GetInstance();
				instance.SetActive(true);
				gemList.Add(instance);
				instance.transform.parent = ((MatchLogic)matchLogic).grid.transform;
				instance.GetComponent<UISprite>().spriteName = spriteName;
				instance.transform.localScale = MatchLogic.cellScale;
				if(fall)
				{
					instance.transform.localPosition = new Vector3(x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2, (Data.tileHeight - 1 - y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2 + MatchLogic.cellHeight * Data.tileHeight, 0f);
				} else {
					instance.transform.localPosition = new Vector3(x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2, (Data.tileHeight - 1 - y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2, 0f);
				}
				MatchItem tile = instance.GetComponent<MatchItem>();
				tile.cell = cells[x, y];
				tile.point = new TilePoint(x, y);
				((MatchLogic)matchLogic).tiles.Add(tile);
			}
		}

		MoveGemDown ();
	}


	public void EmptyBlackList()
	{
		if(CheckPause(config.UserData))
		{
			for(int i = 0; i < blackList.Count; i++)
			{
				blackPool.ReturnInstance(blackList[i]);
				blackList[i].SetActive(false);
			}
			blackList.Clear ();
		}
		
	}

	private void EmptyGemList()
	{
		for(int i = 0; i < gemList.Count; i++)
		{
			gemPool.ReturnInstance(gemList[i]);
			gemList[i].SetActive(false);
		}
		gemList.Clear ();
	}

	public void DisplayBlackTitleGrid(GridTutorialData gridData)
	{
		for (var x = 0; x < Data.tileWidth; x++) {
			for (var y = 0; y < Data.tileHeight; y++) {
				bool check = false;
				for(int k = 0; k < gridData.CellPosition.x.Count; k++)
				{
					if(x == gridData.CellPosition.x[k] && y == gridData.CellPosition.y[k])
					{
						check = true;
						break;
					}
				}
				if(!check)
				{
					CreateBlackItem(0.7f,x,y);
				}
			}
		}
		TilePoint tilePoint = new TilePoint (gridData.SelectPosition.x [0], gridData.SelectPosition.y [0]);
		MatchItem tile = ((MatchLogic)matchLogic).FindTile (tilePoint);
		((MatchLogic)matchLogic).SetCurTile (tile);
	}

	private void CreateBlackItem(float alpha, int x, int y)
	{
		GameObject instance = blackPool.GetInstance();
		instance.SetActive(true);
		blackList.Add(instance);
		instance.transform.parent = ((MatchLogic)matchLogic).grid.transform;
		UISprite image = instance.GetComponent<UISprite>();
		image.spriteName = "bga";
		image.width = 95;
		image.height = 95;
		image.fillAmount = 1;
		image.depth = 2;
		image.alpha = alpha;
		instance.transform.localScale = MatchLogic.cellScale;
		instance.transform.localPosition = new Vector3(x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2, (Data.tileHeight - 1 - y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2, 0f);
	}

	public void CreateBlackObject(Transform parent)
	{
		blackObject = null;
		blackObject = blackBgPool.GetInstance();
		blackObject.SetActive(true);
		blackObject.transform.parent = parent;
		UISprite image = blackObject.GetComponent<UISprite>();
		image.fillAmount = 1;
		blackObject.transform.localPosition = Vector3.zero;
		image.width = 750;
		image.height = 1050;

		TweenAlpha tweenAlpha;
		tweenAlpha = UITweener.Begin<TweenAlpha>(blackObject, 0.5f);
		tweenAlpha.from = 0f;
		tweenAlpha.to = 0.7f;
	}

	private void CreateComponentBlack(float x, float y, float w, float h, Transform parent)
	{
		GameObject blackObject = blackBgPool.GetInstance();
		blackList.Add(blackObject);
		blackObject.SetActive(true);
		blackObject.transform.parent = parent;
		UISprite image = blackObject.GetComponent<UISprite>();
		blackObject.transform.localScale = Vector3.one;
		image.fillAmount = 1;
		blackObject.transform.localPosition = new Vector3(x,y,0);
		image.width = (int)w;
		image.height = (int)h;
		image.alpha = 0.7f;
	}

	public void CreateBlackObject(Transform parent, HomeTownTutorialData tutoData)
	{
		if(tutoData.EventName == 0)
		{
			blackList = new List<GameObject> ();
			CreateComponentBlack(-55,-45,900,1200,parent);
		} else {
			if(tutoData.Target != "empty")
			{
				UISprite img = GameObject.Find(tutoData.Target).GetComponent<UISprite>();
				Vector3 pos = Utils.GetPosition2D(img.transform);
				int sw = 640;
				int sh = 960;
				SetNameContent sContent = parent.parent.GetComponent<SetNameContent>();
				if(sContent != null)
				{
					sContent.arrow.gameObject.SetActive(true);
					sContent.arrow.transform.localPosition = new Vector3(pos.x, pos.y+img.height/2+50, pos.z);
				}
				GameObject go;
				for(int i = 0; i < tutoData.ImgName.Count; i++)
				{
					go = GameObject.Find(tutoData.ImgName[i]);
					ImgDataShow ids = new ImgDataShow(go.transform, go.transform.parent);
					listParent.Add(ids);
					Vector3 posGo = Utils.GetPosition2D(go.transform);
					go.transform.parent = parent;
					go.transform.localPosition = posGo;
					go.SetActive(false);
					go.SetActive(true);
				}
			}
		}
	}

	public void ReturnParent()
	{
		if(listParent.Count > 0)
		{
			GameObject gObject = null;
			gObject = listParent [0].child.gameObject;
			for(int i = 0; i < listParent.Count; i++)
			{
				listParent[i].child.parent = listParent[i].parent;
				listParent[i].child.gameObject.SetActive(false);
				listParent[i].child.gameObject.SetActive(true);
			}
			listParent.Clear();
		}
	}

	public void ReturnBlackObject()
	{
		blackBgPool.ReturnInstance(blackObject);
		if(blackObject != null)
			blackObject.SetActive(false);
	}
	
	private void MoveGemDown()
	{
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
				MatchItem tile = ((MatchLogic)matchLogic).FindTile(point);
				Vector3 pos = new Vector3(tile.point.x * MatchLogic.cellWidth + MatchLogic.cellWidth / 2, (Data.tileHeight - 1 - tile.point.y) * MatchLogic.cellHeight + MatchLogic.cellHeight / 2);
				TweenParms parms = new TweenParms().Prop("localPosition", pos).Ease(EaseType.EaseInQuad).Delay(m * waitTime).OnComplete (new TweenDelegate.TweenCallback(PlaySoundGemDown));
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
				HOTween.To(tile.transform, fallTime, parms);
				if (m * waitTime + fallTime > delay) delay = m * waitTime + fallTime;
				m++;
			}
		}
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

	private Cell InitCell(Data.TileTypes data)
	{
		Cell cell = new Cell ();
		cell.cellType = data;
		return cell;
	}

	public void AddManaTut()
	{
		
		BattleEntity team1 = ((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1);
		HeroStat hStat = team1.getStat ();
		team1.AddMana (hStat.baseStat.manaPool - (int)hStat.CurrentMana - 10);
	}

	public void AddTurn()
	{
		if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.TUT_MATCH_5)
		{
			countTurn++;
		}
	}

	public bool CheckTutorialProgress()
	{
		return config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.START;
	}
}
