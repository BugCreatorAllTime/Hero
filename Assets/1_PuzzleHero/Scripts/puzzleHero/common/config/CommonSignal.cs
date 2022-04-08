//This file contains all the signals that are dispatched between Contexts

using System;
using strange.extensions.signal.impl;


public class StartSignal : Signal{}

public class OnLoginSignal : Signal {}

public class GoToDungeonTutorialSignal : Signal<int>{}

public class PlaySignal : Signal {}

