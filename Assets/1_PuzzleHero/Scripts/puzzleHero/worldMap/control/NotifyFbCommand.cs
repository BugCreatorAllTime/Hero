using UnityEngine;
using System.Collections;
using strange.extensions.command.impl;
using strange.examples.strangerocks;

public class NotifyFbCommand : Command {

	[Inject]
	public LoadInfoTab loadTab { get; set; }

	[Inject]
	public ConfigManager config { get; set;}

	[Inject]
	public WorldMapHander worldMapHander { get;set;}

	[Inject]
	public IRoutineRunner routineRunner { get; set; }

	public override void Execute ()
	{
		routineRunner.StartCoroutine(NotifyFB ());
	}

	IEnumerator NotifyFB()
	{
		yield return new WaitForSeconds(0f);
		if(!config.UserData.GetDefeat() && config.UserData.GetNotifyFB() == 1 &&
		   config.UserData.GetIconUpdate() == 0)
		{
			config.UserData.SetNotifyFB(0);
			worldMapHander.ShowNotifyFB();
		}
	}
}
