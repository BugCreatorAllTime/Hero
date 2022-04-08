using System;

public class BoardSkillInfo
{
	public enum AffectType
	{
		Attack,
		Defend,
		Skill,
		Heal,
		Gold,
		Random
	}

	public string skillType;
	public string affectedGemType;
	public string fromGemType;
	public string toGemType;
	public int numberOfAffectedGems;
	public int numberOfTurnsToActivate;
	public int numberOfTurnsToDeactivate;
	public int skillRate;
	public string animation;
	public string extras;

	public Data.TileTypes Convert(AffectType affectType)
	{
		return (Data.TileTypes)Enum.Parse(typeof(Data.TileTypes), affectType.ToString());
	}

	public AffectType Parse(string affectType)
	{
		return (AffectType) Enum.Parse(typeof (AffectType), affectType);
	}
}