using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class DungeonStateData
{
	public CharacterState characterState { get; set; }
	public MonsterState monsterState { get; set; }
	public BoardState boardState { get; set; }
	public BattleState battleState { get; set; }


	public DungeonStateData()
	{
		characterState = new CharacterState();
		monsterState = new MonsterState();
		boardState = new BoardState();
		battleState = new BattleState();
	}

	public override string ToString()
	{
		return characterState.ToString() + "\n" + monsterState.ToString() + "\n" + boardState.ToString() + "\n" +
		       battleState.ToString();
	}
}

public class SimplifiedStat
{
	public int curHp { get; set; }
	public int curArmor { get; set; }
	public int currMana { get; set; }

	public SimplifiedStat()
	{
	}

	public SimplifiedStat(HeroStat stat)
	{
		if (stat == null) return;
		this.curHp = stat.CurHp;
		this.curArmor = stat.CurArmor;
		this.currMana = (int) stat.CurrentMana;
	}

	public override string ToString()
	{
		return "hp: " + curHp + " mana " + currMana + " armor " + curArmor;
	}
}

public class SimplifiedMonsterSkill
{
	public int numberOfTurnsToActivate { get; set; }
	public List<TilePoint> points { get; set; }
	public string type { get; set; }
	public BoardSkillInfo info { get; set; }

	public SimplifiedMonsterSkill()
	{
	}

	public SimplifiedMonsterSkill(MonsterSkill skill)
	{
		this.numberOfTurnsToActivate = skill.numberOfTurnsToActivate;
		this.points = new List<TilePoint>();
		List<MatchItem> affectedGems = skill.GetAffectedGems();
		for (int i = 0; i < affectedGems.Count; i++)
		{
			MatchItem item = affectedGems[i];
			points.Add(item.point);
		}
		this.type = skill.GetType().ToString();
		this.info = skill.GetSkillInfo();
	}

	public override string ToString()
	{
		string s = "";
		s += "type" + type;
		s += "\nnumberOfTurn " + this.numberOfTurnsToActivate;
		for (int i = 0; i < points.Count; i++) {
			s += "\n" + points[i].x + ":" + points[i].y;
		}
		s += "\ninfo";
		FieldInfo[] f = this.info.GetType().GetFields(BindingFlags.Public |
														 BindingFlags.NonPublic |
														 BindingFlags.Instance);
		for (int i = 0; i < f.Length; i++) {
			s += "\n" + f[i].ToString() + " " + f[i].GetValue(this.info);
		}
		return s;
	}
}

public class CharacterState
{
	public SimplifiedStat stat { get; set; }

	public void ToCharacterEntity(BattleEntity character)
	{
		HeroStat curCharStat = character.getStat();
		curCharStat.CurArmor = stat.curArmor;
		curCharStat.CurHp = stat.curHp;
		curCharStat.AddMana(stat.currMana);
	}

	public override string ToString()
	{
		return this.GetType() + "\n" + stat.ToString();
	}
}

public class MonsterState
{
	public SimplifiedStat stat { get; set; }
	public List<SimplifiedMonsterSkill> skills { get; set; }
	public int curTurn { get; set; }

	public void ToMonsterEntity(BattleEntity monster)
	{
		monster.getStat().CurHp = stat.curHp;
		monster.CurTurn = curTurn;
	}

	public static List<SimplifiedMonsterSkill> ConvertMonsterSkills(List<MonsterSkill> skills)
	{
		List<SimplifiedMonsterSkill> returnSkills = new List<SimplifiedMonsterSkill>();
		for (int i = 0; i < skills.Count; i++)
		{
			MonsterSkill monsterSkill = skills[i];
			SimplifiedMonsterSkill s = new SimplifiedMonsterSkill(monsterSkill);
			returnSkills.Add(s);
		}
		return returnSkills;
	}

	public List<MonsterSkill> ToMonsterSkills(BattleGameLogic battleGameLogic, MatchLogic matchLogic, EffectsManager effectsManager, SkillsManager skillsManager)
	{
		List<MonsterSkill> mSkills = new List<MonsterSkill>();
		for (int i = 0; i < skills.Count; i++)
		{
			SimplifiedMonsterSkill simplifiedSkill = skills[i];
			Type type = Type.GetType(simplifiedSkill.type);
			MonsterSkill skill1 = (MonsterSkill)Activator.CreateInstance(type);
			skill1.SkillInfo = simplifiedSkill.info;
			skill1.SkillInfo.skillType = type.ToString();
			skill1.battleGameLogic = battleGameLogic;
			skill1.matchLogic = matchLogic;
			skill1.effectsManager = effectsManager;
			skill1.skillsManager = skillsManager;
			skill1.rate = simplifiedSkill.info.skillRate;
			skill1.numberOfTurnsToActivate = simplifiedSkill.numberOfTurnsToActivate;

			SetupAffectedGems(skill1, simplifiedSkill, matchLogic, effectsManager);

			mSkills.Add(skill1);
			skillsManager.AddSkill(skill1);
			BattleEntity owner = battleGameLogic.FindEntity(BattleGameLogic.TEAM2);
			skill1.SetOwner(owner);
		}
		return mSkills;
	}

	private void SetupAffectedGems(MonsterSkill monsterSkill, SimplifiedMonsterSkill simplifiedSkill, MatchLogic matchLogic, EffectsManager effectsManager)
	{
		List<MatchItem> affectedGems = new List<MatchItem>();
		Dictionary<MatchItem, GameObject> fxObjects = new Dictionary<MatchItem, GameObject>();
		List<TilePoint> points = simplifiedSkill.points;
		for (int k = 0; k < points.Count; k++)
		{
			TilePoint p = points[k];
			MatchItem item = matchLogic.FindTile(p);
			item.SetAffected(true);
			bool isLocked = simplifiedSkill.type.Contains("Lock");
			item.SetLocked(isLocked);
			affectedGems.Add(item);
		}
		monsterSkill.SetAffectedGems(affectedGems);
		monsterSkill.PlayFxOnGem();
	}

	public override string ToString()
	{
		return this.GetType() + "\n" + stat.ToString();
	}
}

public class BoardState
{
	private const string separator = ":";
	public Dictionary<string, Data.TileTypes> gems;

	public BoardState()
	{
	}

	public BoardState(MatchLogic matchLogic)
	{
		gems = new Dictionary<string, Data.TileTypes>();
		for (int i = 0; i < matchLogic.tiles.Count; i++)
		{
			MatchItem item = matchLogic.tiles[i];
			TilePoint p = item.point;
			Data.TileTypes value = item.cell.cellType;
			string key = p.x + separator + p.y;
			gems[key] = value;
		}
	}

	public void ToBoard(MatchLogic matchLogic)
	{
		Dictionary<TilePoint, Data.TileTypes> predefined = new Dictionary<TilePoint, Data.TileTypes>();
		for (int i = 0; i < gems.Count; i++)
		{
			KeyValuePair<string, Data.TileTypes> pair = gems.ElementAt(i);
			string[] s = pair.Key.Split(char.Parse(separator));
			int x = int.Parse(s[0]);
			int y = int.Parse(s[1]);
			TilePoint key = new TilePoint(x, y);
			predefined[key] = pair.Value;
		}

		matchLogic.Predefine(predefined);
	}

	public override string ToString()
	{
		string s = "";
		for (int i = 0; i < gems.Count; i++)
		{
			s += "\n" + gems.ElementAt(i).Key + " - " + gems.ElementAt(i).Value;
		}
		return s;
	}
}

public class BattleState
{
	public PlayMode playMode { get; set; }
	public List<Reward> itemList { get; set; }
	public DungeonState.State state { get; set; }
	public List<KeyValuePair<int, int>> defeatedMonsters { get; set; }

	public int exp { get; set; }
	public int chestID { get; set; }
	public int startTime { get; set; }
	public int indexStop { get; set; }
	public int reviveNumber { get; set; }
	public int collectedGold { get; set; }
	public int remainingMoves { get; set; }

	public int dungeonId { get; set; }
	public int monsterIndexInDungeon { get; set; }

	public BattleState()
	{
	}

	public BattleState(DungeonState dunState, CrossContextData crossContextData)
	{
		FetchData(dunState);
		FetchData(crossContextData);
	}

	public void ToDungeonState(DungeonState dunState)
	{
		dunState.state = this.state;
		dunState.playMode = this.playMode;
		dunState.itemList = this.itemList;
		dunState.startTime = this.startTime;
		dunState.ReviveNumber = this.reviveNumber;
		dunState.remainingMoves = this.remainingMoves;
		dunState.IndexStop(this.indexStop);
		dunState.CollectGold(this.collectedGold);
		dunState.SetDefeatedMonsters(this.defeatedMonsters);

		//Logger.Trace("restore remaining moves", dunState.remainingMoves);
		for (int i = 0; i < defeatedMonsters.Count; i++)
		{
			//Logger.Trace(defeatedMonsters[i].Key + " " + defeatedMonsters[i].Value);
		}
	}

	public void ToCrossContextData(CrossContextData data)
	{
		data.dungeonId = dungeonId;
		data.monsterIndexInDungeon = monsterIndexInDungeon;
	}

	private void FetchData(DungeonState dunState)
	{
//		FieldInfo[] fieldInfos = dunState.GetType().GetFields(BindingFlags.Public |
//		                                                      BindingFlags.NonPublic |
//		                                                      BindingFlags.Instance);
//		FieldInfo[] bStateFieldInfos = this.GetType().GetFields(BindingFlags.Public |
//		                                                        BindingFlags.NonPublic |
//		                                                        BindingFlags.Instance);
//		for (int i = 0; i < fieldInfos.Length; i++)
//		{
//			FieldInfo f = fieldInfos[i];
//			Logger.Trace("field", f.ToString());
//			for (int k = 0; k < bStateFieldInfos.Length; k++)
//			{
//				FieldInfo f2 = bStateFieldInfos[k];
//				if (f.ToString().Equals(f2.ToString()))
//				{
//					Logger.Trace("field", f, "value", f.GetValue(dunState));
//					f2.SetValue(this, f.GetValue(dunState));
//				}
//			}
//		}

		this.state = dunState.state;
		this.playMode = dunState.playMode;
		this.itemList = dunState.itemList;
		this.indexStop = dunState.IndexStop();
		this.startTime = (int) dunState.startTime;
		this.reviveNumber = dunState.ReviveNumber;
		this.collectedGold = dunState.Gold();
		this.remainingMoves = dunState.remainingMoves.GetValue();
		//Logger.Trace("save remaining moves", dunState.remainingMoves);
		this.defeatedMonsters = dunState.GetDefeatedMonsters();
	}

	private void FetchData(CrossContextData crossContextData)
	{
		dungeonId = crossContextData.dungeonId;
		monsterIndexInDungeon = crossContextData.monsterIndexInDungeon;
	}

	public override string ToString()
	{
		FieldInfo[] f = this.GetType().GetFields(BindingFlags.Public |
		                                         BindingFlags.NonPublic |
		                                         BindingFlags.Instance);
		string s = "";
		for (int i = 0; i < f.Length; i++)
		{
			s += "\n" + f[i].ToString() + " " + f[i].GetValue(this);
		}
		return s;
	}
}