using strange.examples.strangerocks;
using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.extensions.pool.api;

public class WinNodeCommand : Command
{
	[Inject]
	public CrossContextData CrossContextData { get; set; }
	[Inject]
	public ConfigManager config { get; set; }
	[Inject]
	public CreateTeam2Signal createTeam2Signal { get; set; }
	[Inject]
	public MiniMapManager minimap { get; set; }
	[Inject]
	public DungeonState dState {get;set;}
	[Inject(GuiObjectName.victoryPopup)]
	public GameObject victoryPopup { get; set; }
	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	[Inject]
	public DungeonState dungeonState { get; set; }

	[Inject]
	public FbHandler fbHandler { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public Handler handler { get; set; }

	public override void Execute()
	{
		//Logger.Trace(GetType(), "::Execute");
		Handler h = handler;
		h.Wait();
	}

	
}

public class Handler {
	[Inject]
	public BattlePhaseManager bpm { get; set; }
	[Inject]
	public CrossContextData CrossContextData { get; set; }
	[Inject]
	public ConfigManager config { get; set; }
	[Inject]
	public CreateTeam2Signal createTeam2Signal { get; set; }
	[Inject]
	public MiniMapManager minimap { get; set; }
	[Inject]
	public DungeonState dState { get; set; }
	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }
	[Inject]
	public DungeonState dungeonState { get; set; }
	[Inject]
	public FbHandler fbHandler { get; set; }

	private bool waitForCharacterTurnFinish = false;
	BattlePhaseManager.BattlePhase prevPhase = BattlePhaseManager.BattlePhase.Character;

	[PostConstruct]
	public void Init()
	{
		//Logger.Trace(GetType(),"::postConstruct");
		bpm.OnBattlePhaseChangedEvent += OnPhaseChange;
	}

	public void Wait()
	{
		waitForCharacterTurnFinish = true;
		if (bpm.Phase == BattlePhaseManager.BattlePhase.Neutral)
		{
			OnPhaseChange(BattlePhaseManager.BattlePhase.Neutral);
		}
	}

	private void OnPhaseChange(BattlePhaseManager.BattlePhase phase) {
		if (phase != prevPhase)
		{
			prevPhase = phase;
			//Logger.Trace(GetType(), "::OnPhaseChange", phase);
		}
		if (!waitForCharacterTurnFinish) return;
		if (phase == BattlePhaseManager.BattlePhase.Neutral) {
			if (CrossContextData.monsterIndexInDungeon < config.DungeonCfg.dungeon[CrossContextData.dungeonId.ToString()].IdMonster.Count - 1) {
				CrossContextData.monsterIndexInDungeon++;
				createTeam2Signal.Dispatch(CrossContextData.monsterIndexInDungeon);
				minimap.EnableMiniMap();
			}
			else {
				dungeonState.state = DungeonState.State.Win;
				guiEventHandler.ShowVictoryPopUp();
				config.UserData.UnlockNextMap(CrossContextData.dungeonId);
				fbHandler.SendScore();
			}
			//bpm.OnBattlePhaseChangedEvent -= OnPhaseChange;
			waitForCharacterTurnFinish = false;
		}
	}
}
