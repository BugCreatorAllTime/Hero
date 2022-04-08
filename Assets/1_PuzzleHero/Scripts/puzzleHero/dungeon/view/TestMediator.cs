using strange.examples.strangerocks;
using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
using Nfury.Base;

public class TestMediator : Mediator
{
	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic abstractLogic { get; set; }
	[Inject]
	public TestView view { get; set; }
	[Inject]
	public Team1PrepareAttackSignal team1Action { get; set; }
	[Inject]
	public AddActionSignal addAction { get; set; }
	[Inject]
	public Team1StartAttackSignal startAction { get; set; }

	[Inject]
	public BoardSkillSignal boardSkillSignal { get; set; }

	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public ItemService itemService { get; set; }
	[Inject]
	public ConfigManager configMgr { get; set; }

	[Inject]
	public GuiEventHandler guiEventHandler { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	[Inject]
	public AddActionSignal addActionSignal { get; set; }

	[Inject]
	public Team1StartAttackSignal team1StartAttackSignal { get; set; }

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	[Inject]
	public LoadingManager loadManager{ get; set;}

	private BattleGameLogic battleLogic;
	private bool canAttack = true;

	void Start()
	{
		battleLogic = abstractLogic as BattleGameLogic;
	
	}

	private void StartAction()
	{
		startAction.Dispatch(true);
	}

	void OnGUI0()
	{
		GUI.color = Color.red;
		if (battleLogic.FindEntity(BattleGameLogic.TEAM1) != null)
		{
			GUI.Label(new Rect(0, 0, 100, 100), "HP: " + battleLogic.FindEntity(BattleGameLogic.TEAM1).getStat().CurHp);
			GUI.Label(new Rect(0, 20, 100, 100), "ARMOR: " + battleLogic.FindEntity(BattleGameLogic.TEAM1).getStat().CurArmor);
		}
//		if (battleLogic.FindEntity(BattleGameLogic.TEAM2) != null)
//		{
//			GUI.Label(new Rect(280, 0, 100, 100), "HP: " + battleLogic.FindEntity(BattleGameLogic.TEAM2).getStat().CurHp);
//			GUI.Label(new Rect(280, 20, 100, 100), "TURN: " + battleLogic.FindEntity(BattleGameLogic.TEAM2).CurTurn);
//		}

		GUIStyle style = new GUIStyle();
		style.fontStyle = FontStyle.Bold;
		style.fontSize = 20;
		style.normal.textColor = Color.magenta;
		GUI.Label(new Rect(0, 400, 300, 100), "board locked " + ((MatchLogic)matchLogic).IsBoardInputDisabled(), style);
		GUI.Label(new Rect(0, 425, 300, 100), "action queue " + battlePhaseManager.QueueCount(), style);
		GUI.Label(new Rect(0, 450, 300, 100), "gui displayed " + guiEventHandler.IsGuiBeingDisplayed(), style);

		if (GUI.Button(new Rect(0, 200, 60, 60), "Attack"))
		{
			/*battleLogic.FindEntity(BattleGameLogic.TEAM1).AddSkill(new DefaultSkill(3));
			battleLogic.FindEntity(BattleGameLogic.TEAM1).skills[0].Active(battleLogic.FindEnemy(BattleGameLogic.TEAM1));
			battleLogic.FindEntity(BattleGameLogic.TEAM1).skills[0].OnPhaseChangedEvent += new BaseSkill.OnPhaseChanged(
				delegate(ActionPhase phase) {
					                            if (phase == ActionPhase.END)
					                            {
													battleLogic.FindEntity(BattleGameLogic.TEAM1).skills.RemoveAt(0);
					                            }
				});*/
			battleLogic.FindEntity(BattleGameLogic.TEAM1).AddMana(100);
//			battleLogic.FindEntity(BattleGameLogic.TEAM1).DealDmgTo(battleLogic.FindEntity(BattleGameLogic.TEAM2), 20000);
		}
		if (GUI.Button(new Rect(200, 0, 60, 60), "Restart"))
		{
			Destroy(GameObject.Find("FingerGestures"));
			loadManager.SetScreen("Dungeon");
			Application.LoadLevel("Loading");
		}

//		if (GUI.Button(new Rect(100, 100, 100, 50), Skills.DestroySkill.ToString()))
//		{
//			BoardSkillInfo skillInfo = new BoardSkillInfo();
//			skillInfo.affectedGemType = Data.TileTypes.Heal;
//			skillInfo.numberOfAffectedGems = 3;
//			skillInfo.numberOfTurnsToActivate = 0;
//			skillInfo.skillType = Skills.DestroySkill.ToString();
//			boardSkillSignal.Dispatch(skillInfo);
//		}
//		if (GUI.Button(new Rect(200, 100, 100, 50), Skills.TransformSkill.ToString()))
//		{
//			BoardSkillInfo skillInfo = new BoardSkillInfo();
//			skillInfo.numberOfAffectedGems = 3;
//			skillInfo.numberOfTurnsToActivate = 0;
//			skillInfo.fromGemType = Data.TileTypes.Heal;
//			skillInfo.toGemType = Data.TileTypes.Gold;
//			skillInfo.skillType = Skills.TransformSkill.ToString();
//			boardSkillSignal.Dispatch(skillInfo);
//		}
		if (GUI.Button(new Rect(300, 200, 100, 50), "home"))
		{
			loadManager.SetScreen("WorldMap");
			Application.LoadLevel("Loading");
		}
//		if (GUI.Button(new Rect(300, 100, 100, 50), "Shuffle"))
//		{
//			((MatchLogic)matchLogic).Shuffle();
//		}
//		if (GUI.Button(new Rect(400, 100, 100, 50), "AdventurerSetSkill"))
//		{
//			battleLogic.setItemSkill = "AdventurerSetSkillWrapper";
//		}
//		if (GUI.Button(new Rect(100, 150, 100, 50), typeof(HunterSetSkill).ToString()))
//		{
//			battleLogic.setItemSkill = typeof(HunterSetSkill).ToString();
//		}
//		if (GUI.Button(new Rect(200, 150, 100, 50), typeof(BronzeSetSkill).ToString()))
//		{
//			battleLogic.setItemSkill = typeof(BronzeSetSkill).ToString();
//		}
//		GUI.Label(new Rect(100, 10, 100, 100), battleLogic.setItemSkill);
		if (GUI.Button(new Rect(0, 120, 60, 60), "TakeOffItem"))
		{
			GameObject go = battleLogic.FindEntity(BattleGameLogic.TEAM1).gObject;
			SkeletonAnimation skeAni = go.GetComponent<SkeletonAnimation>();
			itemService.TakeOffItemView(skeAni, configMgr.UserData.EquippedItemData[0]);
		}

		GUI.Label(new Rect(0,60,100,60),"gem used: " + configMgr.UserData.gainedGem.ToString());
	}

	private IEnumerator Attack(float delay)
	{
		yield return new WaitForSeconds(delay);
		team1StartAttackSignal.Dispatch(true);
		canAttack = true;
	}
}
