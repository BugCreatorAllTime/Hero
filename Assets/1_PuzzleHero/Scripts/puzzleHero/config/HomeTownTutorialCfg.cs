using System.Collections;
using System.Collections.Generic;
using System;

public class HomeTownTutorialCfg {

	public const int SET_USERNAME = 1;
	public const int TUTORIAL = 2;
	public const int POSITIONING = 3;
	public const int TUTSHOW = 4;
	public const int TUTCLICK = 5;
	public const int POSCLICK = 6;
	public const int START = -3;
	public const int IN_HOMETOWN = 8;

	public Dictionary<string, HomeTownTutorialData> homeTown = new Dictionary<string, HomeTownTutorialData>();
	
	public HomeTownTutorialData getTownTutorial (int id)
	{
		return this.homeTown [id.ToString ()];
	}
}
