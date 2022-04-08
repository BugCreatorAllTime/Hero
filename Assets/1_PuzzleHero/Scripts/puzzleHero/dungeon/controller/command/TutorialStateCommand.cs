using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;
using Nfury.Base;

public class TutorialStateCommand : Command {

	[Inject]
	public int type { get; set;}

	[Inject]
	public string monsterSkill { get; set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set;}

	[Inject]
	public TutorialFirstBattleLogic tutLogic { get; set;}

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public GuiEventHandler guiHandler { get; set;}

	[Inject(DungeonContext.BATTLE_LOGIC)]
	public AbstractGameLogic battleLogic { get; set; }

	public override void Execute()
	{
		if(monsterSkill != "")
		{
			if(GetFlagSkill(monsterSkill))
			{
				HomeTownTutorialData tutoData = tutLogic.GetDataTut(monsterSkill);
				tutoData.Description = tutoData.Description.Replace(";",",");
				tutLogic.checkInput = false;
				routineRunner.StartCoroutine(ShowTutorial(tutoData,1.25f));
			}
			monsterSkill = "";
		}

		if(config.UserData.currentStepTownTutorial < TutorialFirstBattleLogic.START)
		{
			if(tutLogic.countTurn == 0 || tutLogic.countTurn > config.general.NumberFreeTurn)
				config.UserData.NextStep();
			bool check = true;
			if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.TUT_MATCH_5)
			{
				if(tutLogic.countTurn <= config.general.NumberFreeTurn - 1)
				{
					check = false;
					if(tutLogic.countTurn == 0)
						tutLogic.countTurn++;
				} else {
					routineRunner.StartCoroutine(ChangeBoard(6));
				}
			}
			if(check)
			{
				HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial (config.UserData.currentStepTownTutorial);
				if(config.UserData.currentStepTownTutorial ==  TutorialFirstBattleLogic.TUT_MANA)
				{
					tutLogic.AddManaTut();
				}
				if(tutoData.Type == HomeTownTutorialCfg.TUTORIAL)
				{
					if(config.UserData.currentStepTownTutorial == TutorialFirstBattleLogic.TRANS)
					{
						routineRunner.StartCoroutine(ChangeBoard(12));
					}
					tutoData.Description = tutoData.Description.Replace(";",",");
					tutLogic.checkInput = false;
					routineRunner.StartCoroutine(ShowTutorial(tutoData,0.25f));
				}
			}
		}
	
		if(type == TutorialFirstBattleLogic.TUT_FRIST_BRING)
		{
			if(config.UserData.GetFirstBringDown() == 1 && config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.FIRST_BRING)
			{
				tutLogic.current = config.UserData.currentStepTownTutorial;
				config.UserData.currentStepTownTutorial = TutorialFirstBattleLogic.FIRST_BRING;
			}
			if(config.UserData.currentStepTownTutorial <= TutorialFirstBattleLogic.END_BRING && config.UserData.currentStepTownTutorial >= TutorialFirstBattleLogic.FIRST_BRING
			   && config.UserData.GetFirstBringDown() == 1)
			{
				HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial (config.UserData.currentStepTownTutorial);
				tutoData.Description = tutoData.Description.Replace(";",",");
				routineRunner.StartCoroutine(ShowTutorial(tutoData,0.25f));
			} else {

			}
		}
		if(type == TutorialFirstBattleLogic.TUT_BRING_END)
		{
			config.UserData.currentStepTownTutorial = tutLogic.current;
		}
		if(type == TutorialFirstBattleLogic.TUT_COLLECT_GOLD)
		{
			HomeTownTutorialData tutoData = config.townTutorialCfg.getTownTutorial (TutorialFirstBattleLogic.ID_TUT_COLLECT_GOLD);
			if(tutoData.Type == HomeTownTutorialCfg.TUTORIAL)
			{
				tutoData.Description = tutoData.Description.Replace(";",",");
				tutLogic.checkInput = false;
				routineRunner.StartCoroutine(ShowTutorial(tutoData,0.25f));
			}
		}
	}

	private IEnumerator ShowTutorial(HomeTownTutorialData tutoData, float duration)
	{
		yield return new WaitForSeconds(duration);
		if(((BattleGameLogic) battleLogic).FindEntity(BattleGameLogic.TEAM1).getStat().CurHp != 0)
		{
			if (type == TutorialFirstBattleLogic.TUT_SHOW) {
				guiHandler.ShowTutorial (tutoData, type);
			} else if (type == TutorialFirstBattleLogic.TUT_FRIST_BRING) {
				guiHandler.TutorialShow (tutoData);
			}
			else if (type == TutorialFirstBattleLogic.TUT_FIRST_SKILL_MONSTER)
			{
				guiHandler.ShowTutorial(tutoData, type);;
			} else if(type == TutorialFirstBattleLogic.TUT_COLLECT_GOLD)
			{
				guiHandler.ShowTutorial(tutoData, type);
			}
		}
	}

	private bool GetFlagSkill(string nameSkill)
	{
		switch(nameSkill)
		{
			case TutorialFirstBattleLogic.POISON:
				if(config.UserData.GetTutPoison() == 1)
				{
					config.UserData.SetTutPoison(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.FIRE:
				if(config.UserData.GetTutFire() == 1)
				{
					config.UserData.SetTutFire(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.ICE:
				if(config.UserData.GetTutIce() == 1)
				{
					config.UserData.SetTutIce(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.ROCK:
				if(config.UserData.GetTutRock() == 1)
				{
					config.UserData.SetTutRock(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.TRANSFORM:
				if(config.UserData.GetTutTransform() == 1)
				{
					config.UserData.SetTutTransform(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.DESTROY:
				if(config.UserData.GetTutDestroy() == 1)
				{
					config.UserData.SetTutDestroy(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.SHUFFLE:
				if(config.UserData.GetTutShuffle() == 1)
				{
					config.UserData.SetTutShuffle(0);
					return true;
				} else return false;
				break;
			case TutorialFirstBattleLogic.HEAL:
				if(config.UserData.GetTutHeal() == 1)
				{
					config.UserData.SetTutHeal(0);
					return true;
				} else return false;
				break;
			default:
				return false;
				break;
			}
	}

	private IEnumerator ChangeBoard(int index)
	{
		yield return new WaitForSeconds(0.75f);
		tutLogic.InitTileGrid(index);
	}
}
