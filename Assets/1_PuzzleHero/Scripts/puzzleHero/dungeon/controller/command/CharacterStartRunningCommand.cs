using Nfury.Base;
using strange.extensions.command.impl;

public class CharacterStartRunningCommand : Command
{
	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	[Inject]
	public BattlePhaseManager battlePhaseManager { get; set; }

	public override void Execute()
	{
		((MatchLogic)matchLogic).DisableBoardInput();
		battlePhaseManager.StartRunning();
	}
}