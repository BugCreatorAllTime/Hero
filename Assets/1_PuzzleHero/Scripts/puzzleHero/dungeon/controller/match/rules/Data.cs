/// <summary>
/// Default setting and type of tile..
/// </summary>
public class Data
{
	public const int tileWidth = 7;
	public const int tileHeight = 6;

	public enum TileTypes
	{
		Empty,
		Attack,
		Defend,
		Skill,
		Heal,
		Gold,
		Chest
	};
}