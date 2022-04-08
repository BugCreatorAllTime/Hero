using System;
using System.Collections.Generic;

public class InfoUserService {

	[Inject]
	public ConfigManager config { get; set;}

	private List<InfoUserFBData> listUser = new List<InfoUserFBData>();
	private UIAtlas atlas = null;
	private int countCheckShowInfo = 0;

	public bool IsUpdateInfo()
	{
		countCheckShowInfo++;
		if(countCheckShowInfo >= config.general.NumberRequestFB)
		{
			countCheckShowInfo = 0;
			return true;
		}
		return false;
	}

	public void SetListUser(List<InfoUserFBData> listUser)
	{
		this.listUser = listUser;
	}

	public List<InfoUserFBData> GetListUser()
	{
		return this.listUser;
	}

	public UIAtlas GetAtlas()
	{
		return this.atlas;
	}

	public void SetAtlas(UIAtlas atlas)
	{
		this.atlas = atlas;
	}

	public void DestroyAtlas()
	{
		if(atlas != null)
			NGUITools.Destroy (atlas.gameObject);
	}
}
