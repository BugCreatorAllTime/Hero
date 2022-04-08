using Nfury.Base;
using strange.extensions.command.impl;

public class EnableBoardInputCommand : Command
{
	[Inject(DungeonContext.MATCH_LOGIC)]
	public AbstractGameLogic matchLogic { get; set; }

	public override void Execute()
	{
//		((MatchLogic) matchLogic).EnableBoardInput();
	}
}