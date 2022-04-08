using System;
using strange.extensions.mediation.impl;

public class HudMediator : Mediator
{
	[Inject]
	public HudView hudView { get; set; }

	[Inject]
	public CharacterHpChangedSignal characterHpChangedSignal { get; set; }

	[Inject]
	public MonsterHpChangedSignal monsterHpChangedSignal { get; set; }

	[Inject]
	public CharacterDefendChangedSignal characterDefendChangedSignal { get; set; }

	[Inject]
	public CharacterSkillChangedSignal characterSkillChangedSignal { get; set; }

	[Inject]
	public MonsterTurnChangedSignal monsterTurnChangedSignal { get; set; }

	[Inject]
	public RemainingMovesChangedSignal remainingMovesChangedSignal { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	public override void OnRegister()
	{
//		Logger.Trace("hudMediator OnRegister");
		hudView.Init();
		characterHpChangedSignal.AddListener(OnCharacterHpChanged);
		monsterHpChangedSignal.AddListener(OnMonsterHpChanged);
		characterDefendChangedSignal.AddListener(OnCharacterDefendChanged);
		characterSkillChangedSignal.AddListener(OnCharacterSkillChanged);
		monsterTurnChangedSignal.AddListener(OnMonsterTurnChanged);
		remainingMovesChangedSignal.AddListener(OnRemainingMovesChanged);
		hudView.onClick.AddListener(OnClick);
	}

	public override void OnRemove()
	{
	}

	public void OnCharacterHpChanged(int currentHp, int maxHp)
	{
//		Logger.Trace("new hp value ");
		hudView.OnCharacterHpChanged(currentHp, maxHp);
	}

	public void OnMonsterHpChanged(int currentHp, int maxHp)
	{
		hudView.OnMonsterHpChanged(currentHp, maxHp);
	}

	public void OnCharacterDefendChanged(int currentDef, int maxDef)
	{
		hudView.OnDefendChanged(currentDef, maxDef);
	}

	public void OnCharacterSkillChanged(float currentEnergy)
	{
		hudView.OnCharacterSkillChanged(currentEnergy);
	}

	public void OnMonsterTurnChanged(int turn)
	{
		hudView.OnMonsterTurnChanged(turn);
	}

	public void OnRemainingMovesChanged(string text)
	{
		hudView.SetRemainingMoves(text);
	}

	private void OnClick(string buttonName)
	{
		guiEventHandler.HandleClick(buttonName);
	}
}