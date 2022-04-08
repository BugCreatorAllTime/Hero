using UnityEngine;

public class ComboBonus
{
	private const int defaultComboCount = -1;

	private int autoMatchCombo = defaultComboCount;
	private MatchLogic matchLogic;
	private int bonusDamage = 0;
	private int firstMatchDamage = 0;

	public int BonusDamage
	{
		get { return bonusDamage; }
		set { bonusDamage = value; }
	}

	public int FirstMatchDamage
	{
		get { return firstMatchDamage; }
		set { firstMatchDamage = value; }
	}

	public ComboBonus(MatchLogic matchLogic)
	{
		this.matchLogic = matchLogic;
	}

	public int GetComboCount()
	{
		return autoMatchCombo;
	}

	public void CountCombo()
	{
		autoMatchCombo++;
		bonusDamage = Mathf.CeilToInt(matchLogic.dungeonService.GetBonusFromCombo(autoMatchCombo) * firstMatchDamage);
		//Logger.Trace("combo", autoMatchCombo, "first match damage", firstMatchDamage, "bonus damage", bonusDamage, "raw bonus dmg", matchLogic.dungeonService.GetBonusFromCombo(autoMatchCombo) * (float)firstMatchDamage);
	}

	public void ResetCombo()
	{
		autoMatchCombo = defaultComboCount;
		bonusDamage = 0;
		firstMatchDamage = 0;
		//Logger.Trace("reset combo");
	}

	public bool IsUserFirstMatch()
	{
		return autoMatchCombo == defaultComboCount;
	}
}