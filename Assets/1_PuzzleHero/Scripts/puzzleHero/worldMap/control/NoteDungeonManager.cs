using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using System.Collections.Generic;
using System;
using LitJson;
using strange.extensions.pool.api;
using System.Linq;

public class NoteDungeonManager {

	[Inject(PrefabWorldMap.dungeon)]
	public IPool<GameObject> dungeonPool { get; set; }

	[Inject]
	public AssetMgr mgr{get; set;}

	[Inject]
	public DungeonService dungeonService { get; set;}

	[Inject]
	public ConfigManager config { get; set;}

	private int numberOfMapPiece;
	private int numberOfDungeon;
	private Dictionary<int, InfoUserFBData> dungeonUsers;

	public List<Dungeon> dungeons;
	public List<ClickDungeonView> dungeonPrefabs;
	public GameObject dungeonsParent;
	public GameObject iconChestPrefab;
	public GameObject itemAtlasPrefab;

	[PostConstruct]
	public void Init()
	{
		dungeonPrefabs = new List<ClickDungeonView> ();
		dungeonUsers = new Dictionary<int, InfoUserFBData> ();
	}
	
	public void LoadNoteDungeon(int indexPiece)
	{
		int id;
		int energy ;
		int levelReq = 0;
		bool isLock;
		Vector3 pos = Vector3.zero;
		for(int j = 0; j < dungeons.Count; j++){
			Dungeon dungeon = dungeons[j];
			if(dungeon.PosY >= indexPiece * 960 - 480 && dungeon.PosY < (indexPiece+1) * 960 - 480)
			{
				GameObject button = CreatePoolGameObject (dungeonPool);
				button.GetComponent<UIButton>().state = UIButtonColor.State.Normal;
				button.transform.parent = dungeonsParent.transform;
				button.transform.localScale = Vector3.one;
				ClickDungeonView dContent = button.GetComponent<ClickDungeonView>();
				dungeonPrefabs.Add(dContent);
				id = dungeon.Level;
				energy = dungeonService.GetEnergy(dungeon.Level);
				isLock = config.UserData.curMapId < id;
				if(isLock) button.GetComponent<UIButton>().state = UIButtonColor.State.Disabled;
				dContent.SetData(id,dungeon.Piece,energy,levelReq,isLock);
				pos.x = dungeon.PosX;
				pos.y = dungeon.PosY;
				button.transform.localPosition = pos;
				dContent.iconChest.gameObject.SetActive(false);
				dContent.ava.gameObject.SetActive(false);
				DungeonConfImpl dungeonData = config.DungeonCfg.getDungeon(dungeon.Level);
				for(int k = 0; k < dungeonData.IdMonster.Count; k++)
				{
					if(dungeonData.IdMonster[k] > 1000)
					{
						dContent.iconChest.gameObject.SetActive(true);
						dContent.iconChest.spriteName = config.chestCfg.GetChestCfg(dungeonData.IdMonster[k]).Icon;
						break;
					}
				}
				if(dungeonUsers.ContainsKey(id))
				{
					dContent.ava.gameObject.SetActive(true);
					dContent.ava.atlas = dungeonUsers[id].atlas;
					dContent.ava.spriteName = dungeonUsers[id].iconName;
					ClickAvatarView cContent = dContent.ava.gameObject.GetComponent<ClickAvatarView>();
					cContent.SetData(dungeonUsers[id].name, dungeonUsers[id].gender, dungeonUsers[id].equipments, dungeonUsers[id].iconName);
				}
			}
		}
	}

	public void UnLoadNoteDungeon(int indexPiece)
	{
		for(int i = dungeonPrefabs.Count - 1; i >= 0; i--)
		{
			int posY = (int)dungeonPrefabs[i].transform.localPosition.y;
			if(posY >= indexPiece * 960 - 480 && posY < (indexPiece+1) * 960 - 480)
			{
				ReturnInstance(dungeonPrefabs[i].gameObject, dungeonPool);
				dungeonPrefabs.RemoveAt(i);
			}
		}
	}

	public List<Dungeon> LoadFromJson(string file, Action<List<Dungeon>> onLoadList)
	{
		List<Dungeon> dungeons = new List<Dungeon>();
		mgr.GetAsset<TextAsset>(file, delegate (TextAsset ta){
			string jsonText = ta.text;
			JsonData data = JsonMapper.ToObject(jsonText);
			numberOfMapPiece = (int)data["numberOfPiece"];
			numberOfDungeon = (int)data["dungeon"].Count;
			
			for (int i = 0; i < numberOfDungeon; i++)
			{
				int piece = (int)data["dungeon"][i]["piece"];
				int level = (int)data["dungeon"][i]["level"];
				int posX = (int)data["dungeon"][i]["x"];
				int posY = (int)data["dungeon"][i]["y"];
				Dungeon dun = new Dungeon(piece, level, posX, posY);
				dungeons.Add(dun);
			}
			onLoadList(dungeons);
		});
		return null;
	}

	private GameObject CreatePoolGameObject(IPool<GameObject> poolObject)
	{
		GameObject contentObject = poolObject.GetInstance();
		contentObject.SetActive(true);
		return contentObject;
	}

	private void ReturnInstance(GameObject contentObject, IPool<GameObject> contentPool)
	{
		if(contentObject != null)
		{
			contentObject.SetActive (false);
			contentPool.ReturnInstance(contentObject);
		}
	}

	public void SetListUserFaceBook(List<InfoUserFBData> listUser)
	{
		for(int i = 0; i < listUser.Count; i++)
		{
			if(!dungeonUsers.ContainsKey(listUser[i].currentLevel))
			{
				dungeonUsers.Add(listUser[i].currentLevel, listUser[i]);
			}
		}
		for(int i = 0; i < dungeonUsers.Count; i++)
		{
			int id = int.Parse(dungeonPrefabs[i].lb_id.text);
			if(dungeonUsers.ContainsKey(id))
			{
				dungeonPrefabs[i]. ava.gameObject.SetActive(true);
				dungeonPrefabs[i].ava.atlas = dungeonUsers[id].atlas;
				dungeonPrefabs[i].ava.spriteName = dungeonUsers[id].iconName;
				ClickAvatarView cContent = dungeonPrefabs[i].ava.gameObject.GetComponent<ClickAvatarView>();
				cContent.SetData(dungeonUsers[id].name, dungeonUsers[id].gender, dungeonUsers[id].equipments, dungeonUsers[id].iconName);
			}
		}
	}

	public void HideUserFaceBook()
	{
		for(int i = 0; i < dungeonUsers.Count; i++)
		{
			int id = int.Parse(dungeonPrefabs[i].lb_id.text);
			if(dungeonUsers.ContainsKey(id))
			{
				dungeonPrefabs[i]. ava.gameObject.SetActive(false);
			}
		}
		dungeonUsers.Clear ();
	}
}
