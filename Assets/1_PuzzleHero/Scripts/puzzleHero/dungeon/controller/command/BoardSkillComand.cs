using strange.extensions.command.impl;

public class BoardSkillComand : Command
{
	[Inject]
	public BoardSkillInfo skillInfo { get; set; }

	[Inject]
	public SkillsManager skillManager { get; set; }

	public override void Execute()
	{
//		skillManager.AddSkill(skillInfo);
	}
}