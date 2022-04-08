using System;

public class GlobalConfig
{
	public GlobalConfig ()
	{
	}

	public int[] WeightPerColors {get; set;}
	public int WeightPerLevel {get; set;}
	public int MaxItemLevelUpgrade {get; set;}
	public int MaxItemForUpgrade {get; set;}

	public int[] ItemLevelThreshold {get; set;}
	public int[] ItemStatUpgradeRatePerLevel {get; set;}
	public int UserSellDecreaseRate {get;set;}
	public int UserBuyBackDecreaseRate {get;set;}
	public int MaxBuyBackSlot {get;set;}
	public int BuyBackExpireTime {get;set;}
	public int ExpMonsterDropFormular {get;set;}
	public int GoldMonsterDropFormular {get;set;}
	public int GoldPerWeight {get;set;}

	public int[] AttackMatchBonusRates {get;set;}
	public int[] ArmorMatchBonusRates {get;set;}
	public int[] HealMatchBonusRates {get;set;}
	public int[] GoldMatchBonusRates {get;set;}
	public int[] ManaMatchBonusRates {get;set;}
	public int GoldMatchBase {get;set;}
	public int ArmorMatchBase {get;set;}
	public int EnergyCooldown {get;set;}
	public string GoogleStore_PublicKey {get;set;}
	public string AppStore_Validate_Url {get;set;}
	public bool AppStore_EnableValidate {get;set;}
	public string AppStoreUrl { get; set; }
	public string GooglePlayUrl { get; set; }
	public int[] GAThemeRanges { get; set; }
	public int HintTime { get; set; }
	public int TimeCheckNotifyFB { get; set;}
	public int[] ComboBonus { get; set; }
	public int NumberRequestFB {get;set;}
	public int NumberFreeTurn { get; set;}
}


