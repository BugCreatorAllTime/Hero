/// <summary>
/// To record the location of tile data.
/// </summary>
public class Cell
{
	public Data.TileTypes cellType;

	public bool IsEmpty
	{
		get { return cellType == Data.TileTypes.Empty; }
	}

	public void SetRandomTile(int total)
	{
		cellType = (Data.TileTypes) UnityEngine.Random.Range(0, total) + 1;
	}
}