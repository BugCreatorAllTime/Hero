// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using strange.extensions.context.api;
using UnityEngine;

public abstract class NotificationService
{
	[Inject]
	public ApplicationDispatcherService appService{get;set;}

	[Inject]
	public ConfigManager configMgr {get;set;}

	public NotificationService ()
	{

	}

	[PostConstruct]
	public void Init()
	{
		appService.dispatcher.OnApplicationPauseHandler += OnPause;
		appService.dispatcher.OnApplicationQuitHandler += OnQuit;
	}

	public abstract void SendNotification(string name, string title, string body, int delay);

	void OnPause(bool pauseStatus)
	{
		UserData uData = configMgr.UserData;
		if(!uData.IsFullEnergy() && uData.canNotify)
		{
			int delay = CalculateFullEnergyTime(uData);
			SendNotification("Name","Title","You are full of energy. Get back to continue your adventure",delay);
			uData.canNotify = false;
		}
	}

	void OnQuit()
	{
		OnPause(true);
	}

	private int CalculateFullEnergyTime(UserData uData)
	{
		long ret = 0;
		long curTime = Utils.GetCurrentTimeInSecond();
		int countE = configMgr.CharacterCfg.GetMaxEnergy() - uData.energy;
		ret = uData.lastTimeUpdate + countE * configMgr.general.EnergyCooldown - curTime;
		return (int) ret;
	}
}

