using System.Collections.Generic;

public class BattleBackgroundsCfg
{
	public Dictionary<string, BattleBackgrounds> BattleBg = new Dictionary<string, BattleBackgrounds>();
}

public class BattleBackgrounds
{
	public int Id { get; set; }
	public List<string> Backgrounds { get; set; }
}