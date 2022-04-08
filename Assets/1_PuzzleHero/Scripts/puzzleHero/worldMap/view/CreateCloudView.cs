using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class CreateCloudView : View {

	[Inject]
	public AssetMgr mgr{get; set;}

	private const string cloudName = "Prefabs/WorldMap/Cloud";
	private GameObject cloudPrefab;
	private GameObject grid;

	internal void Init()
	{
		mgr.GetAsset<GameObject>(cloudName, CreateCloudLoad);
		grid = GameObject.Find ("Grid");
	}

	private void CreateCloudLoad(GameObject go)
	{
		cloudPrefab = go;
		int countIndex = 0;
		int index = 0;
		bool check = false;
		for (int i = 0; i < grid.transform.childCount; i++)
		{
			for(int j = 0; j < 2; j++)
			{
				GameObject cloud = NGUITools.AddChild(grid.transform.GetChild(i).gameObject, cloudPrefab);
				OscillateView dContent = cloud.GetComponent<OscillateView>();
				dContent.state = OscillateView.HORIZONTAL;
				dContent.start = check;
				dContent.index = index;
				dContent.numerical = countIndex+1;
				countIndex++;
				if(countIndex > 2)
				{
					countIndex = 0;
					check = RandomDirection();
				}
			}
		}
	}

	public bool RandomDirection()
	{
//		int check = Random.Range (0,2);
//		if (check == 0)
//			return true;
//		else
//			return false;
		return false;
	}

}
