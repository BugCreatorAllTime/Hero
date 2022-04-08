using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DungeonState
{
	public enum State
	{
		Win, Lose, Playing, Loading
	}
	public PlayMode playMode { get; set; }
	public State state { get; set; }
	public IntegerWrapper remainingMoves { get; set; }
	public float startTime { get; set; }

	public List<Reward> itemList = new List<Reward>();
	public IntegerWrapper chestID = -1;
	private IntegerWrapper indexStop = 0;
	private IntegerWrapper collectedGold;
	private List<KeyValuePair<int, int>> defeatedMonsters;
	private IntegerWrapper reviveNumber;
	private IntegerWrapper exp = 0;

	[Inject]
	public ConfigManager configManager { get; set; }

	[PostConstruct]
	public void Init()
	{
		collectedGold = 0;
		defeatedMonsters = new List<KeyValuePair<int, int>>();
		reviveNumber = 0;
		playMode = PlayMode.Monster;
		state = State.Loading;
		startTime = Time.time;
		remainingMoves = 0;
	}

	public void Start()
	{
		state = State.Playing;
	}

	public int ReviveNumber
	{
		get { return reviveNumber.GetValue(); }
		set { this.reviveNumber = value; }
	}

	public void CollectGold(int gold)
	{
		collectedGold += gold;
		/*Logger.Trace("collected gold ", Gold());*/
	}

	public void DefeatMonster(int monsterId, int monsterLevel)
	{
		defeatedMonsters.Add(new KeyValuePair<int, int>(monsterId, monsterLevel));
		/*StringBuilder s = new StringBuilder();
		for (int i = 0; i < defeatedMonsters.Count; i++)
		{
			s.Append(defeatedMonsters[i]);
		}
		Logger.Trace("defeated monster ", monsterId, " total ", s.ToString());*/
	}

	public int Gold()
	{
		return collectedGold.GetValue();;
	}

	public List<KeyValuePair<int, int>> GetDefeatedMonsters()
	{
		return defeatedMonsters;
	}

	public void SetDefeatedMonsters(List<KeyValuePair<int, int>> monsters)
	{
		this.defeatedMonsters = monsters;
	}

	public void Revive()
	{
		reviveNumber++;
	}

	public void NextItemGet()
	{
		this.indexStop++;
	}

	public int IndexStop()
	{
		return indexStop.GetValue();
	}

	public void IndexStop(int index)
	{
		this.indexStop = index;
	}

	public void GainExp(int exp)
	{
		this.exp += exp;
	}

	public int GetExp()
	{
		return this.exp.GetValue();
	}
}