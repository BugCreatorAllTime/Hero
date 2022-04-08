using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Collections;
using strange.examples.strangerocks;

public class GenMapView : View
{
	[Inject]
	public AssetMgr mgr{get; set;}

	[Inject]
	public CrossContextData crossContextData { get; set; }

	[Inject]
	public DungeonService dungeonService { get; set;}

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public NoteDungeonManager noteManager {get;set;}

    private const string fileName = "Config/WorldMapCfg.json";
	private const string pieceName = "Prefabs/WorldMap/mapPiece";
	private string iconChestName = "Prefabs/General/UISprite";
	private const string mapPath = "Textures/WorldMap/";
	private const string itemAtlasName = "Atlas/ItemAtlas";
	private const string mapName = "map";
    private GameObject mapPiecePrefab;
	private GameObject iconChestPrefab;
	private GameObject itemAtlasPrefab;
	private GameObject dungeonsParent;
	private Material[] materials = new Material[2];

	private int index;
	private int pos;
	private UIScrollView scrollView;

	internal void Init()
	{
		dungeonsParent = GameObject.Find ("Dungeons");
		materials[0] = new Material(Shader.Find("Unlit/Transparent Colored"));
		materials[1] = new Material(Shader.Find("Unlit/Transparent Colored"));
		noteManager.dungeonsParent = dungeonsParent;
		pos = (int)((-crossContextData.lastMapPosition.y + 480) / 960);
		OnLoadPrefabSuccess ();
	}

	private void LoadMaterialToMap(UITexture sprite)
	{
		if(index <= 1)
		{
			GameObject bg = gameObject.transform.GetChild(index).gameObject;
			bg.AddComponent<MapPieceContent>();
			MapPieceContent mContent = bg.GetComponent<MapPieceContent>();
			if(pos > 0)
			{
				bg.transform.localPosition = new Vector3(0,(pos+index)*960,0);
			}
			mContent.noteManager = noteManager;
			mContent.indexMap = index + pos;
			mContent.mgr = mgr;
			sprite = bg.GetComponent<UITexture>();
			materials[index].mainTexture = sprite.mainTexture;
			sprite.material = materials[index];
			sprite.mainTexture = null;
			mContent.newMaterial = materials[index];
			if(pos > 0)
			{
				mgr.GetAsset<Texture2D>(mapPath + mapName + (index+pos), delegate (Texture2D mTexture){
					sprite.material.mainTexture = mTexture;
					noteManager.LoadNoteDungeon(index+pos);
					index++;
					LoadMaterialToMap(sprite);
				});
			} else {
				noteManager.LoadNoteDungeon(index);
				index++;
				LoadMaterialToMap(sprite);
			}
		} else {
			scrollView = gameObject.transform.parent.gameObject.GetComponent<UIScrollView>();
			scrollView.onDragFinished += new UIScrollView.OnDragFinished(OnDragFinished);
			RestoreLastMapPosition();
			gameObject.GetComponent<UpdateDataScrollView>().enabled = true;
			routineRunner.StartCoroutine(GameObject.Find("Tab").GetComponent<LoadInfoTab>().ProgressHideLoading(true));
		}
	}

	private void OnLoadPrefabSuccess()
	{
		noteManager.LoadFromJson(fileName, o =>
		{
			index = 0;
			noteManager.dungeons = o;
			UITexture sprite = null;
			LoadMaterialToMap(sprite);

		});
	}

	private void RestoreLastMapPosition()
	{
		scrollView.transform.localPosition = crossContextData.lastMapPosition;
		UIPanel panel = scrollView.transform.GetComponent<UIPanel>();
		Vector2 co = panel.clipOffset;
		co.x -= scrollView.transform.localPosition.x;
		co.y -= scrollView.transform.localPosition.y;
		panel.clipOffset = co;
	}

	private void OnDragFinished()
	{
		crossContextData.lastMapPosition = scrollView.transform.localPosition;
	}
}