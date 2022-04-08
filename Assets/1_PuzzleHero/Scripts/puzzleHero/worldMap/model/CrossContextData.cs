using UnityEngine;
using System.Collections;

public class CrossContextData {

	public int dungeonId {get; set;}
	public int dungeonChoose;
	public string nameDungeon;
	public int monsterIndexInDungeon{get;set;}
	public Vector3 lastMapPosition;

	public void OnNewDungeonContextConstruction()
	{
		Reset();
	}

	private void Reset()
	{
		monsterIndexInDungeon = 0;
	}
}
