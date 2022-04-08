public class GA
{
	[Inject]
	public CrossContextData crossContextData { get; set; }

	[Inject]
	public ConfigManager configManager { get; set; }

	private int currentBattleMonsterId = -1;

	public class Category
	{
		public const string characterState = "Character_state";
		public const string itemEquipment = "Item_equipment";
		public const string itemGem = "Item_gem";
		public const string itemGold = "Item_gold";
		public const string itemEnergy = "Item_energy";
		public const string itemChest = "Item_chest";
		public const string time = "Time";
	}

	public class Action
	{
		public const string die = "Die";
		public const string revive = "Revive";
		public const string upgrade = "Upgrade";
		public const string pick = "Pick";
		public const string buy = "Buy";
		public const string spendInDungeon = "Spend_in_dungeon";
	}

	public class Label
	{
		public const string empty = "";
		public const string theme = "Theme_{0}_{1}";
		public const string dungeon = "Dungeon_{0}_{1}";
		public const string fromTo = "{0}_to_{1}";
	}

	[PostConstruct]
	public void Init() {

	}

	public void TrackFailureInDungeon()
	{
		TrackFailureInDungeon(currentBattleMonsterId);
	}

	public void TrackFailureInDungeon(int monsterId)
	{
		Track(Category.characterState, Action.die, monsterId.ToString());
	}

	public void TrackRevive()
	{
		Track(Category.characterState, Action.revive, Label.empty);
	}

	public void TrackEquipmentUpgrade(int itemId)
	{
		int dungeon = configManager.UserData.curMapId;
		//Logger.Trace(dungeon);
		TrackEquipmentUpgrade(itemId, dungeon);
	}

	public void TrackEquipmentUpgrade(int itemId, int dungeon)
	{
		int theme = FindThemeOf(dungeon);
		Track(Category.itemEquipment, Action.upgrade, string.Format(Label.theme, theme, itemId));
	}

	public void TrackEquipmentPick(int dungeon, int count)
	{
		Track(Category.itemEquipment, Action.pick, string.Format(Label.dungeon, dungeon, count));
	}

	public void TrackCashItemBuy(int type, int id)
	{
		switch (type)
		{
			case ShopCfg.SHOP_ITEM_ENERGY:
				TrackEnergyBuy(id);
				break;
			case ShopCfg.SHOP_ITEM_GOLD:
				TrackGoldBuy(id);
				break;
			case ShopCfg.SHOP_ITEM_GEM:
				TrackGemBuy(id);
				break;
		}
	}

	public void TrackGemBuy(int gemId)
	{
		Track(Category.itemGem, Action.buy, gemId.ToString());
	}

	public void TrackGoldBuy(int goldId)
	{
		Track(Category.itemGold, Action.buy, goldId.ToString());
	}

	public void TrackEnergyBuy(int energyId)
	{
		Track(Category.itemEnergy, Action.buy, energyId.ToString());
	}

	public void TrackChestBuy(int chestId)
	{
		Track(Category.itemChest, Action.buy, chestId.ToString());
	}

	public void TrackTimeSpendInDungeon(int second)
	{
		Track(Category.time, Action.spendInDungeon, FindRangeOf(second));
	}

	public void TrackEquipmentBuy(int itemId)
	{
		Track(Category.itemEquipment, Action.buy, itemId.ToString());
	}

	public void SetCurrentBattleMonsterId(int monsterId)
	{
		this.currentBattleMonsterId = monsterId;
	}

	private void Track(string category, string action, string label)
	{
		Track(category, action, label, 1);
	}

	private void Track(string category, string action, string label, int value)
	{
		//Logger.Trace("TRACK ", category, action, label, value);
		Analytics.gua.sendEventHit(category, action, label, value);
	}

	private int FindThemeOf(int dungeon)
	{
		int[] configRanges = configManager.general.GAThemeRanges;
		int[][] ranges = new int[configRanges.Length/2][];
		for (int i = 0; i < configRanges.Length / 2; i++)
		{
			int min = configRanges[i * 2];
			int max = configRanges[i * 2 + 1];
			ranges[i] = new[] {min, max};
		}
		
		int theme = -1;
		for (int i = ranges.Length; i > 0; i--) {
			int[] range = ranges[i - 1];
			int min = range[0];
			int max = range[1];
			if (min <= dungeon && dungeon <= max) {
				theme = i;
			}
		}
		return theme;
	}

	private string FindRangeOf(int spendTime)
	{
		int[][] ranges = new[]
		{
			new int[] {0, 300},
			new int[] {301, 600},
			new int[] {601, 900},
			new int[] {901, 1200},
			new int[] {1201, int.MaxValue}
		};

		string s = "";
		for (int i = ranges.Length; i > 0; i--) {
			int[] range = ranges[i - 1];
			int min = range[0];
			int max = range[1];
			if (min <= spendTime && spendTime <= max)
			{
				min = min / 60;
				max = max / 60;
				s = string.Format(Label.fromTo, min, max);
			}
		}
		return s;
	}
}