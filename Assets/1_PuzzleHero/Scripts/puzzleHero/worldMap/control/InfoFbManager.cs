using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using strange.extensions.pool.api;
using System.Linq;

public class InfoFbManager {

	[Inject]
	public AssetMgr mgr{get; set;}

	[Inject]
	public InfoUserService infoUserService { get; set;}

	[Inject]
	public FbHandler fbHandler { get; set;}

	[Inject]
	public NoteDungeonManager noteManager { get; set;}

	private List<InfoUserFBData> listUser;

	[PostConstruct]
	public void PostConstruct()
	{
		listUser = infoUserService.GetListUser ();
	}

	public void SetInfoUserToMap()
	{
		listUser = infoUserService.GetListUser ();
		SetInfoDungeon ();
	}

	private void SetInfoDungeon()
	{
		noteManager.SetListUserFaceBook (listUser);
	}


	public void HideUser()
	{
		noteManager.HideUserFaceBook ();
	}

	public void AddCountToCheckUpdateInfoFriend()
	{
		if(infoUserService.IsUpdateInfo() && FB.IsLoggedIn)
		{
			HideUser();
			fbHandler.QueryScores();
		}
	}
}
