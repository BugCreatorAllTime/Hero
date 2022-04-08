using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using System.Collections.Generic;

public class UpgradeFxCommand : Command {

	[Inject]
	public ItemBaseData item {get;set;}

	[Inject]
	public GameObject player {get;set;}

	[Inject]
	public string type { get; set;}

	[Inject]
	public AssetMgr assetMgr { get; set; }

	private Dictionary<int, Color32> colorsFx;
	private Dictionary<string, string> typeFx;
	private Color32 fifthFx = new Color32 (0,255,255,255);
	private Color32 tenthFx = new Color32 (255,203,0,255);
	private string fxPathWeapon = "Animation/Fx/GradeFx/TenthGradeWeapon";
	private string fxPathShield = "Animation/Fx/GradeFx/TenthGradeShield";

	[PostConstruct]
	public void PostConstruct()
	{
		colorsFx = new Dictionary<int, Color32> ();
		colorsFx.Add (5, fifthFx);
		colorsFx.Add (10, tenthFx);

		typeFx = new Dictionary<string, string> ();
		typeFx.Add (ItemCfg.WEAPON, fxPathWeapon);
		typeFx.Add (ItemCfg.SHIELD, fxPathShield);
	}

	public override void Execute ()
	{
		GameObject fx = GetObject();
		if(item.LevelUpgrade < 5)
		{
			if(fx != null)
			{
				fx.SetActive(false);
			}
		} else {
			if(fx != null)
			{
				fx.SetActive(true);
				OnFxObjectCreated(fx);
			} else {
				string path = "";
				typeFx.TryGetValue(type, out path);
				assetMgr.GetAsset<GameObject>(path, delegate (GameObject go){
					fx = GameObject.Instantiate(go) as GameObject;
					fx.name = type;
					fx.transform.parent = player.transform;
					OnFxObjectCreated(fx);
				});
			}
			
		}
	}

	private void OnFxObjectCreated(GameObject fx)
	{
		fx.GetComponent<UpgradePosition>().item = item;
		fx.GetComponent<UpgradePosition>().ResetPos();
		int level = 10;
		if (item.LevelUpgrade < 10) level = 5;
		Color32 color = Color.red;
		colorsFx.TryGetValue(level, out color);
		fx.GetComponentInChildren<ParticleSystem>().startColor = color;
	}

	private GameObject GetObject()
	{
		for(int i = 0; i < player.transform.childCount; i++)
		{
			GameObject child = player.transform.GetChild(i).gameObject;
			if(child.name == type)
			{
				return child;
			}
		}
		return null;
	}
}
