using UnityEngine;
using System.Collections;

public class MapPieceContent : MonoBehaviour{

	public int indexMap;
	public NoteDungeonManager noteManager;
	public AssetMgr mgr;
	public Material newMaterial;
	private const string mapPath = "Textures/WorldMap/";
	private const string mapName = "map";

	public void UpdateData(int index, int increase)
	{
		this.indexMap = index;
		mgr.GetAsset<Texture2D>(mapPath + mapName + index, delegate (Texture2D mTexture){
			if(CheckLoadSuccess(indexMap))
			{
				UITexture sprite = gameObject.GetComponent<UITexture>();
				sprite.drawCall.baseMaterial.mainTexture = mTexture;
				sprite.drawCall.dynamicMaterial.mainTexture = mTexture;
				sprite.MakePixelPerfect();
				noteManager.UnLoadNoteDungeon(index + increase);
				noteManager.LoadNoteDungeon(index);
			} else {
				ReLoad(increase);
			}
		});
	}

	private bool CheckLoadSuccess(int indexCheck)
	{
		if(indexCheck == gameObject.transform.localPosition.y/960)
			return true;
		else return false;
	}

	private void ReLoad(int increase)
	{
		int index = (int)gameObject.transform.localPosition.y / 960;
		mgr.GetAsset<Texture2D>(mapPath + mapName + index, delegate (Texture2D mTexture){
			if(CheckLoadSuccess(index))
			{
				UITexture sprite = gameObject.GetComponent<UITexture>();
				sprite.drawCall.baseMaterial.mainTexture = mTexture;
				sprite.drawCall.dynamicMaterial.mainTexture = mTexture;
				sprite.MakePixelPerfect();
				noteManager.UnLoadNoteDungeon(index + increase);
				noteManager.LoadNoteDungeon(index);
			} else {
				ReLoad(increase);
			}
		});
	}
}
