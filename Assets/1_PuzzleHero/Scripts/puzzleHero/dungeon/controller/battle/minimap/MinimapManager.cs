using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using strange.extensions.mediation.impl;
using UnityEngine;
using System.Collections;
using strange.examples.strangerocks;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Holoville.HOTween.Plugins;

public class MiniMapManager {
	[Inject]
	public IRoutineRunner routineRunner { get; set; }
	[Inject]
	public AssetMgr assetMgr { get; set; }
	[Inject]
	public CrossContextData crossContextData { get; set; }
	[Inject(DungeonContext.BOARD)]
	public GameObject board { get; set; }
	[Inject]
	public ConfigManager cfg { get; set; }
	[Inject]
	public SoundManager soundMng { get; set; }

	private GameObject miniMap;
	private SkeletonAnimation skeletonAnimation;
	private const string fileName = "Config/MiniMapCfg.json";
	private List<Vector3> listMovePosition = new List<Vector3>();
	List<Vector3> listPosition = new List<Vector3>();
	Vector3 endMarker;
	private int numberOfPoints = 15;
	private Vector3[] controlPoints = new Vector3[3];
	private int count = 0;
	private int countTarget = 0;
	private GameObject character;
	private List<GameObject> listChild = new List<GameObject>();
	const float TIME_OPEN_MAP = 0.5f;
	const float TIME_CLOSE_MAP = 0.5f;
	public int posMonster = 0;
	private bool save;

	public void CreatMiniMap() {
		if (cfg.UserData.currentStepTownTutorial >= 1) {
			miniMap = GameObject.Instantiate(assetMgr.GetAssetSync<GameObject>("Prefabs/General/UISprite")) as GameObject;
			for (int i = 0; i < cfg.miniMapCfg.minimap.Count; i++) {
				if (cfg.miniMapCfg.getMiniMap(i + 1).ID == cfg.DungeonCfg.getDungeon(crossContextData.dungeonId).MiniMapId) {
					int countMonster = 0;
					for (int j = 0; j < cfg.miniMapCfg.getMiniMap(i + 1).NodePosition.x.Count; j++) {
						listMovePosition.Add(new Vector3(cfg.miniMapCfg.getMiniMap(i + 1).NodePosition.x[j],
														 cfg.miniMapCfg.getMiniMap(i + 1).NodePosition.y[j], 0));
						if (j == 0) {
							CreatObject("start", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 4);
						}
						if (j % 2 == 0 && j != 0) {
							int idMonster = cfg.DungeonCfg.getDungeon(crossContextData.dungeonId).IdMonster[countMonster];
							switch (cfg.MonsterCfg.GetMonsterCfgData(idMonster).Type) {
								case MonsterCfg.TYPE_MOB:
									countMonster++;
									CreatObject("combat", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 3);
									break;
								case MonsterCfg.TYPE_MINI_BOSS:
									countMonster++;
									CreatObject("combat2", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 3);
									break;
								case MonsterCfg.TYPE_BOSS:
									countMonster++;
									CreatObject("bossb", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 3);
									break;
								case MonsterCfg.TYPE_CHEST:
									countMonster++;
									CreatObject("chest", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 3);
									break;
								default:
									countMonster++;
									CreatObject("combat", j, cfg.miniMapCfg.getMiniMap(i + 1).NodePosition, 3);
									break;
							}
						}
					}
					for (int j = 0; j < cfg.miniMapCfg.getMiniMap(i + 1).RockPosition.x.Count; j++) {
						if (cfg.miniMapCfg.getMiniMap(i + 1).RockPosition.x[j] != -9999)
							CreatObject("rock", j, cfg.miniMapCfg.getMiniMap(i + 1).RockPosition, 3);

					}
					for (int j = 0; j < cfg.miniMapCfg.getMiniMap(i + 1).TreePosition.x.Count; j++) {
						if (cfg.miniMapCfg.getMiniMap(i + 1).TreePosition.x[j] != -9999)
							CreatObject("tree", j, cfg.miniMapCfg.getMiniMap(i + 1).TreePosition, 3);
					}
					for (int j = 0; j < cfg.miniMapCfg.getMiniMap(i + 1).SwampPosition.x.Count; j++) {
						if (cfg.miniMapCfg.getMiniMap(i + 1).SwampPosition.x[j] != -9999)
							CreatObject("swamp", j, cfg.miniMapCfg.getMiniMap(i + 1).SwampPosition, 3);
					}
					for (int j = 0; j < cfg.miniMapCfg.getMiniMap(i + 1).NodePosition.x.Count; j++) {
						SetWay(true);
						listPosition = new List<Vector3>();
					}
					if (cfg.UserData.dungeonStateData != null && cfg.UserData.restoreDungeonState)
						count = 0 + cfg.UserData.dungeonStateData.battleState.monsterIndexInDungeon * 2;
					else count = 0;
					break;
				}
			}
			UISprite imgMiniMap = miniMap.GetComponent<UISprite>();
			miniMap.transform.parent = board.transform.parent;
			imgMiniMap.type = UIBasicSprite.Type.Sliced;
			imgMiniMap.spriteName = "1";
			imgMiniMap.depth = 2;
			imgMiniMap.width = 169;
			imgMiniMap.height = 500;
			miniMap.transform.localPosition = new Vector3(0, -200, 0);
			miniMap.name = "MiniMap";
			miniMap.transform.localScale = Vector3.one;
			character = GameObject.Instantiate(assetMgr.GetAssetSync<GameObject>("Prefabs/General/UISprite")) as GameObject;
			character.transform.parent = miniMap.transform;
			character.GetComponent<UISprite>().spriteName = "key";
			character.GetComponent<UISprite>().MakePixelPerfect();
			character.transform.localPosition = new Vector3(listMovePosition[0].x, listMovePosition[0].y, 0);
			character.name = "Character";
			character.GetComponent<UISprite>().pivot = UIWidget.Pivot.BottomLeft;
			character.GetComponent<UISprite>().depth = 4;
			listChild.Add(character);
			SetWay(false);
		}
	}

	public void EnableMiniMap() {
		miniMap.SetActive(true);
		SetWay(false);
	}

	private void CreatObject(string name, int posObject, NoteMiniMap noteMiniMap, int depth) {
		GameObject note = GameObject.Instantiate(assetMgr.GetAssetSync<GameObject>("Prefabs/General/UISprite")) as GameObject;
		listChild.Add(note);
		note.GetComponent<UISprite>().depth = depth;
		note.transform.parent = miniMap.transform;
		note.GetComponent<UISprite>().spriteName = name;
		note.GetComponent<UISprite>().MakePixelPerfect();
		note.transform.localPosition = new Vector3(noteMiniMap.x[posObject],
												   noteMiniMap.y[posObject], 0);
		note.name = name;
	}

	private void CreatLine() {
		int count = 0;
		int checkFirst = 0;
		List<GameObject> lines = new List<GameObject>();
		for (int i = 1; i < listPosition.Count; i++) {
			float distance = Vector3.Distance(listPosition[checkFirst], listPosition[i]);
			bool check = false;
			while (distance >= 45) {
				check = true;
				GameObject line = GameObject.Instantiate(assetMgr.GetAssetSync<GameObject>("Prefabs/General/UISprite")) as GameObject;
				listChild.Add(line);
				line.GetComponent<UISprite>().depth = 2;
				line.transform.parent = miniMap.transform;
				line.GetComponent<UISprite>().spriteName = "line";
				line.GetComponent<UISprite>().MakePixelPerfect();

				line.transform.localPosition = listPosition[checkFirst] + Vector3.ClampMagnitude(listPosition[i] - listPosition[checkFirst], 1) * 30;
				line.name = "line";
				lines.Add(line);
				if (lines.Count > 2)
					if (lines[lines.Count - 2].transform.localPosition == lines[lines.Count - 1].transform.localPosition) {
						lines[lines.Count - 1].transform.localPosition += Vector3.ClampMagnitude(lines[lines.Count - 2].transform.localPosition - lines[lines.Count - 3].transform.localPosition, 1) * 30;
					}

				distance -= 45;
				count++;
			}
			if (check) checkFirst = i;
		}
		for (int i = 0; i < lines.Count - 1; i++) {
			if (lines[i + 1].transform.position.x - lines[i].transform.position.x != 0) {
				lines[i].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan((lines[i + 1].transform.position.y - lines[i].transform.position.y)
																								 / (lines[i + 1].transform.position.x - lines[i].transform.position.x)) * 180 / Mathf.PI));
			}
			else {
				if (i > 0)
					lines[i].transform.localRotation = lines[i - 1].transform.localRotation;
			}
		}
		if (controlPoints[2].x - lines[lines.Count - 1].transform.localPosition.x != 0) {

			lines[lines.Count - 1].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan((controlPoints[2].y - lines[lines.Count - 1].transform.localPosition.y)
																												 / (controlPoints[2].x - lines[lines.Count - 1].transform.localPosition.x)) * 180 / Mathf.PI));
		}
		else {
			if (lines.Count > 2) {
				lines[lines.Count - 1].transform.localRotation = lines[lines.Count - 2].transform.localRotation;
			}
			else {
				if (lines.Count == 2)
					lines[lines.Count - 2].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
				lines[lines.Count - 1].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
			}
		}

	}

	private void SetWay(bool createLines) {
		if (count < listMovePosition.Count - 1) {
			controlPoints[0] = listMovePosition[count];
			count++;
			controlPoints[1] = listMovePosition[count];
			count++;
			controlPoints[2] = listMovePosition[count];
			Vector3 p0, p1, p2;
			for (int j = 0; j < controlPoints.Length - 2; j++) {
				p0 = 0.5f * (controlPoints[j] + controlPoints[j + 1]);
				p1 = controlPoints[j + 1];
				p2 = 0.5f * (controlPoints[j + 1] + controlPoints[j + 2]);

				Vector3 position;
				float t;
				float pointStep = 1.0f / numberOfPoints;
				if (j == controlPoints.Length - 3) {
					pointStep = 1.0f / (numberOfPoints - 1);
				}
				listPosition.Add(controlPoints[0]);
				for (int i = 0; i < numberOfPoints; i++) {
					t = i * pointStep;
					position = (1 - t) * (1 - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;

					listPosition.Add(position);
				}
				listPosition.Add(controlPoints[2]);
			}
			if (createLines) {
				CreatLine();
			}
			else {
				SetMove();
				routineRunner.StartCoroutine(DrawMiniMap(TIME_OPEN_MAP, false));
				routineRunner.StartCoroutine(HideMiniMap());
			}
		}
	}

	private IEnumerator HideMiniMap() {
		yield return new WaitForSeconds(1.5f);
		if (miniMap != null)
			routineRunner.StartCoroutine(DrawMiniMap(TIME_CLOSE_MAP, true));
	}

	private IEnumerator DrawMiniMap(float duration, bool hideMap) {
		float passedTime = 0;
		if (!hideMap)
			miniMap.GetComponent<UISprite>().width = 169;
		for (int i = 0; i < listChild.Count; i++) {
			listChild[i].GetComponent<UISprite>().fillAmount = 0;
		}
		while (passedTime < duration) {
			passedTime += Time.deltaTime;
			if (!hideMap)
				miniMap.GetComponent<UISprite>().width = (int)(169 + 1 / duration * passedTime * 331);
			else miniMap.GetComponent<UISprite>().width = (int)(500 - 1 / duration * passedTime * 331);

			yield return new WaitForEndOfFrame();
		}
		if (!hideMap) {
			for (int i = 0; i < listChild.Count; i++) {
				listChild[i].GetComponent<UISprite>().fillAmount = 1;
			}
		}
		if (!hideMap) {
			routineRunner.StartCoroutine(UpdateMove(2.2f));
		}
		else miniMap.SetActive(false);
	}

	private IEnumerator UpdateMove(float duration) {
		float passedTime = 0;

		while (passedTime < duration) {
			passedTime += Time.deltaTime;
			TweenParms parms = new TweenParms();
			parms.Prop("localPosition", endMarker).Ease(EaseType.EaseOutSine).OnComplete(new TweenDelegate.TweenCallback(SetMove));
			HOTween.To(character.transform, 2.2f / (numberOfPoints + 2) * Vector3.Distance(character.transform.localPosition, endMarker) / 7, parms);
			yield return new WaitForEndOfFrame();
		}
	}

	private void SetMove() {
		if (countTarget < listPosition.Count - 1) {
			character.transform.localPosition = listPosition[countTarget];
			endMarker = listPosition[countTarget + 1];
			countTarget++;
		}
	}
}