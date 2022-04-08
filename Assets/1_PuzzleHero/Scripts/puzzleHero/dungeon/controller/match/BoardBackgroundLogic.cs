using System.Collections.Generic;
using strange.extensions.pool.api;
using UnityEngine;

public class BoardBackgroundLogic
{
	private const int boardBackgroundWidth = 640;
	private const int boardBackgroundHeight = 555;
	public const string boardBg = "Prefabs/Board/BoardBg";
	public const string boardBgMaterial = "Prefabs/Fx/Materials/BoardBackground";

	private UI2DSprite background;
	private List<UISprite> cellBackgrounds;
	private IPool<GameObject> pool;
	private GameObject boardBackgrounds;
	private AssetMgr assetMgr;
	private BoardGrayscale boardGrayscale;
	private MatchLogic matchLogic;
	private GameObject boardBackground;
	private PreloadedAssets preloadedAssets;

	public UI2DSprite Background
	{
		get { return background; }
	}

	public BoardBackgroundLogic(MatchLogic matchLogic, AssetMgr assetMgr, GameObject boardBackgrounds, PreloadedAssets preloadedAssets)
	{
		cellBackgrounds = new List<UISprite>();
		this.boardBackgrounds = boardBackgrounds;
		this.assetMgr = assetMgr;
		this.matchLogic = matchLogic;
		this.preloadedAssets = preloadedAssets;
		if (matchLogic.config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.START) return;
		boardGrayscale = new BoardGrayscale(this.matchLogic, this);
	}

	public void InitObjects(IPool<GameObject> pool)
	{
		if (this.pool == null)
		{
			this.pool = pool;
		}

		//assetMgr.GetAsset<GameObject> (boardBg, LoadedBackgroundObject);
		this.boardBackground = (GameObject)preloadedAssets.GetAsset(boardBg);

		for (int column = 0; column < Data.tileWidth; column++)
		{
			int startRow = 0;
			if (column % 2 == 0)
			{
				startRow = 1;
			}
			for (int row = 0; row < Data.tileHeight; row++)
			{
				if (row >= startRow)
				{
					GameObject cellBackgroundObject = pool.GetInstance();
					cellBackgroundObject.transform.parent = boardBackgrounds.transform;
					float cellX = column * MatchLogic.cellWidth + MatchLogic.cellWidth / 2;
					float cellY = row * MatchLogic.cellHeight + MatchLogic.cellHeight / 2;
					cellBackgroundObject.transform.localPosition = new Vector3(cellX, cellY, 0);
					cellBackgroundObject.transform.localScale = Vector3.one;
					UISprite cellBackground = cellBackgroundObject.GetComponent<UISprite>();
					cellBackgrounds.Add(cellBackground);
					//					Logger.Trace("column ", column, " row ", row);
					startRow += 2;
				}
			}
		}
	}

	private void LoadedBackgroundObject(GameObject go)
	{
		this.boardBackground = go;
		
	}

	public static string GetBgTexturePath(BoardBgInfo boardBgInfo)
	{
		return boardBgInfo.Background;
	}

	public void UpdateAssets(BoardBgInfo boardInfo)
	{
				GameObject backgroundObject = GameObject.Instantiate(boardBackground) as GameObject;
				background = backgroundObject.GetComponent<UI2DSprite>();
				background.material = (Material)preloadedAssets.GetAsset(boardBgMaterial);
				backgroundObject.transform.parent = boardBackgrounds.transform;
				backgroundObject.transform.localScale = Vector3.one;
				Texture2D texture2D = (Texture2D)preloadedAssets.GetAsset(boardInfo.Background);
				background.material.mainTexture = texture2D;
				background.sprite2D = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 1);
				int width = Mathf.CeilToInt(boardBackgroundWidth * MatchLogic.cellScaleX);
				int height = Mathf.CeilToInt(boardBackgroundHeight * MatchLogic.cellScaleY);
				background.SetDimensions(width, height);
				int i = 0;
				foreach (UISprite cellBackground in cellBackgrounds) {
					string spriteName = null;
					if (i % 2 == 0) {
						spriteName = boardInfo.FirstCell;
					}
					else {
						spriteName = boardInfo.SecondCell;
					}
					cellBackground.spriteName = spriteName;
					cellBackground.transform.localScale = MatchLogic.cellScale;
					i++;
				}
				
				float boardX = background.width / 2 - 5;
				float boardY = background.height / 2 - 5;
				background.gameObject.transform.localPosition = new Vector3(boardX, boardY, 0);
	}
}