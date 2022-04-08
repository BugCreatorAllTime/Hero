using UnityEngine;
using System.Collections;
using strange.extensions.signal.impl;

public class StartGameSignal:Signal
{
}
public class CreateTeam1Signal:Signal<bool>
{
}
public class CreateTeam2Signal:Signal<int>
{
}
public class Team1PrepareAttackSignal:Signal
{
}
public class Team1StartAttackSignal:Signal<bool>
{
}
public class Team1FinishAttackSignal:Signal
{
}
public class Team2StartAttackSignal:Signal
{
}
public class Team2FinishAttackSignal:Signal
{
}
public class FinishBattleSignal:Signal
{
}
public class AddActionSignal:Signal<Action>
{
}
public class WinNodeSignal:Signal
{
}
public class LoseNodeSignal:Signal
{
}
public class OnTapSignal : Signal
{
}

public class DisableBoardInputSignal : Signal
{
}

public class EnableBoardInputSignal : Signal
{
}

public class BoardSkillSignal : Signal<BoardSkillInfo>
{
}

public class QuitDungeonSignal : Signal
{
}

public class CharacterHpChangedSignal : Signal<int, int>
{
}

public class MonsterHpChangedSignal : Signal<int, int>
{
}

public class CharacterDefendChangedSignal : Signal<int, int>
{
}

public class CharacterSkillChangedSignal : Signal<float> {
}

public class MonsterTurnChangedSignal : Signal<int>
{
}

public class RemainingMovesChangedSignal : Signal<string>
{
}

public class CharacterStartRunningSignal : Signal
{
}

public class CharacterStopRunningSignal : Signal
{
}

public class CalculatorComboSignal:Signal<bool>
{
}

public class TutorialStateSignal : Signal<int, string>
{
}